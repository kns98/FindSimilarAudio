// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting

using System;
using System.Linq;

namespace Soundfingerprinting.Fingerprinting.Wavelets
{
    public static class WaveletUtils
    {
        public static void WaveletNoiseHardThresholding(float[][] array)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                var len = array[0].Length;
                double median = len % 2 == 1 ? array[i][len / 2 + 1] : array[i][len / 2];
                var copy = new double[len];
                Array.Copy(array[i], copy, len);
                for (var j = 0; j < len; j++) copy[j] = Math.Abs(copy[j] - median);
                var mad = len % 2 == 1 ? copy[len / 2 + 1] : copy[len / 2];
                var t = mad * Math.Sqrt(Math.Log(len)) / 0.6745;
                for (var j = 0; j < len; j++)
                    if (array[i][j] < t)
                        array[i][j] = 0;
            }
        }

        public static void WaveletNoiseHardThresholding2(double[][] array)
        {
            var len = array[0].Length;
            var elements = array.GetLength(0);
            var average = array.Sum(a => a.Sum()) / (len * elements);
            var sum = array.Sum(a => a.Sum(var => Math.Abs(var - average)));
            var mean = sum / (len * elements);
            var t = mean * Math.Sqrt(2 * Math.Log(len));

            for (var i = 0; i < array.GetLength(0); i++)
            for (var j = 0; j < len; j++)
                if (array[i][j] < t)
                    array[i][j] = 0;
        }
    }
}