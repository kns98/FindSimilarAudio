using System;
using CommonUtils;
using Mirage;
using Wavelets;
using Wavelets.Compress;
using Matrix = Comirva.Audio.Util.Maths.Matrix;

/// <summary>
/// Mfcc method copied from the Mirage project:
/// Mirage - High Performance Music Similarity and Automatic Playlist Generator
/// http://hop.at/mirage
///
/// Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
/// Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
///
/// This program is free software; you can redistribute it and/or
/// modify it under the terms of the GNU General Public License
/// as published by the Free Software Foundation; either version 2
/// of the License, or (at your option) any later version.
/// </summary>
namespace Comirva.Audio
{
    public class MfccMirage
    {
        public Matrix dct;
        public Matrix filterWeights;
        private readonly int[] melScaleFreqsIndex; // store the mel scale indexes
        private readonly double[] melScaleTriangleHeights; // store the mel filter triangle heights

        private readonly int numberCoefficients; // number of MFCC COEFFICIENTS. E.g. 20
        private readonly int numberWaveletTransforms = 2; // number of wavelet transform iterations, 3?

        /// <summary>
        ///     Create a Mfcc object
        ///     This method is not optimized in the sense that the Mel Filter Bands
        ///     and the DCT is created here (and not read in)
        ///     Only support an overlap of half the window size
        /// </summary>
        /// <param name="winsize">window size</param>
        /// <param name="srate">sample rate</param>
        /// <param name="numberFilters">number of filters (MEL COEFFICIENTS). E.g. 36 (SPHINX-III uses 40)</param>
        /// <param name="numberCoefficients">number of MFCC COEFFICIENTS. E.g. 20</param>
        public MfccMirage(int winsize, int srate, int numberFilters, int numberCoefficients)
        {
            this.numberCoefficients = numberCoefficients;

            var mel = new double[srate / 2 - 19];
            var freq = new double[srate / 2 - 19];
            var startFreq = 20;

            // Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
            for (var f = startFreq; f <= srate / 2; f++)
            {
                mel[f - startFreq] = LinearToMel(f);
                freq[f - startFreq] = f;
            }

            // Prepare filters
            var freqs = new double[numberFilters + 2];
            melScaleFreqsIndex = new int[numberFilters + 2];

            for (var f = 0; f < freqs.Length; f++)
            {
                var melIndex = 1.0 + (mel[mel.Length - 1] - 1.0) /
                    (freqs.Length - 1.0) * f;
                var min = Math.Abs(mel[0] - melIndex);
                freqs[f] = freq[0];

                for (var j = 1; j < mel.Length; j++)
                {
                    var cur = Math.Abs(mel[j] - melIndex);
                    if (cur < min)
                    {
                        min = cur;
                        freqs[f] = freq[j];
                    }
                }

                melScaleFreqsIndex[f] = MathUtils.FreqToIndex(freqs[f], srate, winsize);
            }

            // triangle heights
            melScaleTriangleHeights = new double[numberFilters];
            for (var j = 0; j < melScaleTriangleHeights.Length; j++)
                melScaleTriangleHeights[j] = 2.0 / (freqs[j + 2] - freqs[j]);

            var fftFreq = new double[winsize / 2 + 1];
            for (var j = 0; j < fftFreq.Length; j++) fftFreq[j] = srate / 2 / (fftFreq.Length - 1.0) * j;

            // Compute the MFCC filter Weights
            filterWeights = new Matrix(numberFilters, winsize / 2);
            for (var j = 0; j < numberFilters; j++)
            for (var k = 0; k < fftFreq.Length; k++)
            {
                if (fftFreq[k] > freqs[j] && fftFreq[k] <= freqs[j + 1])
                    filterWeights.MatrixData[j][k] = (float)(melScaleTriangleHeights[j] *
                                                             ((fftFreq[k] - freqs[j]) / (freqs[j + 1] - freqs[j])));
                if (fftFreq[k] > freqs[j + 1] &&
                    fftFreq[k] < freqs[j + 2])
                    filterWeights.MatrixData[j][k] += (float)(melScaleTriangleHeights[j] *
                                                              ((freqs[j + 2] - fftFreq[k]) /
                                                               (freqs[j + 2] - freqs[j + 1])));
            }
#if DEBUG
            if (Analyzer.DEBUG_INFO_VERBOSE)
                if (Analyzer.DEBUG_OUTPUT_TEXT)
                    filterWeights.WriteAscii("melfilters-mirage-orig.ascii");
            //filterWeights.DrawMatrixGraph("melfilters-mirage-orig.png");
#endif

            // Compute the DCT
            // This whole section is copied from GetDCTMatrix() from CoMirva package
            dct = new DctComirva(numberCoefficients, numberFilters).DCTMatrix;

#if DEBUG
            if (Analyzer.DEBUG_INFO_VERBOSE)
                if (Analyzer.DEBUG_OUTPUT_TEXT)
                    dct.WriteAscii("dct-mirage-orig.ascii");
            //dct.DrawMatrixGraph("dct-mirage-orig.png");
#endif
        }

