﻿/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CommonUtils;
using FindSimilar.AudioProxies;

// Heavily modified by perivar@nerseth.com
namespace Mirage
{
    public class AudioFileReader
    {
        private static readonly object _locker = new object();

        public static float[] Decode(string fileIn, int srate, int secondsToAnalyze)
        {
            var t = new DbgTimer();
            t.Start();

            float[] floatBuffer = null;

            // check if file exists
            if (fileIn != null && fileIn != "")
            {
                var fi = new FileInfo(fileIn);
                if (!fi.Exists)
                {
                    Console.Out.WriteLine("No file found {0}!", fileIn);
                    return null;
                }
            }

            // Try to use Un4Seen Bass
            var bass = BassProxy.Instance;
            var duration = bass.GetDurationInSeconds(fileIn);
            if (duration > 0)
            {
                Dbg.WriteLine("Using BASS to decode the file ...");

                // duration in seconds
                if (duration > secondsToAnalyze)
                {
                    // find segment to extract
                    var startSeconds = duration / 2 - secondsToAnalyze / 2;
                    if (startSeconds < 0) startSeconds = 0;
                    floatBuffer = bass.ReadMonoFromFile(fileIn, srate, secondsToAnalyze * 1000,
                        (int)(startSeconds * 1000));

                    // if this failes, the duration read from the tags was wrong or it is something wrong with the audio file
                    if (floatBuffer == null) IOUtils.LogMessageToFile(Mir.WARNING_FILES_LOG, fileIn);
                }
                else
                {
                    // return whole file
                    floatBuffer = bass.ReadMonoFromFile(fileIn, srate, 0, 0);

                    // if this failes, the duration read from the tags was wrong or it is something wrong with the audio file
                    if (floatBuffer == null) IOUtils.LogMessageToFile(Mir.WARNING_FILES_LOG, fileIn);
                }
            }

            // Bass failed reading or never even tried, so use another alternative
            if (floatBuffer == null)
            {
                Dbg.WriteLine("Using MPlayer and SOX to decode the file ...");
                fileIn = Regex.Replace(fileIn, "%20", " ");
                floatBuffer = DecodeUsingMplayerAndSox(fileIn, srate, secondsToAnalyze);
            }

            return floatBuffer;
        }

        public static float[] DecodeUsingSox(string fileIn, int srate, int secondsToAnalyze)
        {
            lock (_locker)
            {
                using (var toraw = new Process())
                {
                    fileIn = Regex.Replace(fileIn, "%20", " ");
                    var t = new DbgTimer();
                    t.Start();
                    var curdir = Environment.CurrentDirectory;
                    Dbg.WriteLine("Decoding: " + fileIn);
                    var tempFile = Path.GetTempFileName();
                    var raw = tempFile + "_raw.wav";
                    Dbg.WriteLine("Temporary raw file: " + raw);

                    toraw.StartInfo.FileName = "./NativeLibraries\\sox\\sox.exe";
                    toraw.StartInfo.Arguments = " \"" + fileIn + "\" -r " + srate + " -e float -b 32 -G -t raw \"" +
                                                raw + "\" channels 1";
                    toraw.StartInfo.UseShellExecute = false;
                    toraw.StartInfo.RedirectStandardOutput = true;
                    toraw.StartInfo.RedirectStandardError = true;
                    toraw.Start();
                    toraw.WaitForExit();

                    var exitCode = toraw.ExitCode;
                    // 0 = succesfull
                    // 1 = partially succesful
                    // 2 = failed
                    if (exitCode != 0)
                    {
                        var standardError = toraw.StandardError.ReadToEnd();
                        Console.Out.WriteLine(standardError);
                        return null;
                    }

#if DEBUG
                    var standardOutput = toraw.StandardOutput.ReadToEnd();
                    Console.Out.WriteLine(standardOutput);
#endif

                    float[] floatBuffer;
                    FileStream fs = null;
                    try
                    {
                        var fi = new FileInfo(raw);
                        fs = fi.OpenRead();
                        var bytes = (int)fi.Length;
                        var samples = bytes / sizeof(float);
                        if (samples * sizeof(float) != bytes)
                            return null;

                        // if the audio file is larger than seconds to analyze,
                        // find a proper section to exctract
                        if (bytes > secondsToAnalyze * srate * sizeof(float))
                        {
                            var seekto = bytes / 2 - secondsToAnalyze / 2 * sizeof(float) * srate;
                            Dbg.WriteLine("Extracting section: seekto = " + seekto);
                            bytes = secondsToAnalyze * srate * sizeof(float);
                            fs.Seek((samples / 2 - secondsToAnalyze / 2 * srate) * sizeof(float), SeekOrigin.Begin);
                        }

                        var br = new BinaryReader(fs);

                        var bytesBuffer = new byte[bytes];
                        br.Read(bytesBuffer, 0, bytesBuffer.Length);

                        var items = bytes / sizeof(float);
                        floatBuffer = new float[items];

                        for (var i = 0; i < items; i++)
                            floatBuffer[i] = BitConverter.ToSingle(bytesBuffer, i * sizeof(float)); // * 65536.0f;
                    }
                    catch (FileNotFoundException)
                    {
                        floatBuffer = null;
                    }
                    finally
                    {
                        if (fs != null)
                            fs.Close();
                        try
                        {
                            File.Delete(tempFile);
                            File.Delete(raw);
                        }
                        catch (IOException io)
                        {
                            Console.WriteLine(io);
                        }

                        Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
                    }

                    return floatBuffer;
                }
            }
        }

