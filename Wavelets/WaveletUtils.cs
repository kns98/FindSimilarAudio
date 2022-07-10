using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Comirva.Audio.Util.Maths;
using CommonUtils;
using math.transform.jwave;
using math.transform.jwave.handlers;
using math.transform.jwave.handlers.wavelets;
using TestSimpleRNG;
using Wavelets.Compress;

namespace Wavelets
{
    public enum WaveletMethod
    {
        Dwt = 1,
        Haar = 2,
        HaarTransformTensor = 3,
        HaarWaveletDecompositionTensor = 4,
        HaarWaveletDecomposition = 5,
        NonStandardHaarWaveletDecomposition = 6,
        JWaveTensor = 7,
        HaarWaveletCompress = 8
    }

    /// <summary>
    ///     Description of WaveletUtils.
    /// </summary>
    public static class WaveletUtils
    {
        /// <summary>
        ///     Haar Transform of a 2D image to a Matrix.
        ///     This is using the tensor product layout.
        ///     Performance is also quite fast.
        ///     Note that the input array must be a square matrix of dimension 2n x 2n where n is an integer
        /// </summary>
        /// <param name="image">2D array</param>
        /// <param name="disableMatrixDimensionCheck">True if matrix dimension check should be turned off</param>
        /// <returns>Matrix with haar transform</returns>
        public static Matrix HaarWaveletTransform(double[][] image, bool disableMatrixDimensionCheck = false)
        {
            var imageMatrix = new Matrix(image);

            // Check that the input matrix is a square matrix of dimension 2n x 2n (where n is an integer)
            if (!disableMatrixDimensionCheck && !imageMatrix.IsSymmetric() && !MathUtils.IsPowerOfTwo(image.Length))
                throw new Exception("Input matrix is not symmetric or has dimensions that are a power of two!");

            var imagePacked = imageMatrix.GetColumnPackedCopy();
            HaarTransform.haar_2d(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
            var haarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
            return haarMatrix;
        }

        /// <summary>
        ///     Inverse Haar Transform of a 2D image to a Matrix.
        ///     This is using the tensor product layout.
        ///     Performance is also quite fast.
        ///     Note that the input array must be a square matrix of dimension 2n x 2n where n is an integer
        /// </summary>
        /// <param name="image">2D array</param>
        /// <param name="disableMatrixDimensionCheck">True if matrix dimension check should be turned off</param>
        /// <returns>Matrix with inverse haar transform</returns>
        public static Matrix InverseHaarWaveletTransform(double[][] image, bool disableMatrixDimensionCheck = false)
        {
            var imageMatrix = new Matrix(image);

            // Check that the input matrix is a square matrix of dimension 2n x 2n (where n is an integer)
            if (!disableMatrixDimensionCheck && !imageMatrix.IsSymmetric() && !MathUtils.IsPowerOfTwo(image.Length))
                throw new Exception("Input matrix is not symmetric or has dimensions that are a power of two!");

            var imagePacked = imageMatrix.GetColumnPackedCopy();
            HaarTransform.haar_2d_inverse(imageMatrix.Rows, imageMatrix.Columns, imagePacked);
            var inverseHaarMatrix = new Matrix(imagePacked, imageMatrix.Rows);
            return inverseHaarMatrix;
        }

        public static void TestHaarInputOutput(string imageInPath)
        {
            // read image
            var img = Image.FromFile(imageInPath);

            // make sure it's square and power of two
            //int size = (img.Height > img.Width ? img.Height : img.Width);
            //int sizePow2 = MathUtils.NextPowerOfTwo(size);
            //img = ImageUtils.Resize(img, sizePow2, sizePow2, false);

            var bmp = new Bitmap(img);
            var image = new double[bmp.Height][];
            for (var i = 0; i < bmp.Height; i++)
            {
                image[i] = new double[bmp.Width];
                for (var j = 0; j < bmp.Width; j++)
                {
                    var C = bmp.GetPixel(j, i);
                    image[i][j] = (C.R + C.G + C.B) / 3;

                    //image[i][j] = bmp.GetPixel(j, i).ToArgb();
                    //image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
                }
            }

            var inputMatrix = new Matrix(image);
            //inputMatrix.WriteCSV("haar-before.csv", ";");

            //Wavelets.Compress.WaveletComDec.CompressDecompress2D(image, 3, 0);
            //inputMatrix.DrawMatrixImage("haar-transform-back.png", -1, -1, false);
            //return;

            // Haar Wavelet Transform
            //Matrix haarMatrix = HaarWaveletTransform(inputMatrix.MatrixData);

            //Wavelets.Compress.HaarWaveletTransform.HaarTransform2D(image, inputMatrix.Rows, inputMatrix.Columns);
            var lastHeight = 0;
            var lastWidth = 0;
            var levels = 3;
            //Wavelets.Compress.WaveletCompress.Compress2D(image, levels, 500, out lastHeight, out lastWidth);
            WaveletCompress.HaarTransform2D(image, levels, out lastHeight, out lastWidth);
            var haarMatrix = inputMatrix.Copy();

            //Wavelets.Dwt dwt = new Wavelets.Dwt(2);
            //Matrix haarMatrix = dwt.Transform(normalizedMatrix);

            /*
            WaveletInterface wavelet = new Haar02();
            TransformInterface bWave = new FastWaveletTransform(wavelet);
            Transform t = new Transform(bWave); // perform all steps
            double[][] dwtArray = t.forward(normalizedMatrix.MatrixData);
            Matrix haarMatrix = new Matrix(dwtArray);
             */
            //int oldRows = haarMatrix.Rows;
            //int oldColumns = haarMatrix.Columns;
            //haarMatrix = haarMatrix.Resize(20, oldColumns);
            haarMatrix.WriteCSV("haar.csv", ";");

            // Inverse 2D Haar Wavelet Transform
            //Matrix haarMatrixInverse = InverseHaarWaveletTransform(haarMatrix.MatrixData);

            var haarMatrixInverse = haarMatrix.Copy();
            //haarMatrixInverse = haarMatrixInverse.Resize(oldRows, oldColumns);
            //Wavelets.Compress.HaarWaveletTransform.InverseHaarTransform2D(haarMatrixInverse.MatrixData, haarMatrixInverse.Rows, haarMatrixInverse.Columns);
            WaveletDecompress.Decompress2D(haarMatrixInverse.MatrixData, levels, lastHeight, lastWidth);

            //Matrix haarMatrixInverse = dwt.TransformBack(haarMatrix);

            //double[][] dwtArrayInverse = t.reverse(haarMatrix.MatrixData);
            //Matrix haarMatrixInverse = new Matrix(dwtArrayInverse);

            //haarMatrixInverse.WriteCSV("haar-inverse.csv", ";");

            // Output the image
            //haarMatrix.DrawMatrixImageLogValues("haar-transform.png");
            haarMatrixInverse.DrawMatrixImage("haar-transform-back.png", -1, -1, false);
        }

        public static void TestDenoise(string imageInPath)
        {
            // Read Image
            var img = Image.FromFile(imageInPath);
            var bmp = new Bitmap(img);
            var image = new double[bmp.Height][];
            for (var i = 0; i < bmp.Height; i++)
            {
                image[i] = new double[bmp.Width];
                for (var j = 0; j < bmp.Width; j++) //image[i][j] = bmp.GetPixel(j, i).ToArgb();
                    image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
            }

            //Matrix imageMatrix = new Matrix(image);
            //imageMatrix.WriteCSV("lena-blue.csv", ";");

            // Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
            var max = image.Max(b => b.Max(v => Math.Abs(v)));
            var imageNormalized = image.Select(i => i.Select(j => j / max).ToArray()).ToArray();

            var normalizedMatrix = new Matrix(imageNormalized);
            //normalizedMatrix.WriteCSV("lena-normalized.csv", ";");
            normalizedMatrix.DrawMatrixImage("lena-original.png", -1, -1, false);

            // Add Noise using normally distributed pseudorandom numbers
            // image_noisy = image_normalized + 0.1 * randn(size(image_normalized));
            SimpleRNG.SetSeedFromSystemTime();
            var imageNoisy = imageNormalized.Select(i => i.Select(j => j + 0.1 * SimpleRNG.GetNormal()).ToArray())
                .ToArray();
            var matrixNoisy = new Matrix(imageNoisy);
            matrixNoisy.DrawMatrixImage("lena-noisy.png", -1, -1, false);

            // Haar Wavelet Transform
            var haarMatrix = HaarWaveletTransform(imageNoisy);

            // Thresholding
            var threshold = 0.15; // 0.15 seems to work well with the noise added above, 0.1
            var yHard = Thresholding.perform_hard_thresholding(haarMatrix.MatrixData, threshold);
            var ySoft = Thresholding.perform_soft_thresholding(haarMatrix.MatrixData, threshold);
            var ySemisoft = Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold * 2);
            var ySemisoft2 =
                Thresholding.perform_semisoft_thresholding(haarMatrix.MatrixData, threshold, threshold * 4);
            var yStrict = Thresholding.perform_strict_thresholding(haarMatrix.MatrixData, 10);

            // Inverse 2D Haar Wavelet Transform
            var zHard = InverseHaarWaveletTransform(yHard);
            var zSoft = InverseHaarWaveletTransform(ySoft);
            var zSemisoft = InverseHaarWaveletTransform(ySemisoft);
            var zSemisoft2 = InverseHaarWaveletTransform(ySemisoft2);
            var zStrict = InverseHaarWaveletTransform(yStrict);

            //zHard.WriteCSV("lena-thresholding-hard.csv", ";");

            // Output the images
            zHard.DrawMatrixImage("lena-thresholding-hard.png", -1, -1, false);
            zSoft.DrawMatrixImage("lena-thresholding-soft.png", -1, -1, false);
            zSemisoft.DrawMatrixImage("lena-thresholding-semisoft.png", -1, -1, false);
            zSemisoft2.DrawMatrixImage("lena-thresholding-semisoft2.png", -1, -1, false);
            zStrict.DrawMatrixImage("lena-thresholding-strict.png", -1, -1, false);
        }

