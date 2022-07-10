using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CommonUtils;
using Soundfingerprinting.Audio.Strides;
using Soundfingerprinting.Fingerprinting.FFT;
using Soundfingerprinting.Fingerprinting.Wavelets;

namespace Soundfingerprinting.Image
{
    public class ImageService
    {
        private const int SpaceBetweenImages = 10; /*10 pixel space between fingerprint images*/

        private readonly SpectrumService spectrumService;

        private readonly IWaveletService waveletService;

        public ImageService(SpectrumService spectrumService, IWaveletService waveletService)
        {
            this.spectrumService = spectrumService;
            this.waveletService = waveletService;
        }

        public System.Drawing.Image GetImageForFingerprint(bool[] data, int width, int height)
        {
            var image = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            DrawFingerprintInImage(image, data, width, height, 0, 0);
            return image;
        }

        public System.Drawing.Image GetImageForFingerprints(List<bool[]> fingerprints, int width, int height,
            int fingerprintsPerRow)
        {
            var imagesPerRow = fingerprintsPerRow; /*5 bitmap images per line*/
            var fingersCount = fingerprints.Count;
            var rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
            var imageWidth = rowCount == 1
                ? width + 2 * SpaceBetweenImages
                : imagesPerRow * (width + SpaceBetweenImages) + SpaceBetweenImages;
            var imageHeight = rowCount * (height + SpaceBetweenImages) + SpaceBetweenImages;

            var image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
            SetBackground(image, Color.White);

            var verticalOffset = SpaceBetweenImages;
            var horizontalOffset = SpaceBetweenImages;
            var count = 0;
            foreach (var fingerprint in fingerprints)
            {
                DrawFingerprintInImage(image, fingerprint, width, height, horizontalOffset, verticalOffset);
                count++;
                if (count % imagesPerRow == 0)
                {
                    verticalOffset += height + SpaceBetweenImages;
                    horizontalOffset = SpaceBetweenImages;
                }
                else
                {
                    horizontalOffset += width + SpaceBetweenImages;
                }
            }

            return image;
        }

        public System.Drawing.Image GetSignalImage(float[] data, int width, int height)
        {
            var image = new Bitmap(width, height);
            var graphics = Graphics.FromImage(image);

            FillBackgroundColor(width, height, graphics, Color.Black);
            DrawGridlines(width, height, graphics);

            var center = height / 2;
            /*Draw lines*/
            using (var pen = new Pen(Color.MediumSpringGreen, 1))
            {
                /*Find delta X, by which the lines will be drawn*/
                var deltaX = (double)width / data.Length;
                var normalizeFactor = data.Max(a => Math.Abs(a)) / ((double)height / 2);
                for (int i = 0, n = data.Length; i < n; i++)
                    graphics.DrawLine(
                        pen,
                        (float)(i * deltaX),
                        center,
                        (float)(i * deltaX),
                        (float)(center - data[i] / normalizeFactor));
            }

            using (var pen = new Pen(Color.DarkGreen, 1))
            {
                /*Draw center line*/
                graphics.DrawLine(pen, 0, center, width, center);
            }

            return image;
        }

