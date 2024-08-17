using System;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

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
        // TODO: fix this travesty
        // protected set => image = value;
        internal set => image = value;
    }

    public static void SetBrightness(SKBitmap image, int x, int y, float value)
    {
        var brightness = (byte)(value * 255);
        image.SetPixel(x, y, new SKColor(brightness, brightness, brightness));
    }

    public static float GetBrightness(SKBitmap image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        new SKColor(color.Red, color.Green, color.Blue).ToHsv(out _, out _, out var brightness);
        return brightness;
    }

    public static float GetSaturation(SKBitmap image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        new SKColor(color.Red, color.Green, color.Blue).ToHsv(out _, out var saturation, out _);
        return saturation;
    }

    public static float GetHue(SKBitmap image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        new SKColor(color.Red, color.Green, color.Blue).ToHsv(out var hue, out _, out _);
        return hue;
    }

    public static SKBitmap LinearResizeImage(SKBitmap origin, int width, int height)
    {
        var resizedImage = new SKBitmap(width, height);
        using var canvas = new SKCanvas(resizedImage);
        var xScale = (float)width / origin.Width;
        var yScale = (float)height / origin.Height;
        canvas.Scale(xScale, yScale);
        canvas.DrawBitmap(origin, 0, 0);

        return resizedImage;
    }


    public static SKBitmap DuplicateSKBitmap(SKBitmap image)
    {
        var imageCopy = new SKBitmap(image.Width, image.Height, SKColorType.Rgb888x, SKAlphaType.Premul);
        using var canvas = new SKCanvas(imageCopy);
        canvas.DrawBitmap(image, new SKRect(0, 0, image.Width, image.Height));

        return imageCopy;
    }


    public static void Thresholding(SKBitmap bitmap)
    {
        // Define the threshold array
        var threshold = new byte[256];
        for (var i = 0; i < 36; i++)
            threshold[i] = 0;
        for (var i = 36; i < 256; i++)
            threshold[i] = (byte)i;

        for (var x = 0; x < bitmap.Width; x++)
        {
            for (var y = 0; y < bitmap.Height; y++)
            {
                var color = bitmap.GetPixel(x, y);

                // Apply threshold to each color component
                var r = threshold[color.Red];
                var g = threshold[color.Green];
                var b = threshold[color.Blue];

                var newColor = new SKColor(r, g, b);
                bitmap.SetPixel(x, y, newColor);
            }
        }
    }


    public static SKBitmap ArrayToSKBitmap(float[,] array, int w, int h)
    {
        var bitmap = new SKBitmap(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var color = new SKColor((byte)(array[x, y] * 255), (byte)(array[x, y] * 255),
                    (byte)(array[x, y] * 255));
                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
    }

    public SKBitmap GetSKBitmapWithAxes()
    {
        var widthWithAxes = image.Width + 40;
        var heightWithAxes = image.Height + 40;


        var axis = new SKBitmap(widthWithAxes, heightWithAxes, SKColorType.Rgb888x, SKAlphaType.Premul);
        using var canvas = new SKCanvas(axis);

        // Set the background color to light gray
        canvas.Clear(SKColors.LightGray);

        // Draw the image
        canvas.DrawBitmap(image, 35, 5);

        // Draw the black border around the image
        canvas.DrawRect(SKRect.Create(35, 5, image.Width, image.Height),
            new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke });

        // Draw the X axis labels and ticks
        for (var ax = 0; ax < image.Width; ax += 50)
        {
            canvas.DrawText(ax.ToString(), ax + 35, heightWithAxes - 10,
                new SKPaint { Color = SKColors.Black, TextSize = 12 });
            canvas.DrawLine(ax + 35, image.Height + 5, ax + 35, image.Height + 15,
                new SKPaint { Color = SKColors.Black, StrokeWidth = 1 });
        }

        // Draw the Y axis labels and ticks
        for (var ay = 0; ay < image.Height; ay += 50)
        {
            canvas.DrawText(ay.ToString(), 3, ay + 15, new SKPaint { Color = SKColors.Black, TextSize = 12 });
            canvas.DrawLine(25, ay + 5, 35, ay + 5, new SKPaint { Color = SKColors.Black, StrokeWidth = 1 });
        }

        return axis;
    }

    public float GetBrightness(int x, int y) => GetBrightness(image, x, y);

    public void Save(string path) { image.Save(path); }

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

    public Photo Duplicate() => new(DuplicateSKBitmap(image));

    public object Clone() => Duplicate();

    #region Filters

    public void LinearResize(int width, int height) { image = LinearResizeImage(image, width, height); }

    public void AverageResize(int width, int height) { image = AverageResizeImage(image, width, height); }

    public SKBitmap AverageResizeImage(SKBitmap origin, int width, int height)
    {
        // TODO: Doesn't work well for characters of size similar to the target size
        if (origin.Width < width || origin.Height < height)
            // Average height doesn't play well with zooming in; if we are zooming in in direction x or y,
            // use linear transformation
            return LinearResizeImage(origin, width, height);

        var resized = new SKBitmap(width, height, origin.ColorType, origin.AlphaType, origin.ColorSpace);

        using var canvas = new SKCanvas(resized);
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
                var sumAsByte = (byte)sum;
                var color = new SKColor(sumAsByte, sumAsByte, sumAsByte);
                canvas.DrawPoint(x, y, color);
            }
        }

        return resized;
    }

    #endregion

    public float[,] SKBitmapToArray(SKBitmap image, int width, int height)
    {
        var array = new float[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            array[x, y] = GetBrightness(image, x, y);

        return array;
    }

    public float[,] SKBitmapToArrayWithBounds(SKBitmap image, int width, int height)
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

    public int GetPixelColor(int x, int y) => (int)(uint)image.GetPixel(x, y);

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
        var sourceArray = SKBitmapToArray(image, width, height);
        var destinationArray = SKBitmapToArray(image, width, height);

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
            destinationArray[x, y] = destinationArray[x, y] < neighborhood ? 0f : 1f;
        }

        image = ArrayToSKBitmap(destinationArray, width, height);
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