        public static float[] DecodeUsingMplayerAndSox(string fileIn, int srate, int secondsToAnalyze)
        {
            lock (_locker)
            {
                using (var tosoxreadable = new Process())
                {
                    fileIn = Regex.Replace(fileIn, "%20", " ");
                    var t = new DbgTimer();
                    t.Start();
                    var curdir = Environment.CurrentDirectory;
                    Dbg.WriteLine("Decoding: " + fileIn);
                    var tempFile = Path.GetTempFileName();
                    var soxreadablewav = tempFile + ".wav";
                    Dbg.WriteLine("Temporary wav file: " + soxreadablewav);

                    tosoxreadable.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
                    tosoxreadable.StartInfo.Arguments = " -quiet -vc null -vo null -ao pcm:fast:waveheader \"" +
                                                        fileIn + "\" -ao pcm:file=\\\"" + soxreadablewav + "\\\"";
                    tosoxreadable.StartInfo.UseShellExecute = false;
                    tosoxreadable.StartInfo.RedirectStandardOutput = true;
                    tosoxreadable.StartInfo.RedirectStandardError = true;
                    tosoxreadable.Start();
                    tosoxreadable.WaitForExit();

                    var exitCode = tosoxreadable.ExitCode;
                    // 0 = succesfull
                    // 1 = partially succesful
                    // 2 = failed
                    if (exitCode != 0)
                    {
                        var standardError = tosoxreadable.StandardError.ReadToEnd();
                        Console.Out.WriteLine(standardError);
                        return null;
                    }

#if DEBUG
                    var standardOutput = tosoxreadable.StandardOutput.ReadToEnd();
                    Console.Out.WriteLine(standardOutput);
#endif

                    float[] floatBuffer = null;
                    if (File.Exists(soxreadablewav))
                    {
                        floatBuffer = DecodeUsingSox(soxreadablewav, srate, secondsToAnalyze);
                        try
                        {
                            File.Delete(tempFile);
                            File.Delete(soxreadablewav);
                        }
                        catch (IOException io)
                        {
                            Console.WriteLine(io);
                        }
                    }

                    Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
                    return floatBuffer;
                }
            }
        }

        public static float[] DecodeUsingMplayer(string fileIn, int srate)
        {
            lock (_locker)
            {
                using (var towav = new Process())
                {
                    fileIn = Regex.Replace(fileIn, "%20", " ");
                    var t = new DbgTimer();
                    t.Start();
                    var curdir = Environment.CurrentDirectory;
                    Dbg.WriteLine("Decoding: " + fileIn);
                    var tempFile = Path.GetTempFileName();
                    var wav = tempFile + ".wav";
                    Dbg.WriteLine("Temporary wav file: " + wav);

                    towav.StartInfo.FileName = "./NativeLibraries\\mplayer\\mplayer.exe";
                    towav.StartInfo.Arguments = " -quiet -ao pcm:fast:waveheader \"" + fileIn +
                                                "\" -format floatle -af resample=" + srate +
                                                ":0:2,pan=1:0.5:0.5 -channels 1 -vo null -vc null -ao pcm:file=\\\"" +
                                                wav + "\\\"";
                    towav.StartInfo.UseShellExecute = false;
                    towav.StartInfo.RedirectStandardOutput = true;
                    towav.StartInfo.RedirectStandardError = true;
                    towav.Start();
                    towav.WaitForExit();

                    var exitCode = towav.ExitCode;
                    // 0 = succesfull
                    // 1 = partially succesful
                    // 2 = failed
                    if (exitCode != 0)
                    {
                        var standardError = towav.StandardError.ReadToEnd();
                        Console.Out.WriteLine(standardError);
                        return null;
                    }

#if DEBUG
                    var standardOutput = towav.StandardOutput.ReadToEnd();
                    Console.Out.WriteLine(standardOutput);
#endif

                    var riff = new RiffRead(wav);
                    riff.Process();
                    var floatBuffer = riff.SoundData[0];
                    try
                    {
                        File.Delete(tempFile);
                        //File.Delete(wav);
                    }
                    catch (IOException io)
                    {
                        Console.WriteLine(io);
                    }

                    Dbg.WriteLine("Decoding Execution Time: " + t.Stop().TotalMilliseconds + " ms");
                    return floatBuffer;
                }
            }
        }
    }
}