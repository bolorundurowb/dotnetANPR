using System;
using System.IO;
using SkiaSharp;

namespace dotnetANPR.Extensions;

/// <summary>
/// Adapter class providing compatibility between System.Drawing patterns and SkiaSharp.
/// Simplifies migration by offering familiar Bitmap-like interface via SKBitmap.
/// </summary>
internal static class SkiaSharpAdapter
{
    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// Creates a new SKBitmap from a file path.
    /// </summary>
    public static SKBitmap LoadBitmap(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return SKBitmap.Decode(stream);
    }

    /// <summary>
    /// Saves an SKBitmap to a file in JPEG format.
    /// </summary>
    public static void SaveAsJpeg(SKBitmap bitmap, string filePath, int quality = 90)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        using var stream = File.Create(filePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Saves an SKBitmap to a file (auto-detects format from extension).
    /// </summary>
    public static void Save(SKBitmap bitmap, string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var format = ext switch
        {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".png" => SKEncodedImageFormat.Png,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png
        };

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, format == SKEncodedImageFormat.Jpeg ? 90 : 100);
        using var stream = File.Create(filePath);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Creates a duplicate/clone of an SKBitmap.
    /// </summary>
    public static SKBitmap DuplicateBitmap(SKBitmap source)
    {
        return source.Copy();
    }

    /// <summary>
    /// Gets the brightness value of a pixel (0-1 range).
    /// </summary>
    public static float GetBrightness(SKBitmap bitmap, int x, int y)
    {
        var pixmap = bitmap.PeekPixels();
        if (pixmap != null)
        {
            var color = pixmap.GetPixelColor(x, y);
            return (color.Red * 0.299f + color.Green * 0.587f + color.Blue * 0.114f) / 255f;
        }

        var fallback = bitmap.GetPixel(x, y);
        return (fallback.Red * 0.299f + fallback.Green * 0.587f + fallback.Blue * 0.114f) / 255f;
    }

    public static void SetBrightness(SKBitmap bitmap, int x, int y, float value)
    {
        var gray = (byte)(Clamp(value, 0, 1) * 255);
        bitmap.SetPixel(x, y, new SKColor(gray, gray, gray));
    }

    /// <summary>
    /// Gets the HSL saturation of a pixel (0-1 range).
    /// </summary>
    public static float GetSaturation(SKBitmap bitmap, int x, int y)
    {
        var color = bitmap.GetPixel(x, y);
        // Convert RGB to HSL and extract saturation
        var max = Math.Max(color.Red, Math.Max(color.Green, color.Blue));
        var min = Math.Min(color.Red, Math.Min(color.Green, color.Blue));
        var l = (max + min) / 2f / 255f;

        if (max == min)
            return 0;

        var s = l < 0.5f ? (max - min) / (max + min) : (max - min) / (510 - max - min);
        return s;
    }

    /// <summary>
    /// Gets the HSL hue of a pixel (0-1 range, normalized from 0-360).
    /// </summary>
    public static float GetHue(SKBitmap bitmap, int x, int y)
    {
        var color = bitmap.GetPixel(x, y);
        var r = color.Red / 255f;
        var g = color.Green / 255f;
        var b = color.Blue / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        float hue = 0;
        if (delta != 0)
        {
            if (max == r)
                hue = 60 * (((g - b) / delta % 6));
            else if (max == g)
                hue = 60 * (((b - r) / delta) + 2);
            else
                hue = 60 * (((r - g) / delta) + 4);
        }

        if (hue < 0) hue += 360;
        return hue / 360f;
    }

    /// <summary>
    /// Converts a float array to an SKBitmap (grayscale, values 0-1 mapped to 0-255).
    /// </summary>
    public static SKBitmap ArrayToBitmap(float[,] array, int width, int height)
    {
        var bitmap = new SKBitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var gray = (byte)(Clamp(array[x, y], 0, 1) * 255);
                var color = new SKColor(gray, gray, gray);
                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Converts an SKBitmap to a float array (grayscale, values 0-1).
    /// </summary>
    public static float[,] BitmapToArray(SKBitmap bitmap, int width, int height)
    {
        var array = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                // Convert to grayscale using standard formula
                var brightness = (color.Red * 0.299f + color.Green * 0.587f + color.Blue * 0.114f) / 255f;
                array[x, y] = brightness;
            }
        }

        return array;
    }
}
