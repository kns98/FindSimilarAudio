﻿using System;
using System.Collections.Generic;
using System.IO;
using Comirva.Audio.Util.Maths;

//Copyright (c) 2011 Sebastian Böhm sebastian@sometimesfood.org
//                   Heinrich Fink hf@hfink.eu
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

namespace MatchBox
{
    /**
     * This class represents the filters necessary to warp an audio spectrum
     * into Mel-Frequency scaling. It is basically a collection of properly
     * placed TriangleFilters.
     */
    public class MelFilterBank
    {
        private readonly bool normalize_filter_area_;
        private readonly float max_freq_;
        private readonly float min_freq_;
        private readonly int num_bins_;
        private readonly int num_mel_bands_;
        private readonly int sample_rate_;

        /**
         * Creates a new MelFilterBank.
         * @param min_freq The minimum frequency in hz to be considered, i.e.
         * the left edge of the first TriangleFilter.
         * @param max_freq The maximum frequency in hz to be considered, i.e.
         * the right edge of the last TriangleFilter.
         * @param num_mel_bands The number of Mel bands to be calculated, i.e.
         * the number of TriangleFilters to be applied.
         * @param num_bins The number of bins that are present in the fft_buffer
         * that will be passed to the MelFilterBank::apply method. This is also
         * required to properly configure the TriangleFilter instances which
         * operate on array indices only.
         * @param sample_rate The original sample rate the FFT buffer which will
         * be passed to MelFilterBank::apply is based on.
         * @param normalize_filter_area If set to "true", the area of the
         * created TriangleFilter will be normalized, e.g. the height of the
         * filter's triangle shape will be configured in a way, that the area
         * of the triangle shape equals one.
         */
        public MelFilterBank(float min_freq, float max_freq, int num_mel_bands, int num_bins, int sample_rate) : this(
            min_freq, max_freq, num_mel_bands, num_bins, sample_rate, true)
        {
        }

        public MelFilterBank(float min_freq, float max_freq, int num_mel_bands, int num_bins, int sample_rate,
            bool normalize_filter_area)
        {
            min_freq_ = min_freq;
            max_freq_ = max_freq;
            num_mel_bands_ = num_mel_bands;
            num_bins_ = num_bins;
            sample_rate_ = sample_rate;
            normalize_filter_area_ = normalize_filter_area;

            //Let's do some argument checking
            if (min_freq >= max_freq || max_freq == 0)
                throw new Exception(string.Format(
                    "Invalid min/max frequencies for MelFilterBank: min = '{0}' max = '{1}'", min_freq, max_freq));

            if (num_mel_bands == 0)
                throw new Exception(string.Format("Invalid number of mel bands for MelFilterBank: n = {0}",
                    num_mel_bands));

            if (sample_rate == 0)
                throw new Exception(string.Format("Invalid sample rate for MelFilterBank: s = {0}", sample_rate));

            if (num_bins == 0)
                throw new Exception(string.Format("Invalid number of bins for MelFilterBank: s = '{0}'", num_bins));

            var delta_freq = (float)sample_rate_ / (2 * num_bins);

            var mel_min = (float)HzToMel(min_freq_);
            var mel_max = (float)HzToMel(max_freq_);

            // We divide by #band + 1 as min / max should present the beginning / end
            // of beginng up / ending low slope, i.e. it's not the centers of each
            // band that represent min/max frequency in mel bands.
            var delta_freq_mel = (mel_max - mel_min) / (num_mel_bands_ + 1);

            // Fill up equidistant spacing in mel-space
            var mel_left = mel_min;
            for (var i = 0; i < num_mel_bands_; i++)
            {
                var mel_center = mel_left + delta_freq_mel;
                var mel_right = mel_center + delta_freq_mel;

                var left_hz = (float)MelToHz(mel_left);
                var right_hz = (float)MelToHz(mel_right);

                //align to closest num_bins (round)
                var left_bin = (int)(left_hz / delta_freq + 0.5f);
                var right_bin = (int)(right_hz / delta_freq + 0.5f);

                //calculate normalized height
                var height = 1.0f;

                if (normalize_filter_area_)
                    height = 2.0f / (right_bin - left_bin);

                // Create the actual filter
                var fltr = new TriangleFilter(left_bin, right_bin, height);
                Filters.Add(fltr);

                //next left edge is current center
                mel_left = mel_center;
            }
        }

        public List<TriangleFilter> Filters { get; } = new List<TriangleFilter>();

        /// <summary>
        ///     Return the filter bank as a Matrix
        /// </summary>
        public Matrix Matrix
        {
            get
            {
                var numberFilters = Filters.Count;

                //create the filter bank matrix
                var matrix = new double[numberFilters][];

                //fill each row of the filter bank matrix with one triangular mel filter
                for (var i = 0; i < numberFilters; i++)
                {
                    var triFilter = Filters[i];
                    var leftEdge = triFilter.LeftEdge;
                    var rightEdge = triFilter.RightEdge;
                    var size = triFilter.Size;
                    var filterdata = triFilter.FilterData;

                    var filter = new double[num_bins_ + 1];

                    for (var j = 0; j < leftEdge; j++) filter[j] = 0;

                    for (var j = 0; j < size; j++) filter[j + leftEdge] = filterdata[j];

                    matrix[i] = filter;
                }

                //return the filter bank
                return new Matrix(matrix);
            }
        }

        /**
         * Apply all filters on the incoming FFT data, and write out the results
         * into an array.
         * @param fft_data The incoming FFT data on which the triangle filters
         * will be applied on.
         * @param mel_bands The caller is responsible that the passed array
         * accomodates at least num_mel_bands elements. On output this array
         * will be filled with the resulting Mel-Frqeuency warped spectrum.
         */
        public void Apply(float[] fft_data, float[] mel_bands)
        {
            // we assume the caller passes arrays with appropriates sizes
            for (var i = 0; i < num_mel_bands_; ++i)
                mel_bands[i] = Filters[i].Apply(fft_data);
        }

        /// <summary>
        ///     Utility function to convert HZ to Mel.
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        public static double HzToMel(double hz)
        {
            //melFrequency = 2595 * log(1 + linearFrequency/700)
            var ln_10 = Math.Log(10.0);
            var f = 2595.0f / ln_10;
            return f * Math.Log(1 + hz / 700.0f);
        }

        /// <summary>
        ///     Utility function to convert Mel to HZ.
        /// </summary>
        /// <param name="mel"></param>
        /// <returns></returns>
        public static double MelToHz(double mel)
        {
            var ln_10 = Math.Log(10.0f);
            var f = 2595.0f / ln_10;
            return (Math.Exp(mel / f) - 1) * 700.0f;
        }

        /// <summary>
        ///     Used for debugging. Prints out a detailed descriptions of the configured filters.
        /// </summary>
        public void Print()
        {
            Print(Console.Out);
        }

        /// <summary>
        ///     Used for debugging. Prints out a detailed descriptions of the configured filters.
        /// </summary>
        public void Print(TextWriter @out)
        {
            @out.Write("mel_filters = [");
            for (var i = 0; i < Filters.Count; ++i)
            {
                if (i != 0) @out.WriteLine(";");

                @out.Write(Filters[i]);
            }

            @out.Write("];");
            @out.WriteLine();
            @out.WriteLine();
        }
    }
}