        //Func<int, int, double> window
        public static void SaveWaveletImage(string imageInPath, string imageOutPath, WaveletMethod waveletMethod)
        {
            // Read Image
            var img = Image.FromFile(imageInPath);
            var bmp = new Bitmap(img);
            var image = new double[bmp.Height][];
            for (var i = 0; i < bmp.Height; i++)
            {
                image[i] = new double[bmp.Width];
                for (var j = 0; j < bmp.Width; j++) //image[i][j] = bmp.GetPixel(j, i).ToArgb();
                    image[i][j] = bmp.GetPixel(j, i).B; // use only blue channel
            }

            // Normalize the pixel values to the range 0..1.0. It does this by dividing all pixel values by the max value.
            var max = image.Max(b => b.Max(v => v));
            var imageNormalized = image.Select(i => i.Select(j => j / max).ToArray()).ToArray();
            //Matrix normalizedMatrix = new Matrix(imageNormalized);
            //normalizedMatrix.WriteCSV("ImageNormalized.csv", ";");

            var bitmap = GetWaveletTransformedMatrix(imageNormalized, waveletMethod);
            bitmap.DrawMatrixImage(imageOutPath, -1, -1, false);

            img.Dispose();
            bmp.Dispose();
            bitmap = null;
        }

        public static Matrix GetWaveletTransformedMatrix(double[][] image, WaveletMethod waveletMethod)
        {
            var width = image[0].Length;
            var height = image.Length;

            Matrix dwtMatrix = null;

            var stopWatch = Stopwatch.StartNew();
            var startS = stopWatch.ElapsedTicks;

            switch (waveletMethod)
            {
                case WaveletMethod.Dwt:
                    var dwt = new Dwt(8);
                    var imageMatrix = new Matrix(image);
                    dwtMatrix = dwt.Transform(imageMatrix);
                    break;
                case WaveletMethod.Haar:
                    Haar.Haar2d(image, height, width);
                    dwtMatrix = new Matrix(image);
                    break;
                case WaveletMethod.HaarTransformTensor: // This is using the tensor product layout
                    dwtMatrix = HaarWaveletTransform(image);
                    break;
                case WaveletMethod.HaarWaveletDecompositionTensor: // This is using the tensor product layout
                    var haar = new StandardHaarWaveletDecomposition();
                    haar.DecomposeImageInPlace(image);
                    dwtMatrix = new Matrix(image);
                    break;
                case WaveletMethod.HaarWaveletDecomposition:
                    var haarNew = new StandardHaarWaveletDecomposition(false);
                    haarNew.DecomposeImageInPlace(image);
                    dwtMatrix = new Matrix(image);
                    break;
                case WaveletMethod.NonStandardHaarWaveletDecomposition:
                    var haarNonStandard = new NonStandardHaarWaveletDecomposition();
                    haarNonStandard.DecomposeImageInPlace(image);
                    dwtMatrix = new Matrix(image);
                    break;
                case WaveletMethod.JWaveTensor: // This is using the tensor product layout
                    WaveletInterface wavelet = null;
                    wavelet = new Haar02();
                    //wavelet = new Daub02();
                    TransformInterface bWave = null;
                    bWave = new FastWaveletTransform(wavelet);
                    //bWave = new WaveletPacketTransform(wavelet);
                    //bWave = new DiscreteWaveletTransform(wavelet);
                    var t = new Transform(bWave); // perform all steps
                    var dwtArray = t.forward(image);
                    dwtMatrix = new Matrix(dwtArray);
                    break;
                case WaveletMethod.HaarWaveletCompress:
                    var lastHeight = 0;
                    var lastWidth = 0;
                    WaveletCompress.HaarTransform2D(image, 10000, out lastHeight, out lastWidth);
                    dwtMatrix = new Matrix(image);
                    break;
            }

            var endS = stopWatch.ElapsedTicks;
            Console.WriteLine("WaveletMethod: {0} Time in ticks: {1}",
                Enum.GetName(typeof(WaveletMethod), waveletMethod), endS - startS);

            //dwtMatrix.WriteCSV("HaarImageNormalized.csv", ";");

            // increase all values
            var haarImageNormalized5k = dwtMatrix.MatrixData.Select(i => i.Select(j => j * 5000).ToArray()).ToArray();
            //Matrix haarImageNormalized5kMatrix = new Matrix(haarImageNormalized5k);
            //haarImageNormalized5kMatrix.WriteCSV("HaarImageNormalized5k.csv", ";");

            // convert to byte values (0 - 255)
            // duplicate the octave/ matlab method uint8
            var uint8 = new double[haarImageNormalized5k.Length][];
            for (var i = 0; i < haarImageNormalized5k.Length; i++)
            {
                uint8[i] = new double[haarImageNormalized5k.Length];
                for (var j = 0; j < haarImageNormalized5k[i].Length; j++)
                {
                    var v = haarImageNormalized5k[i][j];
                    if (v > 255)
                        uint8[i][j] = 255;
                    else if (v < 0)
                        uint8[i][j] = 0;
                    else
                        uint8[i][j] = (byte)haarImageNormalized5k[i][j];
                }
            }

            var uint8Matrix = new Matrix(uint8);
            //uint8Matrix.WriteCSV("Uint8HaarImageNormalized5k.csv", ";");
            return uint8Matrix;
        }

