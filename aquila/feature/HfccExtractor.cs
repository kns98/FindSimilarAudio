using System;
using System.Numerics;

/**
 * @file HfccExtractor.cpp
 *
 * HFCC feature extraction - implementation.
 *
 * @author Zbigniew Siciarz
 * @date 2007-2010
 * @version 2.5.1
 * @since 2.5.1
 */
namespace Aquila
{
    /**
	 * HFCC feature extractor, basing on MFCC.
	 */
    public class HfccExtractor : MfccExtractor
    {
        /**
		 * Mel filters bank, static and common to all HFCC extractors.
		 */
        protected static MelFiltersBank hfccFilters;

        /**
         * Sets frame length and number of parameters per frame.
         * 
         * @param frameLength frame length in milliseconds
         * @param paramsPerFrame number of params per frame
         */
        public HfccExtractor(int frameLength, int paramsPerFrame) : base(frameLength, paramsPerFrame)
        {
            type = "HFCC";
        }

        /**
		 * Deletes the extractor object.
		 */
        public new void Dispose()
        {
            base.Dispose();
        }

        /**
         * Calculates HFCC features for each frame.
         * 
         * @param wav recording object
         * @param options transform options
         */
        public new void Process(WaveFile wav, TransformOptions options)
        {
            wavFilename = wav.GetFilename();

            var framesCount = wav.GetFramesCount();
            Array.Resize(ref featureArray, framesCount);

            if (m_indicator != null)
                m_indicator.Start(0, framesCount - 1);

            var N = wav.GetSamplesPerFrameZP();
            UpdateFilters(wav.GetSampleFrequency(), N);

            var frameSpectrum = new Complex[N];
            var filtersOutput = new double[Dtw.MELFILTERS];
            var frameHfcc = new double[m_paramsPerFrame];

            var transform = new Transform(options);

            // for each frame: FFT -> Mel filtration -> DCT
            for (var i = 0; i < framesCount; ++i)
            {
                transform.Fft(wav.frames[i], ref frameSpectrum);
                hfccFilters.ApplyAll(ref frameSpectrum, N, ref filtersOutput);
                transform.Dct(filtersOutput, ref frameHfcc);

                //featureArray[i] = frameHfcc;
                featureArray[i] = new double[frameHfcc.Length];
                frameHfcc.CopyTo(featureArray[i], 0);

                if (m_indicator != null)
                    m_indicator.Progress(i);
            }

            if (m_indicator != null)
                m_indicator.Stop();
        }

        /**
         * Updates the filter bank.
         * 
         * (Re)creates new filter bank when sample frequency or spectrum size
         * changed. If requested, enables only some filters.
         * 
         * @param frequency sample frequency
         * @param N spectrum size
         */
        protected new void UpdateFilters(uint frequency, int N)
        {
            if (hfccFilters == null)
            {
                hfccFilters = new MelFiltersBank(frequency, N, true);
            }
            else
            {
                if (hfccFilters.GetSampleFrequency() != frequency || hfccFilters.GetSpectrumLength() != N)
                {
                    if (hfccFilters != null)
                        hfccFilters.Dispose();
                    hfccFilters = new MelFiltersBank(frequency, N, true);
                }
            }

            if (enabledFilters.Length != 0)
                hfccFilters.SetEnabledFilters(enabledFilters);
        }
    }
}