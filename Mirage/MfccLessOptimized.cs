/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;

namespace Mirage
{
    public class MfccLessOptimized
    {
        private readonly Matrix dct;
        private readonly Matrix filterWeights;

        /// <summary>
        ///     Create a Mfcc object
        ///     This method is not optimized in the sense that the Mel Filter Bands
        ///     and the DCT is created here (and not read in)
        /// </summary>
        /// <param name="winsize">window size</param>
        /// <param name="srate">sample rate</param>
        /// <param name="filters">number of filters (MEL COEFFICIENTS). E.g. 36 (SPHINX-III uses 40)</param>
        /// <param name="cc">number of MFCC COEFFICIENTS</param>
        public MfccLessOptimized(int winsize, int srate, int numberFilters, int numberCoefficients)
        {
            var mel = new double[srate / 2 - 19];
            var freq = new double[srate / 2 - 19];
            var startFreq = 20;

            // Mel Scale from StartFreq to SamplingRate/2, step every 1Hz
            for (var f = startFreq; f <= srate / 2; f++)
            {
                mel[f - startFreq] = Math.Log(1.0 + f / 700.0) * 1127.01048;
                freq[f - startFreq] = f;
            }

            // Prepare filters
            var freqs = new double[numberFilters + 2];

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
            }

            var triangleh = new double[numberFilters];
            for (var j = 0; j < triangleh.Length; j++) triangleh[j] = 2.0 / (freqs[j + 2] - freqs[j]);

            var fftFreq = new double[winsize / 2 + 1];
            for (var j = 0; j < fftFreq.Length; j++) fftFreq[j] = srate / 2 / (fftFreq.Length - 1.0) * j;

            // Compute the MFCC filter Weights
            filterWeights = new Matrix(numberFilters, winsize / 2 + 1);
            for (var j = 0; j < numberFilters; j++)
            for (var k = 0; k < fftFreq.Length; k++)
            {
                if (fftFreq[k] > freqs[j] && fftFreq[k] <= freqs[j + 1])
                    filterWeights.d[j, k] = (float)(triangleh[j] *
                                                    ((fftFreq[k] - freqs[j]) / (freqs[j + 1] - freqs[j])));
                if (fftFreq[k] > freqs[j + 1] &&
                    fftFreq[k] < freqs[j + 2])
                    filterWeights.d[j, k] += (float)(triangleh[j] *
                                                     ((freqs[j + 2] - fftFreq[k]) / (freqs[j + 2] - freqs[j + 1])));
            }
#if DEBUG
            if (Analyzer.DEBUG_INFO_VERBOSE) filterWeights.DrawMatrixGraph("melfilters-mirage-lessoptimized.png");
#endif

            // Compute the DCT
            // This whole section is copied from GetDCTMatrix() from CoMirva package
            dct = new Matrix(numberCoefficients, numberFilters);

            // compute constants
            var k1 = Math.PI / numberFilters;
            var w1 = 1.0 / Math.Sqrt(numberFilters);
            var w2 = Math.Sqrt(2.0 / numberFilters);

            //generate dct matrix
            for (var i = 0; i < numberCoefficients; i++)
            for (var j = 0; j < numberFilters; j++)
                if (i == 0)
                    dct.d[i, j] = (float)(w1 * Math.Cos(k1 * i * (j + 0.5d)));
                else
                    dct.d[i, j] = (float)(w2 * Math.Cos(k1 * i * (j + 0.5d)));
#if DEBUG
            if (Analyzer.DEBUG_INFO_VERBOSE) dct.DrawMatrixGraph("dct-mirage-lessoptimized.png");
#endif
        }

        public Matrix Apply(ref Matrix m)
        {
            var t = new DbgTimer();
            t.Start();

            var mel = new Matrix(filterWeights.rows, m.columns);

            /*
            // Performance optimization of ...
            mel = filterWeights.Multiply(m);
            for (int i = 0; i < mel.rows; i++) {
                for (int j = 0; j < mel.columns; j++) {
                    mel.d[i, j] = (mel.d[i, j] < 1.0f ? 0 : (float)(10.0 * Math.Log10(mel.d[i, j])));
                    //mel.d[i, j] = (float)(10.0 * Math.Log10(mel.d[i, j]));
                }
            }
             */

            var mc = m.columns;
            var mr = m.rows;
            var melcolumns = mel.columns;
            var fwc = filterWeights.columns;
            var fwr = filterWeights.rows;

            unsafe
            {
                fixed (float* md = m.d, fwd = filterWeights.d, meld = mel.d)
                {
                    for (var i = 0; i < mc; i++)
                    for (var k = 0; k < fwr; k++)
                    {
                        var idx = k * melcolumns + i;
                        var kfwc = k * fwc;

                        for (var j = 0; j < mr; j++) meld[idx] += fwd[kfwc + j] * md[j * mc + i];

                        meld[idx] = meld[idx] < 1.0f ? 0 : (float)(10.0 * Math.Log10(meld[idx]));
                    }
                }
            }

            var mfcc = dct.Multiply(mel);

            Dbg.WriteLine("mfcc (MfccLessOptimized) Execution Time: " + t.Stop().TotalMilliseconds + " ms");

            return mfcc;
        }
    }
}