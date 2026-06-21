using System;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;

namespace dotnetANPR.ImageAnalysis;

public class Photo(SKBitmap image) : IDisposable, ICloneable
{
    public int Width => image.Width;

    public int Height => image.Height;

    public float Brightness => GetBrightness(image, Width / 2, Height / 2);

    public float Saturation => GetSaturation(image, Width / 2, Height / 2);

    public float Hue => GetHue(image, Width / 2, Height / 2);

    public SKBitmap Image
    {
        get => image;
        internal set => image = value;
    }

    public static void SetBrightness(SKBitmap image, int x, int y, float value)
    {
        SkiaSharpAdapter.SetBrightness(image, x, y, value);
    }

    public static float GetBrightness(SKBitmap image, int x, int y)
    {
        return SkiaSharpAdapter.GetBrightness(image, x, y);
    }

    public static float GetSaturation(SKBitmap image, int x, int y)
    {
        return SkiaSharpAdapter.GetSaturation(image, x, y);
    }

    public static float GetHue(SKBitmap image, int x, int y)
    {
        return SkiaSharpAdapter.GetHue(image, x, y);
    }

    public static SKBitmap LinearResizeImage(SKBitmap origin, int width, int height)
    {
        return origin.LinearResizeImage(width, height);
    }

    public static SKBitmap DuplicateBitmap(SKBitmap image)
    {
        return SkiaSharpAdapter.DuplicateBitmap(image);
    }

    public static void Thresholding(SKBitmap bitmap)
    {
        SkiaBitmapExtensions.Thresholding(bitmap);
    }

    public static SKBitmap ArrayToBitmap(float[,] array, int w, int h)
    {
        return SkiaSharpAdapter.ArrayToBitmap(array, w, h);
    }

    public static SKBitmap CreateBlankBitmap(SKBitmap image) => CreateBlankBitmap(image.Width, image.Height);

    public static SKBitmap CreateBlankBitmap(int width, int height) => SkiaBitmapExtensions.CreateBlankBitmap(width, height);

    public void SetBrightness(int x, int y, float value)
    {
        SkiaSharpAdapter.SetBrightness(image, x, y, value);
    }

    public float GetBrightness(int x, int y) => GetBrightness(image, x, y);

    public float GetSaturation(int x, int y) => GetSaturation(image, x, y);

    public float GetHue(int x, int y) => GetHue(image, x, y);

    public void Save(string path) { SkiaSharpAdapter.Save(image, path); }

