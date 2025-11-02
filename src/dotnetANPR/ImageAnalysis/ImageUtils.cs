using SkiaSharp;
using System;

namespace DotNetANPR.ImageAnalysis;

public static class ImageUtils
{
    /// <summary>
    /// Gets brightness (Value) from an SKColor (range 0-1).
    /// </summary>
    public static float GetBrightness(this SKColor color)
    {
        color.ToHsv(out _, out _, out float v);
        return v / 100.0f;
    }

    public static float GetSaturation(this SKColor color)
    {
        color.ToHsv(out _, out float s, out _);
        return s / 100.0f;
    }

    public static float GetHue(this SKColor color)
    {
        color.ToHsv(out float h, out _, out _);
        return h / 360.0f;
    }

    /// <summary>
    /// Creates a grayscale SKColor from a 0-1 brightness value.
    /// </summary>
    public static SKColor ToGrayscaleColor(float brightness)
    {
        byte val = (byte)(Math.Clamp(brightness, 0f, 1f) * 255);
        return new SKColor(val, val, val);
    }

    /// <summary>
    /// Applies a convolution kernel filter to a bitmap.
    /// </summary>
    public static SKBitmap Convolve(SKBitmap src, float[] kernel, int kernelWidth, int kernelHeight)
    {
        if (kernel.Length != kernelWidth * kernelHeight)
            throw new ArgumentException("Kernel size does not match dimensions.");

        var result = new SKBitmap(src.Info);
        using var canvas = new SKCanvas(result);

        using var filter = SKImageFilter.CreateMatrixConvolution(
            kernelSize: new SKSizeI(kernelWidth, kernelHeight),
            kernel: kernel,
            gain: 1.0f,
            bias: 0.0f,
            kernelOffset: new SKPointI(kernelWidth / 2, kernelHeight / 2),
            tileMode: SKShaderTileMode.Clamp,
            convolveAlpha: false
        );

        using var paint = new SKPaint { ImageFilter = filter };
        canvas.DrawBitmap(src, 0, 0, paint);

        return result;
    }
}