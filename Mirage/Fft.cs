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
using System.Runtime.InteropServices;
using CommonUtils.FFT;
using Lomont;

namespace Mirage
{
    /// <summary>
    ///     This class applies windowing (e.g HammingWindow, HannWindow) and then performs a Fast Fourier Transform
    ///     Float precision
    ///     Modified by perivar@nerseth.com
    /// </summary>
    public class Fft
    {
        private const uint FFTW_R2HC = 0;
        private const uint FFTW_DESTROY_INPUT = 1;
        private const uint FFTW_ESTIMATE = 64;
        private float[] data;
        private readonly float[] fft;
        private readonly int fftsize;

        private readonly IntPtr fftwData;
        private readonly IntPtr fftwPlan;
        private LomontFFT lomonFFT;
        private readonly IWindowFunction win;
        private readonly int winsize;

        public Fft(int winsize, IWindowFunction window)
        {
            this.winsize = winsize;
            fftsize = 2 * winsize;

            fftwData = fftwf_malloc(fftsize * sizeof(float));
            fftwPlan = fftwf_plan_r2r_1d(fftsize, fftwData, fftwData, FFTW_R2HC,
                FFTW_ESTIMATE | FFTW_DESTROY_INPUT);

            fft = new float[fftsize];
            window.Initialize(winsize);
            win = window;
            data = new float[fftsize];

            lomonFFT = new LomontFFT();
        }

        [DllImport("libfftw3f-3")]
        private static extern IntPtr fftwf_malloc(int size);

        [DllImport("libfftw3f-3")]
        private static extern void fftwf_free(IntPtr p);

        [DllImport("libfftw3f-3")]
        private static extern IntPtr fftwf_plan_r2r_1d(int n, IntPtr fftin,
            IntPtr fftout, uint kind, uint flags);

        [DllImport("libfftw3f-3")]
        private static extern void fftwf_destroy_plan(IntPtr plan);

        [DllImport("libfftw3f-3")]
        private static extern void fftwf_execute(IntPtr plan);

        public void ComputeMirageMatrixUsingFftw(ref Matrix m, int j, float[] audiodata, int pos)
        {
            // apply the window method (e.g HammingWindow, HannWindow etc)
            win.Apply(ref data, audiodata, pos);

            Marshal.Copy(data, 0, fftwData, fftsize);
            fftwf_execute(fftwPlan);
            Marshal.Copy(fftwData, fft, 0, fftsize);

            m.d[0, j] = (float)Math.Sqrt(fft[0] * fft[0]);
            for (var i = 1; i < winsize / 2; i++) // amplitude (or magnitude) is the square root of the power spectrum
                // the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
                // use 20*log10(Y) to get dB from amplitude
                // the power spectrum is the magnitude spectrum squared
                // use 10*log10(Y) to get dB from power spectrum
                m.d[i, j] = (float)Math.Sqrt(fft[i * 2] * fft[i * 2] +
                                             fft[fftsize - i * 2] * fft[fftsize - i * 2]);
            m.d[winsize / 2, j] = (float)Math.Sqrt(fft[winsize] * fft[winsize]);
        }

        public void ComputeComirvaMatrixUsingFftw(ref Comirva.Audio.Util.Maths.Matrix m, int j, float[] audiodata,
            int pos)
        {
            // apply the window method (e.g HammingWindow, HannWindow etc)
            win.Apply(ref data, audiodata, pos);

            Marshal.Copy(data, 0, fftwData, fftsize);
            fftwf_execute(fftwPlan);
            Marshal.Copy(fftwData, fft, 0, fftsize);

            // fft input will now contain the FFT values in a Half Complex format
            // r0, r1, r2, ..., rn/2, i(n+1)/2-1, ..., i2, i1
            // Here, rk is the real part of the kth output, and ikis the imaginary part. (Division by 2 is rounded down.)
            // For a halfcomplex array hc[n], the kth component thus has its real part in hc[k] and its imaginary part in hc[n-k],
            // with the exception of k == 0 or n/2 (the latter only if n is even)�in these two cases, the imaginary part is zero due to symmetries of the real-input DFT, and is not stored.
            m.MatrixData[0][j] = Math.Sqrt(fft[0] * fft[0]);
            for (var i = 1; i < winsize / 2; i++) // amplitude (or magnitude) is the square root of the power spectrum
                // the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
                // use 20*log10(Y) to get dB from amplitude
                // the power spectrum is the magnitude spectrum squared
                // use 10*log10(Y) to get dB from power spectrum
                m.MatrixData[i][j] = Math.Sqrt(fft[i * 2] * fft[i * 2] +
                                               fft[fftsize - i * 2] * fft[fftsize - i * 2]);
            //m.MatrixData[winsize/2][j] = Math.Sqrt(fft[winsize] * fft[winsize]);
        }