        /*
         * mat = [5, 6, 1, 2; 4, 2, 5, 5; 3, 1, 7, 1; 6, 3, 5, 1]
         */
        private static double[][] Get2DTestData()
        {
            var mat = new double[4][];
            for (var m = 0; m < 4; m++) mat[m] = new double[4];
            mat[0][0] = 5;
            mat[0][1] = 6;
            mat[0][2] = 1;
            mat[0][3] = 2;

            mat[1][0] = 4;
            mat[1][1] = 2;
            mat[1][2] = 5;
            mat[1][3] = 5;

            mat[2][0] = 3;
            mat[2][1] = 1;
            mat[2][2] = 7;
            mat[2][3] = 1;

            mat[3][0] = 6;
            mat[3][1] = 3;
            mat[3][2] = 5;
            mat[3][3] = 1;

            return mat;
        }

        public static void TestHaar1d()
        {
            var i = 0;
            double[] vec3 = { 4, 2, 5, 5 };

            Haar.Haar1d(vec3, 4);

            Console.Write("The 1D Haar Transform: ");
            Console.Write("\n");
            for (i = 0; i < 4; i++)
            {
                Console.Write(vec3[i]);
                Console.Write(" ");
            }

            Console.Write("\n");
        }

        public static void TestHaar2d()
        {
            Console.Write("\n\nThe 2D Haar Transform: ");
            Console.Write("\n");

            var mat = Get2DTestData();
            Haar.Haar2d(mat, 4, 4);

            var result = new Matrix(mat);
            result.Print();
        }

