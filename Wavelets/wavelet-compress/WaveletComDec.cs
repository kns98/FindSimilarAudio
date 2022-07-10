using System;

namespace Wavelets.Compress
{
    /// Author: Linzhi Qi
    /// Converted from C++ by perivar@nerseth.com
    /// https://github.com/linzhi/wavelet-compress
    public static class WaveletComDec
    {
        public static void CompressDecompress2D(double[][] data_input, int level, int threshold)
        {
            var temp_level = level;

            var ex_height = data_input.Length;
            var ex_width = data_input[0].Length;

            var temp_ex_height = ex_height;
            var temp_ex_width = ex_width;

            while (temp_level > 0 && ex_height > 1 && ex_width > 1)
            {
                HaarWaveletTransform.HaarTransform2D(data_input, ex_height, ex_width);

                if (ex_width > 1)
                    ex_width = ex_width / 2;
                if (ex_height > 1)
                    ex_height = ex_height / 2;

                temp_level--;
            }

            Quantize.DataQuantize2D(data_input, temp_ex_height, temp_ex_width, threshold);

            while (temp_level < level && ex_height > 1 && ex_width > 1)
            {
                if (ex_width > 1)
                    ex_width = ex_width * 2;
                if (ex_height > 1)
                    ex_height = ex_height * 2;

                HaarWaveletTransform.InverseHaarTransform2D(data_input, ex_height, ex_width);

                temp_level++;
            }
        }

        public static void CompressDecompress(string file_input, string file_output, int level, int value)
        {
            var i = 0;
            var j = 0;
            var k = 0;
            var lon = 0;
            var wid = 0;
            var hei = 0;
            var temp_ex_lon = 0;
            var temp_ex_wid = 0;
            var temp_ex_hei = 0;
            var ex_lon = 0;
            var ex_wid = 0;
            var ex_hei = 0;
            int temp;
            var temp_level = level;
            double mse = 0;
            double psnr = 0;

            var file_size = 0;
            var file_in = new double[file_size];
            var file_out = new double[file_size];

            // Load file data into data_input[j][k][i]
            var data_input = new double[j][][];

            while (temp_level > 0 && ex_lon > 1 && ex_wid > 1 && ex_hei > 1)
            {
                HaarWaveletTransform.HaarTransform3D(data_input, ex_lon, ex_wid, ex_hei);

                if (ex_lon > 1)
                    ex_lon = ex_lon / 2;
                if (ex_wid > 1)
                    ex_wid = ex_wid / 2;
                if (ex_hei > 1)
                    ex_hei = ex_hei / 2;

                temp_level--;
            }

            Quantize.DataQuantize3D(data_input, temp_ex_lon, temp_ex_wid, temp_ex_hei, value);

            while (temp_level < level && ex_lon > 1 && ex_wid > 1 && ex_hei > 1)
            {
                if (ex_lon > 1)
                    ex_lon = ex_lon * 2;
                if (ex_wid > 1)
                    ex_wid = ex_wid * 2;
                if (ex_hei > 1)
                    ex_hei = ex_hei * 2;

                HaarWaveletTransform.InverseHaarTransform3D(data_input, ex_lon, ex_wid, ex_hei);

                temp_level++;
            }

            file_out = new double[file_size];

            for (i = 0; i < hei; i++)
            for (j = 0; j < lon; j++)
            for (k = 0; k < wid; k++)
            {
                temp = lon * wid * i + wid * j + k;
                file_out[temp] = data_input[j][k][i];
            }

            mse = VerificationResult.MeanSquaredError(file_in, file_out, ref file_size);
            psnr = VerificationResult.PeakSignalToNoiseRatio(mse);
            Console.Write("MSE: ");
            Console.Write(mse);
            Console.Write("\n");
            Console.Write("PSNR: ");
            Console.Write(psnr);
            Console.Write("\n");

            // save file file_out to file_output with file_size

            file_in = null;
            file_out = null;
            data_input = null;
        }
    }
}