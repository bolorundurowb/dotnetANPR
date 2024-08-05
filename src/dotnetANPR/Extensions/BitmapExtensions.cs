using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace DotNetANPR.Extensions;

internal static class BitmapExtensions
{
    public static void ConvolutionFilter(this Bitmap source, Bitmap destination, float[] kernel)
    {
        var kernelSize = (int)Math.Sqrt(kernel.Length);
        var kernelRadius = kernelSize / 2;
        var srcData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);
        var dstData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        unsafe
        {
            var srcPtr = (byte*)srcData.Scan0.ToPointer();
            var dstPtr = (byte*)dstData.Scan0.ToPointer();

            for (var y = kernelRadius; y < source.Height - kernelRadius; y++)
            for (var x = kernelRadius; x < source.Width - kernelRadius; x++)
            {
                float sum = 0;
                for (var ky = -kernelRadius; ky <= kernelRadius; ky++)
                for (var kx = -kernelRadius; kx <= kernelRadius; kx++)
                {
                    var pixelX = x + kx;
                    var pixelY = y + ky;
                    var pixelPtr = srcPtr + pixelY * srcData.Stride + pixelX * 3;
                    var pixelBrightness = (pixelPtr[0] + pixelPtr[1] + pixelPtr[2]) / 3.0f / 255.0f;
                    sum += pixelBrightness * kernel[(ky + kernelRadius) * kernelSize + kx + kernelRadius];
                }

                var brightnessByte = (byte)(sum * 255);
                var dstPixelPtr = dstPtr + y * dstData.Stride + x * 3;
                dstPixelPtr[0] = dstPixelPtr[1] = dstPixelPtr[2] = brightnessByte;
            }
        }

        source.UnlockBits(srcData);
        destination.UnlockBits(dstData);
    }
    
    public static Bitmap SubImage(this Bitmap source, int x, int y, int width, int height)
    {
        var rect = new Rectangle(x, y, width, height);
        return source.Clone(rect, source.PixelFormat);
    }
}