        /// <summary>
        ///     Apply internal DCT and Mel Filterbands
        ///     This method is faster than ApplyComirvaWay since it uses fewer loops.
        /// </summary>
        /// <param name="m">matrix (stftdata)</param>
        /// <returns>matrix mel scaled and dct'ed</returns>
        public Matrix ApplyMelScaleDCT(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            // 4. Mel Scale Filterbank
            // Mel-frequency is proportional to the logarithm of the linear frequency,
            // reflecting similar effects in the human's subjective aural perception)
            var mel = filterWeights * m;

            // 5. Take Logarithm
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = mel.MatrixData[i][j] < 1.0 ? 0 : 20.0 * Math.Log10(mel.MatrixData[i][j]);

            // 6. DCT (Discrete Cosine Transform)
            var mfcc = dct * mel;

            Dbg.WriteLine("mfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return mfcc;
        }

        /// <summary>
        ///     Apply internal DCT and Mel Filterbands utilising the Comirva Matrix methods
        /// </summary>
        /// <param name="m">matrix (stftdata)</param>
        /// <returns>matrix mel scaled and dct'ed</returns>
        public Matrix ApplyMelScaleDCTComirva(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            // 4. Mel Scale Filterbank
            // Mel-frequency is proportional to the logarithm of the linear frequency,
            // reflecting similar effects in the human's subjective aural perception)
            m = filterWeights * m;

            // 5. Take Logarithm
            // to db
            var log10 = 20 * (1 / Math.Log(10)); // log for base 10 and scale by factor 10
            m.ThrunkAtLowerBoundary(1);
            m.LogEquals();
            m *= log10;

            // 6. DCT (Discrete Cosine Transform)
            m = dct * m;

            Dbg.WriteLine("mfcc (MfccMirage-ComirvaWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Perform an inverse mfcc. E.g. perform an idct and inverse Mel Filterbands and return stftdata
        /// </summary>
        /// <param name="mfcc">mfcc matrix</param>
        /// <returns>matrix idct'ed and mel removed (e.g. stftdata)</returns>
        public Matrix InverseMelScaleDCT(ref Matrix mfcc)
        {
            var t = new DbgTimer();
            t.Start();

            // 6. Perform the IDCT (Inverse Discrete Cosine Transform)
            var mel = dct.Transpose() * mfcc;

            // 5. Take Inverse Logarithm
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 20) / melScaleTriangleHeights[0];

            // 4. Inverse Mel Scale using interpolation
            // i.e. from e.g.
            // mel=Rows: 40, Columns: 165 (average freq, time slice)
            // to
            // m=Rows: 1024, Columns: 165 (freq, time slice)
            //Matrix m = filterWeights.Transpose() * mel;
            var m = new Matrix(filterWeights.Columns, mel.Columns);
            InverseMelScaling(mel, m);

            Dbg.WriteLine("imfcc (MfccMirage-MirageWay) Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     DCT
        /// </summary>
        /// <param name="m">matrix (logSpectrogram)</param>
        /// <returns>matrix dct'ed</returns>
        public Matrix ApplyDCT(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            // 6. DCT (Discrete Cosine Transform)
            m = dct * m;

            Dbg.WriteLine("ApplyDCT Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Perform an inverse DCT
        /// </summary>
        /// <param name="mfcc">dct matrix</param>
        /// <returns>matrix idct'ed (e.g. logSpectrogram)</returns>
        public Matrix InverseDCT(ref Matrix input)
        {
            var t = new DbgTimer();
            t.Start();

            // 6. Perform the IDCT (Inverse Discrete Cosine Transform)
            var m = dct.Transpose() * input;

            Dbg.WriteLine("InverseDCT Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Mel Scale Haar Wavelet Transform
        /// </summary>
        /// <param name="m">matrix (stftdata)</param>
        /// <returns>matrix mel scaled and wavelet'ed</returns>
        public Matrix ApplyMelScaleWaveletPadding(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            // 4. Mel Scale Filterbank
            // Mel-frequency is proportional to the logarithm of the linear frequency,
            // reflecting similar effects in the human's subjective aural perception)
            var mel = filterWeights * m;

            // 5. Take Logarithm
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = mel.MatrixData[i][j] < 1.0 ? 0 : 20.0 * Math.Log10(mel.MatrixData[i][j]);

            // 6. Wavelet Transform
            // make sure the matrix is square before transforming (by zero padding)
            Matrix resizedMatrix;
            if (!mel.IsSymmetric() || !MathUtils.IsPowerOfTwo(mel.Rows))
            {
                var size = mel.Rows > mel.Columns ? mel.Rows : mel.Columns;
                var sizePow2 = MathUtils.NextPowerOfTwo(size);
                resizedMatrix = mel.Resize(sizePow2, sizePow2);
            }
            else
            {
                resizedMatrix = mel;
            }

            var wavelet = WaveletUtils.HaarWaveletTransform(resizedMatrix.MatrixData, true);

            Dbg.WriteLine("Wavelet Mel Scale And Wavelet Compression Padding - Execution Time: " +
                          t.Stop().TotalMilliseconds + " ms");
            return wavelet;
        }

        /// <summary>
        ///     Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and
        ///     return stftdata
        /// </summary>
        /// <param name="wavelet">wavelet matrix</param>
        /// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
        public Matrix InverseMelScaleWaveletPadding(ref Matrix wavelet)
        {
            var t = new DbgTimer();
            t.Start();

            // 6. Perform the Inverse Wavelet Transform
            var mel = WaveletUtils.InverseHaarWaveletTransform(wavelet.MatrixData);

            // Resize (remove padding)
            mel = mel.Resize(melScaleFreqsIndex.Length - 2, wavelet.Columns);

            // 5. Take Inverse Logarithm
            // Divide with first triangle height in order to scale properly
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 20) / melScaleTriangleHeights[0];

            // 4. Inverse Mel Scale using interpolation
            // i.e. from e.g.
            // mel=Rows: 40, Columns: 165 (average freq, time slice)
            // to
            // m=Rows: 1024, Columns: 165 (freq, time slice)
            //Matrix m = filterWeights.Transpose() * mel;
            var m = new Matrix(filterWeights.Columns, mel.Columns);
            InverseMelScaling(mel, m);

            Dbg.WriteLine("Inverse Mel Scale And Wavelet Compression Padding - Execution Time: " +
                          t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Haar Wavelet Transform and Compress
        /// </summary>
        /// <param name="m">matrix (logSpectrogram)</param>
        /// <returns>matrix wavelet'ed</returns>
        public Matrix ApplyWaveletCompression(ref Matrix m, out int lastHeight, out int lastWidth)
        {
            var t = new DbgTimer();
            t.Start();

            // Wavelet Transform
            var wavelet = m.Copy();
            WaveletCompress.HaarTransform2D(wavelet.MatrixData, numberWaveletTransforms, out lastHeight, out lastWidth);

            // Compress
            var waveletCompressed = wavelet.Resize(numberCoefficients, wavelet.Columns);

            Dbg.WriteLine("Wavelet Compression - Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return waveletCompressed;
        }

        /// <summary>
        ///     Perform an inverse decompressed haar wavelet transform. E.g. perform an ihaar2d and return logSpectrogram
        /// </summary>
        /// <param name="wavelet">wavelet matrix</param>
        /// <returns>matrix inverse wavelet'ed (e.g. logSpectrogram)</returns>
        public Matrix InverseWaveletCompression(ref Matrix wavelet, int firstHeight, int firstWidth, int rows,
            int columns)
        {
            var t = new DbgTimer();
            t.Start();

            // Resize, e.g. Uncompress
            wavelet = wavelet.Resize(rows, columns);

            // 6. Perform the Inverse Wavelet Transform
            var m = wavelet.Copy();
            WaveletDecompress.Decompress2D(m.MatrixData, numberWaveletTransforms, firstHeight, firstWidth);

            Dbg.WriteLine("Inverse Wavelet Compression - Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Mel Scale Haar Wavelet Transform and Compress
        /// </summary>
        /// <param name="m">matrix (stftdata)</param>
        /// <returns>matrix mel scaled and wavelet'ed</returns>
        public Matrix ApplyMelScaleAndWaveletCompress(ref Matrix m, out int lastHeight, out int lastWidth)
        {
            var t = new DbgTimer();
            t.Start();

            // 4. Mel Scale Filterbank
            // Mel-frequency is proportional to the logarithm of the linear frequency,
            // reflecting similar effects in the human's subjective aural perception)
            var mel = filterWeights * m;

            // 5. Take Logarithm
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = mel.MatrixData[i][j] < 1.0 ? 0 : 20.0 * Math.Log10(mel.MatrixData[i][j]);

            // 6. Perform the Wavelet Transform and Compress
            var waveletCompressed = ApplyWaveletCompression(ref mel, out lastHeight, out lastWidth);

            Dbg.WriteLine("Mel Scale And Wavelet Compression - Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return waveletCompressed;
        }

        /// <summary>
        ///     Perform an inverse haar wavelet mel scaled transform. E.g. perform an ihaar2d and inverse Mel Filterbands and
        ///     return stftdata
        /// </summary>
        /// <param name="wavelet">wavelet matrix</param>
        /// <returns>matrix inverse wavelet'ed and mel removed (e.g. stftdata)</returns>
        public Matrix InverseMelScaleAndWaveletCompress(ref Matrix wavelet, int firstHeight, int firstWidth)
        {
            var t = new DbgTimer();
            t.Start();

            // 6. Ucompress and then perform the Inverse Wavelet Transform
            var mel = InverseWaveletCompression(ref wavelet, firstHeight, firstWidth, melScaleFreqsIndex.Length - 2,
                wavelet.Columns);

            // 5. Take Inverse Logarithm
            // Divide with first triangle height in order to scale properly
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 20) / melScaleTriangleHeights[0];

            // 4. Inverse Mel Scale using interpolation
            // i.e. from e.g.
            // mel=Rows: 40, Columns: 165 (average freq, time slice)
            // to
            // m=Rows: 1024, Columns: 165 (freq, time slice)
            //Matrix m = filterWeights.Transpose() * mel;
            var m = new Matrix(filterWeights.Columns, mel.Columns);
            InverseMelScaling(mel, m);

            Dbg.WriteLine("Inverse Mel Scale and Wavelet Compression - Execution Time: " + t.Stop().TotalMilliseconds +
                          " ms");
            return m;
        }

        /// <summary>
        ///     Mel Scale and Log
        /// </summary>
        /// <param name="m">matrix (stftdata)</param>
        /// <returns>matrix mel scaled</returns>
        public Matrix ApplyMelScaleAndLog(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            // 4. Mel Scale Filterbank
            // Mel-frequency is proportional to the logarithm of the linear frequency,
            // reflecting similar effects in the human's subjective aural perception)
            var mel = filterWeights * m;

            // 5. Take Logarithm
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = mel.MatrixData[i][j] < 1.0 ? 0 : 20.0 * Math.Log10(mel.MatrixData[i][j]);
            //mel.MatrixData[i][j] = 20.0 * Math.Log10(mel.MatrixData[i][j]);

            Dbg.WriteLine("Mel Scale And Log - Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return mel;
        }

        /// <summary>
        ///     Perform an inverse mel scale and log.
        /// </summary>
        /// <param name="wavelet">mel scaled matrix</param>
        /// <returns>matrix mel removed and un-logged (e.g. stftdata)</returns>
        public Matrix InverseMelScaleAndLog(ref Matrix mel)
        {
            var t = new DbgTimer();
            t.Start();

            // 5. Take Inverse Logarithm
            // Divide with first triangle height in order to scale properly
            for (var i = 0; i < mel.Rows; i++)
            for (var j = 0; j < mel.Columns; j++)
                mel.MatrixData[i][j] = Math.Pow(10, mel.MatrixData[i][j] / 20) / melScaleTriangleHeights[0];

            // 4. Inverse Mel Scale using interpolation
            // i.e. from e.g.
            // mel=Rows: 40, Columns: 165 (average freq, time slice)
            // to
            // m=Rows: 1024, Columns: 165 (freq, time slice)
            //Matrix m = filterWeights.Transpose() * mel;
            var m = new Matrix(filterWeights.Columns, mel.Columns);
            InverseMelScaling(mel, m);

            Dbg.WriteLine("Inverse Mel Scale And Log - Execution Time: " + t.Stop().TotalMilliseconds + " ms");
            return m;
        }

        /// <summary>
        ///     Perform an inverse mel scale using interpolation
        ///     i.e. from e.g.
        ///     mel=Rows: 40, Columns: 165 (average freq, time slice)
        ///     to
        ///     m=Rows: 1024, Columns: 165 (freq, time slice)
        /// </summary>
        /// <param name="mel"></param>
        /// <param name="m"></param>
        private void InverseMelScaling(Matrix mel, Matrix m)
        {
            // for each row, interpolate values to next row according to mel scale
            for (var j = 0; j < mel.Columns; j++)
            {
                for (var i = 0; i < mel.Rows - 1; i++)
                {
                    var startValue = mel.MatrixData[i][j];
                    var endValue = mel.MatrixData[i + 1][j];

                    // what indexes in resulting matrix does this row cover?
                    //Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", i, freqsIndex[i+1], freqsIndex[i+2]-1, startValue, endValue);

                    // add interpolated values
                    AddInterpolatedValues(m, melScaleFreqsIndex[i + 1], melScaleFreqsIndex[i + 2], startValue, endValue,
                        j);
                }

                // last row
                var iLast = mel.Rows - 1;
                var startValueLast = mel.MatrixData[iLast][j];
                var endValueLast = 0.0; // mel.MatrixData[iLast][j];

                // what indexes in resulting matrix does this row cover?
                //Console.Out.WriteLine("Mel Row index {0} corresponds to Linear Row index {1} - {2} [{3:0.00} - {4:0.00}]", iLast, freqsIndex[iLast+1], freqsIndex[iLast+2]-1, startValueLast, endValueLast);

                // add interpolated values
                AddInterpolatedValues(m, melScaleFreqsIndex[iLast + 1], melScaleFreqsIndex[iLast + 2], startValueLast,
                    endValueLast, j);
            }
        }

        private void AddInterpolatedValues(Matrix m, int startIndex, int endIndex, double startValue, double endValue,
            int columnIndex)
        {
            // interpolate and add values
            var partSteps = endIndex - startIndex;
            for (var step = 0; step < partSteps; step++)
            {
                var p = step / (double)partSteps;

                // interpolate
                var val = MathUtils.Interpolate(startValue, endValue, p);

                // add to matrix data
                m.MatrixData[startIndex + step][columnIndex] = val;
            }
        }

        #region Mel scale to Linear and Linear to Mel scale

        /// <summary>
        ///     Converts frequency from linear to Mel scale.
        ///     Mel-frequency is proportional to the logarithm of the linear frequency,
        ///     reflecting similar effects in the human's subjective aural perception)
        /// </summary>
        /// <param name="lFrequency">lFrequency frequency in linear scale</param>
        /// <returns>frequency in Mel scale</returns>
        public static double LinearToMel(double lFrequency)
        {
            return 1127.01048 * Math.Log(1.0 + lFrequency / 700.0);
        }

        /// <summary>
        ///     Converts frequency from Mel to linear scale.
        ///     Mel-frequency is proportional to the logarithm of the linear frequency,
        ///     reflecting similar effects in the human's subjective aural perception)
        /// </summary>
        /// <param name="mFrequency">frequency in Mel scale</param>
        /// <returns>frequency in linear scale</returns>
        public static double MelToLinear(double mFrequency)
        {
            return 700.0 * (Math.Exp(mFrequency / 1127.01048) - 1);
        }

        #endregion
    }
}