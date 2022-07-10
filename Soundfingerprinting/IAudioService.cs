﻿using System;
using Mirage;
using Soundfingerprinting.Audio.Models;

namespace Soundfingerprinting.Audio.Services
{
    public interface IAudioService : IDisposable
    {
        /// <summary>
        ///     Read audio from file at a specific frequency rate
        /// </summary>
        /// <param name="pathToFile">Filename to read from</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="milliSeconds">Milliseconds to read</param>
        /// <param name="startMilliSeconds">Start at a specific millisecond</param>
        /// <returns>Array with data samples</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate, int milliSeconds, int startMilliSeconds);

        /// <summary>
        ///     Read data from file
        /// </summary>
        /// <param name="pathToFile">Filename to be read</param>
        /// <param name="sampleRate">Sample rate at which to perform reading</param>
        /// <returns>Array with data</returns>
        float[] ReadMonoFromFile(string pathToFile, int sampleRate);

        double[][] CreateSpectrogram(
            string pathToFilename, IWindowFunction windowFunction, int sampleRate, int overlap, int wdftSize);

        double[][] CreateLogSpectrogram(
            string pathToFilename, IWindowFunction windowFunction, AudioServiceConfiguration configuration);

        double[][] CreateLogSpectrogram(
            float[] samples, IWindowFunction windowFunction, AudioServiceConfiguration configuration);
    }
}