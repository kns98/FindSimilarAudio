using Mirage;
using Soundfingerprinting.Audio.Strides;

namespace Soundfingerprinting.Fingerprinting.Configuration
{
    public class DefaultFingerprintingConfiguration : IFingerprintingConfiguration
    {
        public DefaultFingerprintingConfiguration()
        {
            // The parameters used in these transformation steps will be equal to those that have been found to work well in other audio fingerprinting studies
            // (specifically in A Highly Robust Audio Fingerprinting System):
            // audio frames that are 371 ms long (2048 samples),
            // taken every 11.6 ms (64 samples),
            // thus having an overlap of 31/32
            FingerprintLength = 128;
            WindowSize = 2048; // 2048/5512 	= 371 ms
            Overlap = 64; // 64/5512 		= 11,6 ms
            SamplesPerFingerprint = FingerprintLength * Overlap;
            MinFrequency = 318;
            MaxFrequency = 2000;
            SampleRate = 5512;
            LogBase = 2; // 2 or 10;

            // In Content Fingerprinting Using Wavelets, a static 928 ms stride was used in database creation,
            // and a random 0-46 ms stride was used in querying (random stride was used in order to minimize the coarse effect of unlucky time alignment).
            Stride = new IncrementalStaticStride(5115, FingerprintLength * Overlap); // 5115 / 5512 = 0,928 sec

            TopWavelets = 200;
            LogBins = 32;

            // Each fingerprint will be LogBins x FingerprintLength x 2 Bits long
            // e.g. 128 x 32 x 2 = 8192
            StartFingerprintIndex = 0;
            EndFingerprintIndex = LogBins * FingerprintLength * 2;

            WindowFunction = new HannWindow(WindowSize);
            NormalizeSignal = true;
            UseDynamicLogBase = false;

            // Number of LSH tables
            NumberOfHashTables = 25;

            // Number of Min Hash keys per 1 hash function (1 LSH table)
            NumberOfKeys = 4;
        }

        /// <summary>
        ///     Gets number of samples to read in order to create single signature. The granularity is 1.48 seconds
        /// </summary>
        /// <remarks>
        ///     Default = 8192
        /// </remarks>
        public int SamplesPerFingerprint { get; }

        /// <summary>
        ///     Gets overlap between the sub fingerprints, 11.6 ms
        /// </summary>
        /// <remarks>
        ///     Default = 64
        /// </remarks>
        public int Overlap { get; }

        /// <summary>
        ///     Gets size of the WDFT block, 371 ms
        /// </summary>
        /// <remarks>
        ///     Default = 2048
        /// </remarks>
        public int WindowSize { get; }

        /// <summary>
        ///     Gets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///     Default = 318
        /// </remarks>
        public int MinFrequency { get; }

        /// <summary>
        ///     Gets frequency range which is taken into account when creating the signature
        /// </summary>
        /// <remarks>
        ///     Default = 2000
        /// </remarks>
        public int MaxFrequency { get; }

        /// <summary>
        ///     Gets number of Top wavelets to consider
        /// </summary>
        /// <remarks>
        ///     Default = 200
        /// </remarks>
        public int TopWavelets { get; }

        /// <summary>
        ///     Gets sample rate at which the audio file will be pre-processed
        /// </summary>
        /// <remarks>
        ///     Default = 5512
        /// </remarks>
        public int SampleRate { get; }

        /// <summary>
        ///     Gets log base used for computing the logarithmically spaced frequency bins
        /// </summary>
        /// <remarks>
        ///     Default = 10
        /// </remarks>
        public double LogBase { get; }

        /// <summary>
        ///     Gets number of logarithmically spaced bins between the frequency components computed by Fast Fourier Transform.
        /// </summary>
        public int LogBins { get; }

        /// <summary>
        ///     Gets signature's length
        /// </summary>
        public int FingerprintLength { get; }

        /// <summary>
        ///     Gets default stride size between 2 consecutive signature
        /// </summary>
        /// <remarks>
        ///     Default = 5115
        /// </remarks>
        public IStride Stride { get; }

        public IWindowFunction WindowFunction { get; }

        public bool NormalizeSignal { get; }

        public bool UseDynamicLogBase { get; }

        /// <summary>
        ///     Number of LSH tables
        /// </summary>
        public int NumberOfHashTables { get; }

        /// <summary>
        ///     Number of Min Hash keys per 1 hash function (1 LSH table)
        /// </summary>
        public int NumberOfKeys { get; }

        /// <summary>
        ///     Fingerprint start index
        /// </summary>
        public int StartFingerprintIndex { get; }

        /// <summary>
        ///     Each fingerprint will be LogBins x FingerprintLength x 2 Bits long
        ///     e.g. 128 x 32 x 2 = 8192
        /// </summary>
        public int EndFingerprintIndex { get; }
    }
}