        public void ComputeComirvaMatrixUsingLomontRealFFT(ref Comirva.Audio.Util.Maths.Matrix m, int column,
            float[] audiodata, int pos)
        {
            // apply the window method (e.g HammingWindow, HannWindow etc)
            win.Apply(ref data, audiodata, pos);

            var fft = new double[data.Length / 2];
            Array.Copy(data, fft, data.Length / 2);
            lomonFFT.RealFFT(fft, true);

            // fft input will now contain the FFT values
            // r0, r(n/2), r1, i1, r2, i2 ...
            m.MatrixData[0][column] = Math.Sqrt(fft[0] * fft[0] * winsize);
            m.MatrixData[winsize / 2 - 1][column] = Math.Sqrt(fft[1] * fft[1] * winsize);
            for (var row = 1;
                 row < winsize / 2;
                 row++) // amplitude (or magnitude) is the square root of the power spectrum
                // the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
                // use 20*log10(Y) to get dB from amplitude
                // the power spectrum is the magnitude spectrum squared
                // use 10*log10(Y) to get dB from power spectrum
                m.MatrixData[row][column] = Math.Sqrt((fft[2 * row] * fft[2 * row] +
                                                       fft[2 * row + 1] * fft[2 * row + 1]) * winsize);
        }

        public void ComputeComirvaMatrixUsingLomontTableFFT(ref Comirva.Audio.Util.Maths.Matrix m, int column,
            float[] audiodata, int pos)
        {
            // apply the window method (e.g HammingWindow, HannWindow etc)
            win.Apply(ref data, audiodata, pos);

            var complexSignal = FFTUtilsLomont.FloatToComplexDouble(data);
            lomonFFT.TableFFT(complexSignal, true);

            var row = 0;
            for (var i = 0; i < complexSignal.Length / 4; i += 2)
            {
                var re = complexSignal[2 * i];
                var img = complexSignal[2 * i + 1];
                m.MatrixData[row][column] = Math.Sqrt((re * re + img * img) * complexSignal.Length / 2);
                row++;
            }
        }

        public void ComputeInverseComirvaMatrixUsingLomontRealFFT(Comirva.Audio.Util.Maths.Matrix m, int column,
            ref double[] signal, int winsize, int hopsize)
        {
            var spectrogramWindow = m.GetColumn(column);

            // extend window with the inverse duplicate array
            var len = spectrogramWindow.Length;
            var extendedWindow = new double[len * 2];
            Array.Copy(spectrogramWindow, extendedWindow, len);
            for (var i = 1; i < len; i++) extendedWindow[len + i] = spectrogramWindow[len - i];

            // ifft input must contain the FFT values
            // r0, r(n/2), r1, i1, r2, i2 ...

            // Perform the ifft and take just the real part
            var ifft = new double[winsize * 2];
            ifft[0] = extendedWindow[0];
            ifft[1] = extendedWindow[winsize / 2];
            for (var i = 1; i < extendedWindow.Length; i++) ifft[2 * i] = extendedWindow[i];

            lomonFFT.RealFFT(ifft, false);

            var window = win.GetWindow();

            // multiply by window w/ overlap-add
            var N = ifft.Length / 2;
            var returnArray = new double[N];
            for (var j = 0; j < N; j++)
            {
                var re = ifft[2 * j] / Math.Sqrt(winsize);
                returnArray[j] = re * window[j]; // smooth yet another time (also did this when doing FFT)

                // overlap-add method
                // scale with 5 just because the volume got so much lower when using a second smoothing filter when reconstrcting
                signal[j + hopsize * column] = signal[j + hopsize * column] + returnArray[j] * 5;
            }
        }

        public void ComputeInverseComirvaMatrixUsingLomontTableFFT(Comirva.Audio.Util.Maths.Matrix m, int column,
            ref double[] signal, int winsize, int hopsize)
        {
            var spectrogramWindow = m.GetColumn(column);

            // extend window with the inverse duplicate array
            var len = spectrogramWindow.Length;
            var extendedWindow = new double[len * 2];
            Array.Copy(spectrogramWindow, extendedWindow, len);
            for (var i = 1; i < len; i++) extendedWindow[len + i] = spectrogramWindow[len - i];

            var complexSignal = FFTUtilsLomont.DoubleToComplexDouble(extendedWindow);
            lomonFFT.TableFFT(complexSignal, false);

            var window = win.GetWindow();

            // multiply by window w/ overlap-add
            var N = complexSignal.Length / 2;
            var returnArray = new double[N];
            for (var j = 0; j < N; j++)
            {
                var re = complexSignal[2 * j] / Math.Sqrt(winsize);
                //double img = complexSignal[2*j + 1];
                returnArray[j] = re * window[j]; // smooth yet another time (also did this when doing FFT)

                // overlap-add method
                // scale with 2 just because the volume got so much lower when using a second smoothing filter when reconstrcting
                signal[j + hopsize * column] = signal[j + hopsize * column] + returnArray[j] * 2;
            }
        }

        ~Fft()
        {
            fftwf_destroy_plan(fftwPlan);
            fftwf_free(fftwData);
            lomonFFT = null;
        }
    }
}