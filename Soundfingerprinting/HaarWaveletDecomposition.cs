using System;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    public abstract class HaarWaveletDecomposition : IWaveletDecomposition
    {
        public abstract void DecomposeImageInPlace(double[][] image);

        protected void DecompositionStep(double[] array, int h)
        {
            var temp = new double[h];

            h /= 2;
            for (var i = 0; i < h; i++)
            {
                temp[i] = (array[2 * i] + array[2 * i + 1]) / Math.Sqrt(2.0);
                temp[i + h] = (array[2 * i] - array[2 * i + 1]) / Math.Sqrt(2.0);
            }

            for (var i = 0; i < h * 2; i++) array[i] = temp[i];
        }
    }
}