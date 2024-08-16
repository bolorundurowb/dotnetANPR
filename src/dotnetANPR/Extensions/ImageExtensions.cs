using System;
using SkiaSharp;

namespace DotNetANPR.Extensions;

internal static class ImageExtensions
{
    private static T Clamp<T>(T value, T min, T max) where T : IComparable
    {
        if (value.CompareTo(min) < 0)
            return min;

        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }

    public static SKBitmap Convolve(this SKBitmap image, float[,] kernel)
    {
        var kernelSize = (int)Math.Sqrt(kernel.Length);
        var kernelOffset = kernelSize / 2;

        var result = new SKBitmap(image.Width, image.Height);

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                float r = 0, g = 0, b = 0;

                for (var ky = -kernelOffset; ky <= kernelOffset; ky++)
                {
                    for (var kx = -kernelOffset; kx <= kernelOffset; kx++)
                    {
                        var px = Clamp(x + kx, 0, image.Width - 1);
                        var py = Clamp(y + ky, 0, image.Height - 1);

                        var pixel = image.GetPixel(px, py);
                        r += pixel.Red * kernel[ky + kernelOffset, kx + kernelOffset];
                        g += pixel.Green * kernel[ky + kernelOffset, kx + kernelOffset];
                        b += pixel.Blue * kernel[ky + kernelOffset, kx + kernelOffset];
                    }
                }

                r = Clamp(r, 0, 255);
                g = Clamp(g, 0, 255);
                b = Clamp(b, 0, 255);

                result.SetPixel(x, y, SKColors.FromArgb((int)r, (int)g, (int)b));
            }
        }

        return result;
    }

    public static SKBitmap SubImage(this SKBitmap source, int x, int y, int width, int height)
    {
        var rect = new Rectangle(x, y, width, height);
        return source.Clone(rect, source.PixelFormat);
    }
}