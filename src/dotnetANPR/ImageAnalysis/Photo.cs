using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DotNetANPR.Configuration;


using SkiaSharp;
using System;
using System.IO;


namespace DotNetANPR.ImageAnalysis;
    public class Photo : IDisposable
    {
        protected SKBitmap _bitmap;
        private float[,] _brightnessCache;
        private bool _disposed = false;

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;
        public SKImageInfo Info => _bitmap.Info;
        public int Square => Width * Height;

        public Photo(string filepath)
        {
            _bitmap = SKBitmap.Decode(filepath);
            if (_bitmap == null)
            {
                throw new IOException($"{{Error in image loader}} Couldn't read input file {filepath}");
            }
            
            // Ensure 8888 color type for consistency
            if (_bitmap.ColorType != SKColorType.Bgra8888)
            {
                var temp = _bitmap;
                _bitmap = new SKBitmap(temp.Width, temp.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
                using var canvas = new SKCanvas(_bitmap);
                canvas.DrawBitmap(temp, 0, 0);
                temp.Dispose();
            }
        }

        public Photo(SKBitmap bitmap)
        {
            _bitmap = bitmap; // Takes ownership
        }

        public SKBitmap GetBitmap() => _bitmap;

        public Photo Clone()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            return new Photo(_bitmap.Copy());
        }

        public void SaveImage(string filepath)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            
            var format = Path.GetExtension(filepath).ToUpperInvariant() switch
            {
                ".JPG" or ".JPEG" => SKEncodedImageFormat.Jpeg,
                ".PNG" => SKEncodedImageFormat.Png,
                ".BMP" => SKEncodedImageFormat.Bmp,
                _ => throw new IOException("Unsupported file format")
            };

            using var stream = File.OpenWrite(filepath);
            _bitmap.Encode(stream, format, 90);
        }

        // --- Optimized Pixel Access ---

        /// <summary>
        /// Lazily populates and returns a 2D array of brightness values (0-1).
        /// This is a major optimization over the original.
        /// </summary>
        public float[,] GetBrightnessMatrix()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));

            if (_brightnessCache != null)
            {
                return _brightnessCache;
            }

            _brightnessCache = new float[Width, Height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _brightnessCache[x, y] = _bitmap.GetPixel(x, y).GetBrightness();
                }
            }
            return _brightnessCache;
        }

        public float GetBrightness(int x, int y)
        {
            // Uses the fast cache
            return GetBrightnessMatrix()[x, y];
        }

        public float GetSaturation(int x, int y)
        {
             if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            return _bitmap.GetPixel(x, y).GetSaturation();
        }

        public float GetHue(int x, int y)
        {
             if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            return _bitmap.GetPixel(x, y).GetHue();
        }

        public void SetBrightness(int x, int y, float value)
        {
             if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            _bitmap.SetPixel(x, y, ImageUtils.ToGrayscaleColor(value));
            ClearBrightnessCache(); // Invalidate cache
        }
        
        protected void ClearBrightnessCache()
        {
            _brightnessCache = null;
        }

        // --- Filters & Transforms ---

        public void Resize(int width, int height, SKFilterQuality quality = SKFilterQuality.High)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            
            var newInfo = new SKImageInfo(width, height, _bitmap.ColorType, _bitmap.AlphaType);
            var newBitmap = _bitmap.Resize(newInfo, quality);
            
            _bitmap.Dispose();
            _bitmap = newBitmap;
            ClearBrightnessCache();
        }

        public void VerticalEdgeDetector()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            
            float[] kernel = {
                -1, 0, 1,
                -2, 0, 2,
                -1, 0, 1
            };

            var newBitmap = ImageUtils.Convolve(_bitmap, kernel, 3, 3);
            _bitmap.Dispose();
            _bitmap = newBitmap;
            ClearBrightnessCache();
        }

        public void PlainThresholding()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            var stats = new Statistics(this); // Assumes Statistics class is converted
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float brightness = _bitmap.GetPixel(x, y).GetBrightness();
                    float thresholded = stats.ThresholdBrightness(brightness, 1.0f);
                    _bitmap.SetPixel(x, y, ImageUtils.ToGrayscaleColor(thresholded));
                }
            }
            ClearBrightnessCache();
        }

        public void AdaptiveThresholding(int radius)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            if (radius == 0)
            {
                PlainThresholding();
                return;
            }
            
            float[,] sourceMatrix = GetBrightnessMatrix(); // Optimized
            var destBitmap = new SKBitmap(Info);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float neighborhoodSum = 0;
                    int count = 0;

                    int yMin = Math.Max(0, y - radius);
                    int yMax = Math.Min(Height - 1, y + radius);
                    int xMin = Math.Max(0, x - radius);
                    int xMax = Math.Min(Width - 1, x + radius);

                    for (int iy = yMin; iy <= yMax; iy++)
                    {
                        for (int ix = xMin; ix <= xMax; ix++)
                        {
                            neighborhoodSum += sourceMatrix[ix, iy];
                            count++;
                        }
                    }

                    float neighborhoodAvg = neighborhoodSum / count;
                    float pixelVal = sourceMatrix[x, y] < neighborhoodAvg ? 0f : 1f;
                    destBitmap.SetPixel(x, y, ImageUtils.ToGrayscaleColor(pixelVal));
                }
            }
            
            _bitmap.Dispose();
            _bitmap = destBitmap;
            ClearBrightnessCache();
        }

        public HoughTransformation GetHoughTransformation()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            
            var hough = new HoughTransformation(Width, Height);
            float[,] brightnessMatrix = GetBrightnessMatrix(); // Optimized
            
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    hough.AddLine(x, y, brightnessMatrix[x, y]);
                }
            }
            return hough;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _bitmap?.Dispose();
                }
                _bitmap = null;
                _brightnessCache = null;
                _disposed = true;
            }
        }
    }