    public void NormalizeBrightness(float coef)
    {
        var stats = new Statistics(this);
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            var currentBrightness = GetBrightness(image, x, y);
            var thresholdBrightness = stats.ThresholdBrightness(currentBrightness, coef);
            SetBrightness(image, x, y, thresholdBrightness);
        }
    }

    public Photo Duplicate() => new(DuplicateBitmap(image));

    public object Clone() => Duplicate();

    #region Filters

    public void LinearResize(int width, int height)
    {
        var newImage = LinearResizeImage(image, width, height);
        image.Dispose();
        image = newImage;
    }

    public void AverageResize(int width, int height)
    {
        var newImage = AverageResizeImage(image, width, height);
        image.Dispose();
        image = newImage;
    }

    public SKBitmap AverageResizeImage(SKBitmap origin, int width, int height)
    {
        // TODO: Doesn't work well for characters of size similar to the target size
        if (origin.Width < width || origin.Height < height)
            // Average height doesn't play well with zooming in; if we are zooming in in direction x or y,
            // use linear transformation
            return LinearResizeImage(origin, width, height);

        // Java traditionally makes images smaller with the bilinear method (linear mapping), which brings large
        // information loss. Fourier transformation would be ideal, but it is too slow.
        // Therefore we use the method of weighted average.
        var resized = SkiaBitmapExtensions.CreateBlankBitmap(width, height);
        var xScale = (float)origin.Width / width;
        var yScale = (float)origin.Height / height;

        for (var x = 0; x < width; x++)
        {
            var x0Min = (int)(x * xScale);
            var x0Max = (int)((x + 1) * xScale);
            for (var y = 0; y < height; y++)
            {
                var y0Min = (int)(y * yScale);
                var y0Max = (int)((y + 1) * yScale);

                // Do a neighborhood average and save to resized image
                float sum = 0;
                var sumCount = 0;
                for (var x0 = x0Min; x0 < x0Max; x0++)
                for (var y0 = y0Min; y0 < y0Max; y0++)
                {
                    sum += GetBrightness(origin, x0, y0);
                    sumCount++;
                }

                sum /= sumCount;
                SetBrightness(resized, x, y, sum);
            }
        }

        return resized;
    }

    #endregion

    public float[,] BitmapToArray(SKBitmap image, int width, int height)
    {
        return SkiaSharpAdapter.BitmapToArray(image, width, height);
    }

    public float[,] BitmapToArrayWithBounds(SKBitmap image, int width, int height)
    {
        var array = new float[width + 2, height + 2];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            array[x + 1, y + 1] = GetBrightness(image, x, y);

        // Clear the edges
        for (var x = 0; x < width + 2; x++)
        {
            array[x, 0] = 1;
            array[x, height + 1] = 1;
        }

        for (var y = 0; y < height + 2; y++)
        {
            array[0, y] = 1;
            array[width + 1, y] = 1;
        }

        return array;
    }

    public SKBitmap SumBitmaps(SKBitmap image1, SKBitmap image2)
    {
        return SkiaBitmapExtensions.SumBitmaps(image1, image2);
    }

    public void PlainThresholding(Statistics stat)
    {
        var width = image.Width;
        var height = image.Height;
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var brightness = GetBrightness(image, x, y);
            SetBrightness(image, x, y, stat.ThresholdBrightness(brightness, 1.0f));
        }
    }

    public int GetPixelColor(int x, int y)
    {
        var color = image.GetPixel(x, y);
        return (int)((uint)color.Alpha << 24 | (uint)color.Red << 16 | (uint)color.Green << 8 | (uint)color.Blue);
    }

    public void AdaptiveThresholding()
    {
        var statistics = new Statistics(this);
        var radius = Configurator.Instance.Get<int>("photo_adaptivethresholdingradius");
        if (radius == 0)
        {
            PlainThresholding(statistics);
            return;
        }

        var width = image.Width;
        var height = image.Height;
        var sourceArray = BitmapToArray(image, width, height);
        var destinationArray = new float[width, height]; // starts all-zero; filled below

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            // Compute neighborhood
            var count = 0;
            var neighborhood = 0.0f;
            for (var ix = x - radius; ix <= x + radius; ix++)
            for (var iy = y - radius; iy <= y + radius; iy++)
                if (ix >= 0 && iy >= 0 && ix < width && iy < height)
                {
                    neighborhood += sourceArray[ix, iy];
                    count++;
                }

            neighborhood /= count;
            destinationArray[x, y] = sourceArray[x, y] < neighborhood ? 0f : 1f;
        }

        image = ArrayToBitmap(destinationArray, width, height);
    }

    public HoughTransformation GetHoughTransformation()
    {
        var hough = new HoughTransformation(Width, Height);
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            hough.AddLine(x, y, GetBrightness(x, y));

        return hough;
    }


    public void Dispose() { image.Dispose(); }

    #region Overrides

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is null || obj is not Photo)
            return false;

        var comparison = (Photo)obj;

        if (comparison.Width != Width || comparison.Height != Height)
            return false;

        for (var i = 0; i < Width; i++)
        for (var j = 0; j < Height; j++)
            if (GetPixelColor(i, j) != comparison.GetPixelColor(i, j))
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        long rgbSum = 0;
        for (var i = 0; i < Width; i++)
        for (var j = 0; j < Height; j++)
            rgbSum += GetPixelColor(i, j);

        return rgbSum.GetHashCode();
    }

    #endregion
}