        public System.Drawing.Image GetSpectrogramImage(double[][] spectrum, int width, int height)
        {
            // set some default values
            var usePowerSpectrum = false;
            var colorize = true;
            var flipYscale = true;
            var forceWidth = width;
            var forceHeight = height;

            // amplitude (or magnitude) is the square root of the power spectrum
            // the magnitude spectrum is abs(fft), i.e. Math.Sqrt(re*re + img*img)
            // use 20*log10(Y) to get dB from amplitude
            // the power spectrum is the magnitude spectrum squared
            // use 10*log10(Y) to get dB from power spectrum
            var maxValue = spectrum.Max(b => b.Max(v => Math.Abs(v)));
            if (usePowerSpectrum)
                maxValue = 10 * Math.Log10(maxValue);
            else
                maxValue = 20 * Math.Log10(maxValue);

            if (maxValue == 0.0f)
                return null;

            var blockSizeX = 1;
            var blockSizeY = 1;

            var rowCount = spectrum[0].Length;
            var columnCount = spectrum.Length;

            var img = new Bitmap(columnCount * blockSizeX, rowCount * blockSizeY);
            var graphics = Graphics.FromImage(img);

            for (var column = 0; column < columnCount; column++)
            for (var row = 0; row < rowCount; row++)
            {
                var val = spectrum[column][row];
                if (usePowerSpectrum)
                    val = 10 * Math.Log10(val);
                else
                    val = 20 * Math.Log10(val);

                var color = ColorUtils.ValueToBlackWhiteColor(val, maxValue);
                Brush brush = new SolidBrush(color);

                if (flipYscale) // draw a small square
                    graphics.FillRectangle(brush, column * blockSizeX, (rowCount - row - 1) * blockSizeY, blockSizeX,
                        blockSizeY);
                else // draw a small square
                    graphics.FillRectangle(brush, column * blockSizeX, row * blockSizeY, blockSizeX, blockSizeY);
            }

            // Should we resize?
            if (forceHeight > 0 && forceWidth > 0) img = (Bitmap)ImageUtils.Resize(img, forceWidth, forceHeight, false);

            // Should we colorize?
            if (colorize) img = ColorUtils.Colorize(img, 255, ColorUtils.ColorPaletteType.MATLAB);

            return img;
        }

        public System.Drawing.Image GetLogSpectralImages(
            double[][] spectrum,
            IStride strideBetweenConsecutiveImages,
            int fingerprintLength,
            int overlap,
            int imagesPerRow)
        {
            var spectralImages = spectrumService.CutLogarithmizedSpectrum(
                spectrum, strideBetweenConsecutiveImages, fingerprintLength, overlap);

            return GetLogSpectralImages(spectralImages, imagesPerRow);
        }

        public System.Drawing.Image GetLogSpectralImages(List<double[][]> spectralImages, int imagesPerRow)
        {
            var blockSizeX = 4;
            var blockSizeY = 4;

            var width = spectralImages[0].GetLength(0);
            var height = spectralImages[0][0].Length;
            var fingersCount = spectralImages.Count;
            var rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);
            var imageWidth = rowCount == 1
                ? width * blockSizeX + 2 * SpaceBetweenImages
                : blockSizeX * imagesPerRow * (width + SpaceBetweenImages) + SpaceBetweenImages;
            var imageHeight = blockSizeY * rowCount * (height + SpaceBetweenImages) + SpaceBetweenImages;

            var image = new Bitmap(imageWidth, imageHeight);
            var graphics = Graphics.FromImage(image);
            SetBackground(image, Color.White);

            var verticalOffset = SpaceBetweenImages;
            var horizontalOffset = SpaceBetweenImages;
            var count = 0;
            foreach (var spectralImage in spectralImages)
            {
                var maxValue = spectralImage.Max(b => b.Max(v => Math.Abs(v)));
                maxValue = 20 * Math.Log10(maxValue);

                for (var i = 0; i < width /*128*/; i++)
                for (var j = 0; j < height /*32*/; j++)
                {
                    var val = 20 * Math.Log10(spectralImage[i][j]);
                    var color = Color.White;
                    if (!double.IsNaN(val) && !double.IsInfinity(val)) color = ValueToBlackWhiteColor(val, maxValue);
                    Brush brush = new SolidBrush(color);

                    // draw a small square
                    graphics.FillRectangle(brush, (i + horizontalOffset) * blockSizeX,
                        (height - j + verticalOffset + 1) * blockSizeY, blockSizeX, blockSizeY);
                }

                count++;
                if (count % imagesPerRow == 0)
                {
                    verticalOffset += height + SpaceBetweenImages;
                    horizontalOffset = SpaceBetweenImages;
                }
                else
                {
                    horizontalOffset += width + SpaceBetweenImages;
                }
            }

            image = ColorUtils.Colorize(image, 255, ColorUtils.ColorPaletteType.MATLAB);
            return image;
        }

