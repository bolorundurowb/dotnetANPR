using System;
using SkiaSharp;

namespace DotNetANPR.Extensions;

/// <summary>
/// Extension methods for <see cref="SKBitmap"/> providing convolution and sub-image extraction.
/// </summary>
internal static class BitmapExtensions
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
    /// Applies a convolution kernel to the bitmap and returns a new filtered bitmap.
    /// </summary>
    /// <param name="image">The source bitmap.</param>
    /// <param name="kernel">The convolution kernel as a 2D float array.</param>
    /// <returns>A new bitmap with the convolution applied.</returns>
    public static SKBitmap Convolve(this SKBitmap image, float[,] kernel)
    {
        var kernelHeight = kernel.GetLength(0);
        var kernelWidth = kernel.GetLength(1);
        var kernelOffsetY = kernelHeight / 2;
        var kernelOffsetX = kernelWidth / 2;

        var result = new SKBitmap(image.Width, image.Height, image.ColorType, image.AlphaType);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                float r = 0, g = 0, b = 0;

                for (var ky = 0; ky < kernelHeight; ky++)
                {
                    for (var kx = 0; kx < kernelWidth; kx++)
                    {
                        var px = Clamp(x + kx - kernelOffsetX, 0, image.Width - 1);
                        var py = Clamp(y + ky - kernelOffsetY, 0, image.Height - 1);

                        var pixel = image.GetPixel(px, py);
                        r += pixel.Red * kernel[ky, kx];
                        g += pixel.Green * kernel[ky, kx];
                        b += pixel.Blue * kernel[ky, kx];
                    }
                }

                r = Clamp(r, 0f, 255f);
                g = Clamp(g, 0f, 255f);
                b = Clamp(b, 0f, 255f);

                result.SetPixel(x, y, new SKColor((byte)r, (byte)g, (byte)b));
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts a rectangular sub-region from the bitmap.
    /// </summary>
    /// <param name="source">The source bitmap.</param>
    /// <param name="x">The x-coordinate of the top-left corner.</param>
    /// <param name="y">The y-coordinate of the top-left corner.</param>
    /// <param name="width">The width of the sub-region.</param>
    /// <param name="height">The height of the sub-region.</param>
    /// <returns>A new bitmap containing the extracted sub-region.</returns>
    public static SKBitmap SubImage(this SKBitmap source, int x, int y, int width, int height)
    {
        var subset = new SKBitmap();
        source.ExtractSubset(subset, SKRectI.Create(x, y, width, height));
        return subset;
    }
}
