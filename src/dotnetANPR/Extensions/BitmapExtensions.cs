using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace dotnetANPR.Extensions;

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

    public static Bitmap Convolve(this Bitmap image, float[,] kernel)
    {
        var kernelSize = (int)Math.Sqrt(kernel.Length);
        var kernelOffset = kernelSize / 2;
        var width = image.Width;
        var height = image.Height;

        var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var rect = new Rectangle(0, 0, width, height);

        // Lock source as 24bppRgb (GDI+ converts if needed); lock dest for writing
        var srcData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var dstData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        var stride = srcData.Stride; // same for both since both are 24bppRgb
        var srcBytes = new byte[Math.Abs(stride) * height];
        var dstBytes = new byte[Math.Abs(stride) * height];

        Marshal.Copy(srcData.Scan0, srcBytes, 0, srcBytes.Length);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                float sumR = 0, sumG = 0, sumB = 0;

                for (var ky = -kernelOffset; ky <= kernelOffset; ky++)
                {
                    for (var kx = -kernelOffset; kx <= kernelOffset; kx++)
                    {
                        var px = Clamp(x + kx, 0, width - 1);
                        var py = Clamp(y + ky, 0, height - 1);
                        var kv = kernel[ky + kernelOffset, kx + kernelOffset];
                        var srcIdx = py * stride + px * 3;

                        sumB += srcBytes[srcIdx]     * kv; // B
                        sumG += srcBytes[srcIdx + 1] * kv; // G
                        sumR += srcBytes[srcIdx + 2] * kv; // R
                    }
                }

                var dstIdx = y * stride + x * 3;
                dstBytes[dstIdx]     = (byte)Clamp((int)sumB, 0, 255); // B
                dstBytes[dstIdx + 1] = (byte)Clamp((int)sumG, 0, 255); // G
                dstBytes[dstIdx + 2] = (byte)Clamp((int)sumR, 0, 255); // R
            }
        }

        Marshal.Copy(dstBytes, 0, dstData.Scan0, dstBytes.Length);
        image.UnlockBits(srcData);
        result.UnlockBits(dstData);

        return result;
    }

    public static Bitmap SubImage(this Bitmap source, int x, int y, int width, int height)
    {
        var rect = new Rectangle(x, y, width, height);
        return source.Clone(rect, source.PixelFormat);
    }
}