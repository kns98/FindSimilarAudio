// Sound Fingerprinting framework
// git://github.com/AddictedCS/soundfingerprinting.git
// Code license: CPOL v.1.02
// ciumac.sergiu@gmail.com
// http://www.codeproject.com/Articles/206507/Duplicates-detector-via-audio-fingerprinting

using System;
using System.Linq;

namespace Soundfingerprinting.Fingerprinting
{
    /// <summary>
    ///     Signature image encoder/decoder
    /// </summary>
    /// <description>
    ///     Negative Numbers = 01
    ///     Positive Numbers = 10
    ///     Zeros            = 00
    /// </description>
    public class FingerprintDescriptor
    {
        private readonly AbsComparator absComparator;

        public FingerprintDescriptor()
        {
            absComparator = new AbsComparator();
        }

        /// <summary>
        ///     Sets all other wavelet values to 0 except whose which make part of Top Wavelet [top wavelet &gt; 0 ? 1 : -1]
        /// </summary>
        /// <param name="frames">
        ///     Frames with 32 logarithmically spaced frequency bins
        /// </param>
        /// <param name="topWavelets">
        ///     The top Wavelets.
        /// </param>
        /// <returns>
        ///     Signature signature. Array of encoded Boolean elements (wavelet signature)
        /// </returns>
        /// <remarks>
        ///     Negative Numbers = 01
        ///     Positive Numbers = 10
        ///     Zeros            = 00
        /// </remarks>
        /// <description>
        ///     One of the interesting things found in the previous studies of image processing
        ///     was that there is no need to keep the magnitude of top wavelets;
        ///     instead, we can simply keep the sign of it (+/-). This information is enough to keep the extract perceptual
        ///     characteristics of a song.
        /// </description>
        public bool[] ExtractTopWavelets(double[][] frames, int topWavelets)
        {
            var rows = frames.GetLength(0); /*128*/
            var cols = frames[0].Length; /*32*/
            var concatenated = new double[rows * cols]; /* 128 * 32 */
            for (var row = 0; row < rows; row++)
                Array.Copy(frames[row], 0, concatenated, row * frames[row].Length, frames[row].Length);

            var indexes = Enumerable.Range(0, concatenated.Length).ToArray();
            Array.Sort(concatenated, indexes, absComparator);
            var result = EncodeFingerprint(concatenated, indexes, topWavelets);
            return result;
        }

        /// <summary>
        ///     Encode the integer representation of the fingerprint into a Boolean array
        /// </summary>
        /// <param name="concatenated">Concatenated fingerprint (frames concatenated)</param>
        /// <param name="indexes">Sorted indexes with the first one with the highest value in array</param>
        /// <param name="topWavelets">Number of top wavelets to encode</param>
        /// <returns>Encoded fingerprint</returns>
        public bool[] EncodeFingerprint(double[] concatenated, int[] indexes, int topWavelets)
        {
            var result = new bool[concatenated.Length * 2]; // Concatenated float array
            for (var i = 0; i < topWavelets; i++)
            {
                var index = indexes[i];
                var value = concatenated[i];
                if (value > 0)
                    // positive wavelet
                    result[index * 2] = true;
                else if (value < 0)
                    // negative wavelet
                    result[index * 2 + 1] = true;
            }

            return result;
        }

        /// <summary>
        ///     Decode the signature of the fingerprint
        /// </summary>
        /// <param name="signature">Signature to be decoded</param>
        /// <returns>Array of doubles with positive [10], negatives [01], and zeros [00]</returns>
        public double[] DecodeFingerprint(bool[] signature)
        {
            var len = signature.Length / 2;
            var result = new double[len];
            for (var i = 0; i < len * 2; i += 2)
                if (signature[i])
                    // positive if first is true
                    result[i / 2] = 1;
                else if (signature[i + 1])
                    // negative if second is true
                    result[i / 2] = -1;

            // otherwise '0'

            return result;
        }
    }
}