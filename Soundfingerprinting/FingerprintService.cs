using System.Collections.Generic;
using Mirage;
using Soundfingerprinting.Audio.Models;
using Soundfingerprinting.Audio.Services;
using Soundfingerprinting.Audio.Strides;
using Soundfingerprinting.Fingerprinting.FFT;
using Soundfingerprinting.Fingerprinting.Wavelets;
using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;

namespace Soundfingerprinting.Fingerprinting
{
    // for debug

    public class FingerprintService
    {
        public readonly IAudioService AudioService;
        public readonly FingerprintDescriptor FingerprintDescriptor;
        public readonly SpectrumService SpectrumService;
        public readonly IWaveletService WaveletService;

        public FingerprintService(
            IAudioService audioService,
            FingerprintDescriptor fingerprintDescriptor,
            SpectrumService spectrumService,
            IWaveletService waveletService)
        {
            SpectrumService = spectrumService;
            WaveletService = waveletService;
            FingerprintDescriptor = fingerprintDescriptor;
            AudioService = audioService;
        }

        public List<bool[]> CreateFingerprintsFromAudioFile(WorkUnitParameterObject param,
            out double[][] logSpectrogram)
        {
            var samples = AudioService.ReadMonoFromFile(
                param.PathToAudioFile,
                param.FingerprintingConfiguration.SampleRate,
                param.MillisecondsToProcess,
                param.StartAtMilliseconds);

            return CreateFingerprintsFromAudioSamples(samples, param, out logSpectrogram);
        }

        public List<bool[]> CreateFingerprintsFromAudioSamples(float[] samples, WorkUnitParameterObject param,
            out double[][] logSpectrogram)
        {
            var configuration = param.FingerprintingConfiguration;
            var audioServiceConfiguration = new AudioServiceConfiguration
            {
                LogBins = configuration.LogBins,
                LogBase = configuration.LogBase,
                MaxFrequency = configuration.MaxFrequency,
                MinFrequency = configuration.MinFrequency,
                Overlap = configuration.Overlap,
                SampleRate = configuration.SampleRate,
                WindowSize = configuration.WindowSize,
                NormalizeSignal = configuration.NormalizeSignal,
                UseDynamicLogBase = configuration.UseDynamicLogBase
            };

            // store the log spectrogram in the out variable
            logSpectrogram = AudioService.CreateLogSpectrogram(
                samples, configuration.WindowFunction, audioServiceConfiguration);

            return CreateFingerprintsFromLogSpectrum(
                logSpectrogram,
                configuration.Stride,
                configuration.FingerprintLength,
                configuration.Overlap,
                configuration.TopWavelets);
        }

        public List<bool[]> CreateFingerprintsFromLogSpectrum(
            double[][] logarithmizedSpectrum, IStride stride, int fingerprintLength, int overlap, int topWavelets)
        {
            var t = new DbgTimer();
            t.Start();

            // Cut the logaritmic spectrogram into smaller spectrograms with one stride between each
            var spectralImages =
                SpectrumService.CutLogarithmizedSpectrum(logarithmizedSpectrum, stride, fingerprintLength, overlap);

            // Then apply the wavelet transform on them to later reduce the resolution
            // do this in place
            WaveletService.ApplyWaveletTransformInPlace(spectralImages);

            // Then for each of the wavelet reduce the resolution by only keeping the top wavelets
            // and ignore the magnitude of the top wavelets.
            // Instead, we can simply keep the sign of it (+/-).
            // This information is enough to keep the extract perceptual characteristics of a song.
            var fingerprints = new List<bool[]>();
            foreach (var spectralImage in spectralImages)
            {
                var image = FingerprintDescriptor.ExtractTopWavelets(spectralImage, topWavelets);
                fingerprints.Add(image);
            }

            Dbg.WriteLine("Created {1} Fingerprints from Log Spectrum - Execution Time: {0} ms",
                t.Stop().TotalMilliseconds, fingerprints.Count);
            return fingerprints;
        }
    }
}