        public static void TestHaarWaveletTransform2D()
        {
            Console.Write("\n\nThe Haar Wavelet Transform 2D: ");
            Console.Write("\n");

            var mat = Get2DTestData();
            Compress.HaarWaveletTransform.HaarTransform2D(mat, 4, 4);

            var result = new Matrix(mat);
            result.Print();

            Compress.HaarWaveletTransform.InverseHaarTransform2D(mat, 4, 4);
            result.Print();
        }

        public static void TestHaarWaveletDecomposition()
        {
            Console.Write("\n\nThe Standard 2D HaarWaveletDecomposition method: ");
            Console.Write("\n");

            var haar = new StandardHaarWaveletDecomposition();

            var mat = Get2DTestData();

            haar.DecomposeImageInPlace(mat);

            var result = new Matrix(mat);
            result.Print();

            Console.Write("\n\nThe New Standard 2D HaarWaveletDecomposition method: ");
            Console.Write("\n");

            var haarNew = new StandardHaarWaveletDecomposition(false);

            mat = Get2DTestData();
            haarNew.DecomposeImageInPlace(mat);

            var resultNew = new Matrix(mat);
            resultNew.Print();

            Console.Write("\n\nThe Non Standard 2D HaarWaveletDecomposition method: ");
            Console.Write("\n");

            var haarNonStandard = new NonStandardHaarWaveletDecomposition();

            mat = Get2DTestData();
            haarNonStandard.DecomposeImageInPlace(mat);

            var resultNonStandard = new Matrix(mat);
            resultNonStandard.Print();
        }

