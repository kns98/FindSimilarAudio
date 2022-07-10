using System;
using FFTW.NET;

// Please feel free to use/modify this class.
// If you give me credit by keeping this information or
// by sending me an email before using it or by reporting bugs , i will be happy.
// Email : gtiwari333@gmail.com,
// Blog : http://ganeshtiwaridotcomdotnp.blogspot.com/
namespace SpeechRecognitionHMM
{
    // @author Ganesh Tiwari
    public class MFCC
    {
        private double[] bin;
        internal DCT dct;

        internal FFT fft;
        private readonly double lowerFilterFreq = 80.00; // FmelLow
        private readonly int numCepstra; // number of mfcc coeffs
        private readonly int numMelFilters = 30; // how much
        private readonly double preEmphasisAlpha = 0.95;
        private readonly int samplePerFrame;
        private readonly double samplingRate;
        private readonly double upperFilterFreq;

        public MFCC(int samplePerFrame, int samplingRate, int numCepstra)
        {
            this.samplePerFrame = samplePerFrame;
            this.samplingRate = samplingRate;
            this.numCepstra = numCepstra;
            upperFilterFreq = samplingRate / 2.0;

            fft = new FFT();
            dct = new DCT(this.numCepstra, numMelFilters);
        }

        public double[] doMFCC(float[] framedSignal)
        {
            // Magnitude Spectrum
            bin = MagnitudeSpectrum(framedSignal);
            framedSignal = PreEmphasis(framedSignal);

            // cbin=frequencies of the channels in terms of FFT bin indices (cbin[i]
            // for the i -th channel)

            // prepare filter for for melFilter
            var cbin = FftBinIndices(); // same for all

            // process Mel Filterbank
            var fbank = MelFilter(bin, cbin);

            // magnitudeSpectrum and bin filter indices

            // Console.Out.WriteLine("after mel filter");
            // ArrayWriter.printDoubleArrayToConole(fbank);

            // Non-linear transformation
            var f = NonLinearTransformation(fbank);

            // Console.Out.WriteLine("after N L T");
            // ArrayWriter.printDoubleArrayToConole(f);

            // Cepstral coefficients, by DCT
            var cepc = dct.PerformDCT(f);

            // Console.Out.WriteLine("after DCT");
            // ArrayWriter.printDoubleArrayToConole(cepc);
            return cepc;
        }

        private double[] MagnitudeSpectrum(float[] frame)
        {
            var complexSize = (frame.Length >> 1) + 1;

            using (var pinIn = new PinnedArray<double>(frame))
            using (var output = new FftwArrayComplex(complexSize))
            {
                DFT.FFT(pinIn, output);

                var result = new double[output.Length];

                for (var i = 0; i < output.Length; i++) result[i] = output[i].Real;

                return result;
            }

            /*
            // prepare the input arrays
            FFTW.NET.FftwArrayDouble fftwInput = new FFTW.NET.FftwArrayDouble(frame);
            FFTW.NET.FftwArrayComplex fftwOutput = new FFTW.NET.FftwArrayComplex(complexSize);
            FFTW.NET.DFT.FFT(fftwInput, fftwOutput);
            double[] magSpectrum = fftwOutput.Abs;
        
            double[] magSpectrum = new double[frame.Length];

            // calculate FFT for current frame
            fft.ComputeFFT(frame);
            
            // System.err.println("FFT SUCCEED");
            // calculate magnitude spectrum
            for (int k = 0; k < frame.Length; k++)
            {
                magSpectrum[k] = Math.Sqrt(fft.real[k] * fft.real[k] + fft.imag[k] * fft.imag[k]);
            }
             
            return magSpectrum;
            */
        }

        // emphasize high freq signal
        // @param inputSignal
        // @return
        private float[] PreEmphasis(float[] inputSignal)
        {
            // System.err.println(" inside pre Emphasis");
            var outputSignal = new float[inputSignal.Length];

            // apply pre-emphasis to each sample
            for (var n = 1; n < inputSignal.Length; n++)
                outputSignal[n] = (float)(inputSignal[n] - preEmphasisAlpha * inputSignal[n - 1]);
            return outputSignal;
        }

        private int[] FftBinIndices()
        {
            var cbin = new int[numMelFilters + 2];
            cbin[0] = (int)Math.Round(lowerFilterFreq / samplingRate * samplePerFrame); // cbin0
            cbin[cbin.Length - 1] = samplePerFrame / 2; // cbin24
            for (var i = 1; i <= numMelFilters; i++) // from cbin1 to cbin23
            {
                var fc = CenterFreq(i); // center freq for i th filter
                cbin[i] = (int)Math.Round(fc / samplingRate * samplePerFrame);
            }

            return cbin;
        }

        // performs mel filter operation
        // @param bin magnitude spectrum (| |)^2 of fft
        // @param cbin mel filter coeffs
        // @return mel filtered coeffs--> filter bank coefficients.
        private double[] MelFilter(double[] bin, int[] cbin)
        {
            var temp = new double[numMelFilters + 2];
            for (var k = 1; k <= numMelFilters; k++)
            {
                double num1 = 0.0, num2 = 0.0;
                for (var i = cbin[k - 1]; i <= cbin[k]; i++)
                    // Console.Out.WriteLine("Inside MelFilter loop 1");
                    num1 += (i - cbin[k - 1] + 1) / (cbin[k] - cbin[k - 1] + 1) * bin[i];

                for (var i = cbin[k] + 1; i <= cbin[k + 1]; i++)
                    // Console.Out.WriteLine("Inside MelFilter loop 2");
                    num2 += (1 - (i - cbin[k]) / (cbin[k + 1] - cbin[k] + 1)) * bin[i];

                temp[k] = num1 + num2;
            }

            var fbank = new double[numMelFilters];
            for (var i = 0; i < numMelFilters; i++)
                fbank[i] = temp[i + 1];
            // Console.Out.WriteLine(fbank[i]);
            return fbank;
        }

        // performs nonlinear transformation
        // @param fbank
        // @return f log of filter bac
        private double[] NonLinearTransformation(double[] fbank)
        {
            var f = new double[fbank.Length];
            const double FLOOR = -50;
            for (var i = 0; i < fbank.Length; i++)
            {
                f[i] = Math.Log(fbank[i]);

                // check if ln() returns a value less than the floor
                if (f[i] < FLOOR) f[i] = FLOOR;
            }

            return f;
        }

        private double CenterFreq(int i)
        {
            double melFLow, melFHigh;
            melFLow = FreqToMel(lowerFilterFreq);
            melFHigh = FreqToMel(upperFilterFreq);
            var temp = melFLow + (melFHigh - melFLow) / (numMelFilters + 1) * i;
            return InverseMel(temp);
        }

        private double InverseMel(double x)
        {
            var temp = Math.Pow(10, x / 2595) - 1;
            return 700 * temp;
        }

        protected double FreqToMel(double freq)
        {
            return 2595 * Log10(1 + freq / 700);
        }

        private double Log10(double value)
        {
            return Math.Log(value) / Math.Log(10);
        }
    }
}