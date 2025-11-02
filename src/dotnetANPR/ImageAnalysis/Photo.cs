using SkiaSharp;
using System;
using System.IO;

namespace DotNetANPR.ImageAnalysis
{
    /// <summary>
    /// Base class for image manipulation, replacing Photo.java.
    /// Uses SkiaSharp.SKBitmap. Implements IDisposable.
    /// </summary>
    public class Photo : IDisposable
    {
        protected SKBitmap _bitmap;
        private float[,] _brightnessCache;
        private bool _disposed = false;

        public int Width => _bitmap?.Width ?? 0;
        public int Height => _bitmap?.Height ?? 0;
        public SKImageInfo Info => _bitmap.Info;

        public Photo(string filepath)
        {
            _bitmap = SKBitmap.Decode(filepath);
            if (_bitmap == null)
            {
                throw new IOException($"Couldn't read input file {filepath}");
            }
            EnsureCorrectColorType();
        }

        public Photo(SKBitmap bitmap)
        {
            _bitmap = bitmap; // Takes ownership
            EnsureCorrectColorType();
        }

        private void EnsureCorrectColorType()
        {
            if (_bitmap.ColorType != SKColorType.Bgra8888)
            {
                var temp = _bitmap;
                _bitmap = new SKBitmap(temp.Width, temp.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);
                using var canvas = new SKCanvas(_bitmap);
                canvas.DrawBitmap(temp, 0, 0);
                temp.Dispose();
            }
        }

        public SKBitmap GetBitmap() => _bitmap;

        public Photo Clone()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            return new Photo(_bitmap.Copy());
        }

        /// <summary>
        /// Lazily populates and returns a 2D array of brightness values (0-1).
        /// This is a major optimization.
        /// </summary>
        public float[,] GetBrightnessMatrix()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            if (_brightnessCache != null) return _brightnessCache;

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

        public float GetBrightness(int x, int y) => GetBrightnessMatrix()[x, y];
        public float GetSaturation(int x, int y) => _bitmap.GetPixel(x, y).GetSaturation();
        public float GetHue(int x, int y) => _bitmap.GetPixel(x, y).GetHue();

        public void SetBrightness(int x, int y, float value)
        {
            _bitmap.SetPixel(x, y, ImageUtils.ToGrayscaleColor(value));
            ClearBrightnessCache();
        }

        protected void ClearBrightnessCache() => _brightnessCache = null;

        public void Resize(int width, int height, SKFilterQuality quality = SKFilterQuality.High)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            var newInfo = new SKImageInfo(width, height, Info.ColorType, SKAlphaType.Opaque);
            var newBitmap = _bitmap.Resize(newInfo, quality);
            _bitmap.Dispose();
            _bitmap = newBitmap;
            ClearBrightnessCache();
        }

        public void Rotate(double angleDegrees)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));

            double angleRad = Math.PI * angleDegrees / 180.0;
            float sine = (float)Math.Abs(Math.Sin(angleRad));
            float cosine = (float)Math.Abs(Math.Cos(angleRad));
            int newWidth = (int)Math.Round(Width * cosine + Height * sine);
            int newHeight = (int)Math.Round(Width * sine + Height * cosine);

            var newBitmap = new SKBitmap(newWidth, newHeight, Info.ColorType, SKAlphaType.Opaque);
            using var canvas = new SKCanvas(newBitmap);
            canvas.Clear(SKColors.White);
            canvas.Translate(newWidth / 2.0f, newHeight / 2.0f);
            canvas.RotateDegrees((float)angleDegrees);
            canvas.Translate(-Width / 2.0f, -Height / 2.0f);
            canvas.DrawBitmap(_bitmap, 0, 0);

            _bitmap.Dispose();
            _bitmap = newBitmap;
            ClearBrightnessCache();
        }

        public void AdaptiveThresholding(int radius)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Photo));
            
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
}