﻿using System.Collections.Generic;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    public interface IWaveletService
    {
        /// <summary>
        ///     Apply wavelet transform on each of the logarithmic images
        /// </summary>
        /// <param name="logarithmizedSpectrum">List of logarithmic images, taken from the signal</param>
        void ApplyWaveletTransformInPlace(List<double[][]> logarithmizedSpectrum);
    }
}