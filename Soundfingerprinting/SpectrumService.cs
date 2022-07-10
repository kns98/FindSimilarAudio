using System;
using System.Collections.Generic;
using Soundfingerprinting.Audio.Strides;

namespace Soundfingerprinting.Fingerprinting.FFT
{
    public class SpectrumService
    {
        /// <summary>
        ///     Cut logarithmized spetrum to spectral images
        /// </summary>
        /// <param name="logarithmizedSpectrum">Logarithmized spectrum of the initial signal</param>
        /// <param name="strideBetweenConsecutiveImages">Stride between consecutive images (static 928ms db, random 46ms query)</param>
        /// <param name="fingerprintImageLength">
        ///     Length of 1 fingerprint image (w parameter equal to 128, which alogside overlap
        ///     leads to 128*64 = 8192 = 1.48s)
        /// </param>
        /// <param name="overlap">Overlap between consecutive spectral images, taken previously (64 ~ 11.6ms)</param>
        /// <returns>List of logarithmic images</returns>
        public List<double[][]> CutLogarithmizedSpectrum(
            double[][] logarithmizedSpectrum, IStride strideBetweenConsecutiveImages, int fingerprintImageLength,
            int overlap)
        {
            var start = strideBetweenConsecutiveImages.FirstStrideSize / overlap;
            var logarithmicBins = logarithmizedSpectrum[0].Length;
            var spectralImages = new List<double[][]>();

            var width = logarithmizedSpectrum.GetLength(0);

            while (start + fingerprintImageLength < width)
            {
                var spectralImage = AllocateMemoryForFingerprintImage(fingerprintImageLength, logarithmicBins);
                for (var i = 0; i < fingerprintImageLength; i++)
                    Array.Copy(logarithmizedSpectrum[start + i], spectralImage[i], logarithmicBins);

                start += fingerprintImageLength + strideBetweenConsecutiveImages.StrideSize / overlap;
                spectralImages.Add(spectralImage);
            }

            // Make sure at least the input spectrum is a part of the output list
            if (spectralImages.Count == 0)
            {
                var spectralImage = AllocateMemoryForFingerprintImage(fingerprintImageLength, logarithmicBins);
                var rowCount = logarithmizedSpectrum.Length;
                var columnCount = logarithmizedSpectrum[0].Length;
                for (var i = 0; i < rowCount; i++)
                for (var j = 0; j < columnCount; j++)
                    spectralImage[i][j] = logarithmizedSpectrum[i][j];
                spectralImages.Add(spectralImage);
            }

            return spectralImages;
        }

        private double[][] AllocateMemoryForFingerprintImage(int fingerprintLength, int logBins)
        {
            var frames = new double[fingerprintLength][];
            for (var i = 0; i < fingerprintLength; i++) frames[i] = new double[logBins];

            return frames;
        }
    }
}