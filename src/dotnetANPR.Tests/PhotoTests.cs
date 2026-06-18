using DotNetANPR.ImageAnalysis;
using OmniAssert;
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

        brightness.Verify().ToBeApproximately(0f, 0.001f);
    }

    [Fact]
    public void GetBrightness_WhitePixel_ReturnsOne()
    {
        using var bitmap = new SKBitmap(1, 1);
        bitmap.SetPixel(0, 0, SKColors.White);

        var brightness = Photo.GetBrightness(bitmap, 0, 0);

        brightness.Verify().ToBeApproximately(1f, 0.001f);
    }

    [Fact]
    public void CreateBlankBitmap_CreatesBlackBitmap()
    {
        using var bitmap = Photo.CreateBlankBitmap(10, 10);

        bitmap.Width.Verify().ToBe(10);
        bitmap.Height.Verify().ToBe(10);
        bitmap.GetPixel(0, 0).Verify().ToBe(SKColors.Black);
    }

    [Fact]
    public void LinearResizeImage_ChangesDimensions()
    {
        using var original = new SKBitmap(100, 50);
        using var resized = Photo.LinearResizeImage(original, 50, 25);

        resized.Width.Verify().ToBe(50);
        resized.Height.Verify().ToBe(25);
    }

    [Fact]
    public void DuplicateBitmap_CreatesIndependentCopy()
    {
        using var original = new SKBitmap(10, 10);
        using var copy = Photo.DuplicateBitmap(original);

        copy.Width.Verify().ToBe(original.Width);
        copy.Height.Verify().ToBe(original.Height);
    }
}
