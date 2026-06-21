using System;
using SkiaSharp;

namespace dotnetANPR.Extensions;

/// <summary>
/// Extension methods for SKBitmap to support SkiaSharp-based image processing.
/// </summary>
internal static class SkiaBitmapExtensions
{
    private static T Clamp<T>(T value, T min, T max) where T : IComparable
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;
        return value;
    }

    /// <summary>
    /// Applies a convolution filter to the bitmap.
    /// </summary>
    public static SKBitmap Convolve(this SKBitmap image, float[,] kernel)
    {
        var kernelSize = (int)Math.Sqrt(kernel.Length);
        var kernelOffset = kernelSize / 2;
        var width = image.Width;
        var height = image.Height;

        var result = new SKBitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sumR = 0, sumG = 0, sumB = 0;

                for (int ky = -kernelOffset; ky <= kernelOffset; ky++)
                {
                    for (int kx = -kernelOffset; kx <= kernelOffset; kx++)
                    {
                        var px = Clamp(x + kx, 0, width - 1);
                        var py = Clamp(y + ky, 0, height - 1);
                        var kv = kernel[ky + kernelOffset, kx + kernelOffset];
                        var srcColor = image.GetPixel(px, py);

                        sumR += srcColor.Red * kv;
                        sumG += srcColor.Green * kv;
                        sumB += srcColor.Blue * kv;
                    }
                }

                var r = (byte)Clamp((int)sumR, 0, 255);
                var g = (byte)Clamp((int)sumG, 0, 255);
                var b = (byte)Clamp((int)sumB, 0, 255);
                result.SetPixel(x, y, new SKColor(r, g, b));
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts a rectangular sub-region from the bitmap.
    /// </summary>
    public static SKBitmap SubImage(this SKBitmap source, int x, int y, int width, int height)
    {
        var subset = new SKBitmap(width, height);

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                var color = source.GetPixel(x + dx, y + dy);
                subset.SetPixel(dx, dy, color);
            }
        }

        return subset;
    }

    /// <summary>
    /// Resizes a bitmap using high-quality scaling.
    /// </summary>
    public static SKBitmap LinearResizeImage(this SKBitmap source, int newWidth, int newHeight)
    {
        var resized = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(resized);

        var scale = SKMatrix.CreateScale((float)newWidth / source.Width, (float)newHeight / source.Height);
        canvas.SetMatrix(scale);

        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        canvas.DrawBitmap(source, 0, 0, paint);
        return resized;
    }

    /// <summary>
    /// Creates a blank (white) bitmap of specified dimensions.
    /// </summary>
    public static SKBitmap CreateBlankBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        return bitmap;
    }

    /// <summary>
    /// Applies thresholding to convert image to black and white.
    /// </summary>
    public static void Thresholding(SKBitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                // Use luminance to determine threshold
                var gray = (byte)(color.Red * 0.299f + color.Green * 0.587f + color.Blue * 0.114f);

                byte newGray = gray < 36 ? (byte)0 : gray;
                bitmap.SetPixel(x, y, new SKColor(newGray, newGray, newGray));
            }
        }
    }

    /// <summary>
    /// Adds two bitmaps together (pixel-wise addition).
    /// </summary>
    public static SKBitmap SumBitmaps(SKBitmap image1, SKBitmap image2)
    {
        var outWidth = Math.Max(image1.Width, image2.Width);
        var outHeight = Math.Max(image1.Height, image2.Height);
        var outImage = new SKBitmap(outWidth, outHeight);

        for (int y = 0; y < outHeight; y++)
        {
            for (int x = 0; x < outWidth; x++)
            {
                var c1 = x < image1.Width && y < image1.Height ? image1.GetPixel(x, y) : SKColors.Black;
                var c2 = x < image2.Width && y < image2.Height ? image2.GetPixel(x, y) : SKColors.Black;

                var r = (byte)Math.Min(c1.Red + c2.Red, 255);
                var g = (byte)Math.Min(c1.Green + c2.Green, 255);
                var b = (byte)Math.Min(c1.Blue + c2.Blue, 255);

                outImage.SetPixel(x, y, new SKColor(r, g, b));
            }
        }

        return outImage;
    }
}