        public System.Drawing.Image GetWaveletsImages(
            double[][] spectrum,
            IStride strideBetweenConsecutiveImages,
            int fingerprintLength,
            int overlap,
            int imagesPerRow)
        {
            var spectralImages = spectrumService.CutLogarithmizedSpectrum(
                spectrum, strideBetweenConsecutiveImages, fingerprintLength, overlap);

            waveletService.ApplyWaveletTransformInPlace(spectralImages);

            var width = spectralImages[0].GetLength(0);
            var height = spectralImages[0][0].Length;
            var fingersCount = spectralImages.Count;
            var rowCount = (int)Math.Ceiling((float)fingersCount / imagesPerRow);

            var imageWidth = rowCount == 1
                ? width + 2 * SpaceBetweenImages
                : imagesPerRow * (width + SpaceBetweenImages) + SpaceBetweenImages;
            var imageHeight = rowCount * (height + SpaceBetweenImages) + SpaceBetweenImages;

            var image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format16bppRgb565);
            SetBackground(image, Color.White);

            var verticalOffset = SpaceBetweenImages;
            var horizontalOffset = SpaceBetweenImages;
            var count = 0;
            foreach (var spectralImage in spectralImages)
            {
                var average = spectralImage.Average(col => col.Average(v => Math.Abs(v)));
                for (var i = 0; i < width /*128*/; i++)
                for (var j = 0; j < height /*32*/; j++)
                {
                    var color = ValueToBlackWhiteColor(spectralImage[i][j], average);
                    image.SetPixel(i + horizontalOffset, j + verticalOffset, color);
                }

                count++;
                if (count % imagesPerRow == 0)
                {
                    verticalOffset += height + SpaceBetweenImages;
                    horizontalOffset = SpaceBetweenImages;
                }
                else
                {
                    horizontalOffset += width + SpaceBetweenImages;
                }
            }

            return image;
        }

        public System.Drawing.Image GetWaveletTransformedImage(double[][] image, IWaveletDecomposition wavelet)
        {
            var width = image[0].Length;
            var height = image.Length;
            wavelet.DecomposeImageInPlace(image);
            var transformed = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            for (var i = 0; i < transformed.Height; i++)
            for (var j = 0; j < transformed.Width; j++)
                transformed.SetPixel(j, i, Color.FromArgb((int)image[i][j]));

            return transformed;
        }

        private Color ValueToBlackWhiteColor(double value, double maxValue)
        {
            var color = (int)(Math.Abs(value) * 255 / Math.Abs(maxValue));
            if (color > 255) color = 255;

            return Color.FromArgb(color, color, color);
        }

        private void FillBackgroundColor(int width, int height, Graphics graphics, Color color)
        {
            using (Brush brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, new Rectangle(0, 0, width, height));
            }
        }

        private void DrawGridlines(int width, int height, Graphics graphics)
        {
            const int Gridline = 50; /*Every 50 pixels draw gridline*/
            /*Draw gridlines*/
            using (var pen = new Pen(Color.Red, 1))
            {
                /*Draw horizontal gridlines*/
                for (var i = 1; i < height / Gridline; i++)
                    graphics.DrawLine(pen, 0, i * Gridline, width, i * Gridline);

                /*Draw vertical gridlines*/
                for (var i = 1; i < width / Gridline; i++)
                    graphics.DrawLine(pen, i * Gridline, 0, i * Gridline, height);
            }
        }

        private void DrawFingerprintInImage(
            Bitmap image, bool[] fingerprint, int fingerprintWidth, int fingerprintHeight, int xOffset, int yOffset)
        {
            // Scale the fingerprints and write to image
            for (var i = 0; i < fingerprintWidth /*128*/; i++)
            for (var j = 0; j < fingerprintHeight /*32*/; j++)
            {
                // if 10 or 01 element then its white
                var color = fingerprint[2 * fingerprintHeight * i + 2 * j]
                            || fingerprint[2 * fingerprintHeight * i + 2 * j + 1]
                    ? Color.White
                    : Color.Black;
                image.SetPixel(xOffset + i, yOffset + j, color);
            }
        }

        private void SetBackground(Bitmap image, Color color)
        {
            for (var i = 0; i < image.Width; i++)
            for (var j = 0; j < image.Height; j++)
                image.SetPixel(i, j, color);
        }
    }
}