        public static void TestDwt()
        {
            var mat = Get2DTestData();
            var matrix = new Matrix(mat);
            var dwt = new Dwt(2);

            Console.Write("\n\nThe 2D DWT method: ");
            Console.Write("\n");
            var dwtMatrix = dwt.Transform(matrix);
            dwtMatrix.Print();

            Console.Write("\n\nThe 2D IDWT method: ");
            Console.Write("\n");
            var idwtMatrix = dwt.TransformBack(dwtMatrix);
            idwtMatrix.Print();
        }

        public static void TestJWave()
        {
            var mat = Get2DTestData();

            WaveletInterface wavelet = null;
            wavelet = new Haar02();
            TransformInterface bWave = null;
            //bWave = new FastWaveletTransform(wavelet);
            //bWave = new WaveletPacketTransform(wavelet);
            bWave = new DiscreteWaveletTransform(wavelet);
            var t = new Transform(bWave); // perform all steps

            Console.Write("\n\nThe 2D JWave Haar02 Dwt method: ");
            Console.Write("\n");
            var dwtArray = t.forward(mat);

            var dwtMatrix = new Matrix(dwtArray);
            dwtMatrix.Print();

            Console.Write("\n\nThe 2D JWave Haar02 Inverse Dwt method: ");
            Console.Write("\n");
            var idwtArray = t.reverse(dwtArray);

            var idwtMatrix = new Matrix(idwtArray);
            idwtMatrix.Print();
        }

        public static void TestHaarTransform()
        {
            var mat = Get2DTestData();
            var matrix = new Matrix(mat);
            //matrix.Print();

            var packed = matrix.GetColumnPackedCopy();
            HaarTransform.r8mat_print(matrix.Rows, matrix.Columns, packed, "  Input array packed:");

            HaarTransform.haar_2d(matrix.Rows, matrix.Columns, packed);
            HaarTransform.r8mat_print(matrix.Rows, matrix.Columns, packed, "  Transformed array packed:");

            var w = HaarTransform.r8mat_copy_new(matrix.Rows, matrix.Columns, packed);

            HaarTransform.haar_2d_inverse(matrix.Rows, matrix.Columns, w);
            HaarTransform.r8mat_print(matrix.Rows, matrix.Columns, w, "  Recovered array W:");

            var m = new Matrix(w, matrix.Rows);
            //m.Print();
        }
    }
}