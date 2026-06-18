using DotNetANPR.ImageAnalysis;
using SkiaSharp;
using Xunit;

namespace DotNetANPR.Tests;

public class PhotoTests
{
    [Fact]
    public void GetBrightness_BlackPixel_ReturnsZero()
    {
        using var bitmap = new SKBitmap(1, 1);
        bitmap.SetPixel(0, 0, SKColors.Black);

        var brightness = Photo.GetBrightness(bitmap, 0, 0);

        Assert.Equal(0, brightness, precision: 3);
    }

    [Fact]
    public void GetBrightness_WhitePixel_ReturnsOne()
    {
        using var bitmap = new SKBitmap(1, 1);
        bitmap.SetPixel(0, 0, SKColors.White);

        var brightness = Photo.GetBrightness(bitmap, 0, 0);

        Assert.Equal(1, brightness, precision: 3);
    }

    [Fact]
    public void CreateBlankBitmap_CreatesBlackBitmap()
    {
        using var bitmap = Photo.CreateBlankBitmap(10, 10);

        Assert.Equal(10, bitmap.Width);
        Assert.Equal(10, bitmap.Height);
        Assert.Equal(SKColors.Black, bitmap.GetPixel(0, 0));
    }

    [Fact]
    public void LinearResizeImage_ChangesDimensions()
    {
        using var original = new SKBitmap(100, 50);
        using var resized = Photo.LinearResizeImage(original, 50, 25);

        Assert.Equal(50, resized.Width);
        Assert.Equal(25, resized.Height);
    }

    [Fact]
    public void DuplicateBitmap_CreatesIndependentCopy()
    {
        using var original = new SKBitmap(10, 10);
        using var copy = Photo.DuplicateBitmap(original);

        Assert.Equal(original.Width, copy.Width);
        Assert.Equal(original.Height, copy.Height);
    }
}
