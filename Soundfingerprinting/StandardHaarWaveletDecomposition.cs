﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    /// <summary>
    ///     Standart Haar wavelet decomposition algorithm.
    ///     According to Fast Multi-Resolution Image Query paper, Haar wavelet decomposition with standard basis function works
    ///     better in image querying
    /// </summary>
    /// <remarks>
    ///     Implemented according to the algorithm found here
    ///     http://grail.cs.washington.edu/projects/wavelets/article/wavelet1.pdf
    /// </remarks>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "Reviewed. Suppression is OK here.")]
    public class StandardHaarWaveletDecomposition : HaarWaveletDecomposition
    {
        #region IWaveletDecomposition Members

        /// <summary>
        ///     Apply Haar Wavelet decomposition on the image
        /// </summary>
        /// <param name="image">Image to be decomposed</param>
        public override void DecomposeImageInPlace(double[][] image)
        {
            DecomposeImage(image);
        }

        #endregion

        private void Decomposition(double[] array)
        {
            var h = array.Length;
            for (var i = 0; i < h; i++) array[i] /= Math.Sqrt(h);

            while (h > 1)
            {
                DecompositionStep(array, h);
                h /= 2;
            }
        }

        /// <summary>
        ///     The standard 2-dimensional Haar wavelet decomposition involves one-dimensional decomposition of each row followed
        ///     by a one-dimensional decomposition of each column of the result.
        /// </summary>
        /// <param name="image">Image to be decomposed</param>
        private void DecomposeImage(double[][] image)
        {
            var rows = image.Length; /*128*/
            var cols = image[0].Length; /*32*/

            // The order of decomposition is reversed because the image is 128x32 but we consider it reversed 32x128
            for (var col = 0; col < cols /*32*/; col++)
            {
                var column = new double[rows]; /*Length of each column is equal to number of rows*/
                for (var row = 0; row < rows; row++) column[row] = image[row][col]; /*Copying Column vector*/

                Decomposition(column); /*Decomposition of each row*/
                for (var row = 0; row < rows; row++) image[row][col] = column[row];
            }

            for (var row = 0; row < rows /*128*/; row++) Decomposition(image[row]); /*Decomposition of each row*/
        }
    }
}