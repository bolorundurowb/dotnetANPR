using System;
using System.IO;
using DotNetANPR.Configuration;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Base class for all image types in the ANPR system. Wraps an <see cref="SKBitmap"/> and provides
/// pixel-level brightness, saturation, and hue operations, as well as filtering and resizing utilities.
/// </summary>
/// <param name="image">The underlying bitmap image.</param>
public class Photo(SKBitmap image) : IDisposable, ICloneable
{
    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    public int Width => image.Width;

    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    public int Height => image.Height;

    /// <summary>
    /// Gets the perceived brightness of the center pixel (0.0 to 1.0).
    /// </summary>
    public float Brightness => GetBrightness(image, Width / 2, Height / 2);

    /// <summary>
    /// Gets the saturation of the center pixel (0.0 to 1.0).
    /// </summary>
    public float Saturation => GetSaturation(image, Width / 2, Height / 2);

    /// <summary>
    /// Gets the hue of the center pixel (0.0 to 1.0, where 1.0 = 360 degrees).
    /// </summary>
    public float Hue => GetHue(image, Width / 2, Height / 2);

    /// <summary>
    /// Gets or sets the underlying <see cref="SKBitmap"/> image.
    /// </summary>
    public SKBitmap Image
    {
        get => image;
        internal set => image = value;
    }

    /// <summary>
    /// Sets a pixel to a grayscale value corresponding to the given brightness.
    /// </summary>
    /// <param name="bitmap">The target bitmap.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <param name="value">The brightness value (0.0 to 1.0).</param>
    public static void SetBrightness(SKBitmap bitmap, int x, int y, float value)
    {
        var brightness = (byte)Math.Min(255, Math.Max(0, (int)(value * 255)));
        bitmap.SetPixel(x, y, new SKColor(brightness, brightness, brightness));
    }

    /// <summary>
    /// Gets the perceived brightness of a pixel using the luminance formula.
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The brightness value (0.0 to 1.0).</returns>
    public static float GetBrightness(SKBitmap bitmap, int x, int y)
    {
        var color = bitmap.GetPixel(x, y);
        return (0.299f * color.Red + 0.587f * color.Green + 0.114f * color.Blue) / 255f;
    }

    /// <summary>
    /// Gets the saturation of a pixel (0.0 to 1.0) using the HSL color model.
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The saturation value (0.0 to 1.0).</returns>
    public static float GetSaturation(SKBitmap bitmap, int x, int y)
    {
        var color = bitmap.GetPixel(x, y);
        var r = color.Red / 255f;
        var g = color.Green / 255f;
        var b = color.Blue / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        if (max == min)
            return 0f;

        var l = (max + min) / 2f;
        return l <= 0.5f
            ? (max - min) / (max + min)
            : (max - min) / (2f - max - min);
    }

    /// <summary>
    /// Gets the hue of a pixel as a value in the range 0.0 to 1.0 (mapping 0-360 degrees).
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The hue value (0.0 to 1.0).</returns>
    public static float GetHue(SKBitmap bitmap, int x, int y)
    {
        var color = bitmap.GetPixel(x, y);
        var r = color.Red / 255f;
        var g = color.Green / 255f;
        var b = color.Blue / 255f;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));

        if (max == min)
            return 0f;

        var delta = max - min;
        float hue;

        if (max == r)
            hue = (g - b) / delta + (g < b ? 6f : 0f);
        else if (max == g)
            hue = (b - r) / delta + 2f;
        else
            hue = (r - g) / delta + 4f;

        hue /= 6f;
        return hue;
    }

    /// <summary>
    /// Resizes an image using linear (bilinear) interpolation.
    /// </summary>
    /// <param name="origin">The original bitmap to resize.</param>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    /// <returns>A new resized bitmap.</returns>
    public static SKBitmap LinearResizeImage(SKBitmap origin, int width, int height)
    {
        var info = new SKImageInfo(width, height, origin.ColorType, origin.AlphaType);
        var resized = origin.Resize(info, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        return resized ?? origin.Copy(origin.ColorType);
    }

    /// <summary>
    /// Creates an independent copy of the given bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap to duplicate.</param>
    /// <returns>A new bitmap that is a pixel-exact copy of the original.</returns>
    public static SKBitmap DuplicateBitmap(SKBitmap bitmap)
    {
        return bitmap.Copy(bitmap.ColorType);
    }

    /// <summary>
    /// Applies a simple thresholding operation in-place: pixel channel values below 36 are set to 0.
    /// Uses unsafe pointer-based pixel access for performance.
    /// </summary>
    /// <param name="bitmap">The bitmap to threshold.</param>
    public static unsafe void Thresholding(SKBitmap bitmap)
    {
        var threshold = new byte[256];
        for (var i = 0; i < 36; i++)
            threshold[i] = 0;
        for (var i = 36; i < 256; i++)
            threshold[i] = (byte)i;

        var ptr = bitmap.GetPixels();
        var totalBytes = bitmap.Info.BytesSize;
        var pixelData = (byte*)ptr.ToPointer();
        var bytesPerPixel = bitmap.BytesPerPixel;

        for (var i = 0; i < totalBytes; i++)
        {
            // Skip alpha channel (every 4th byte in BGRA/RGBA)
            if (bytesPerPixel == 4 && (i % 4) == 3)
                continue;
            pixelData[i] = threshold[pixelData[i]];
        }
    }

    /// <summary>
    /// Creates a grayscale bitmap from a 2D brightness array.
    /// </summary>
    /// <param name="array">The brightness values (0.0 to 1.0), indexed as [x, y].</param>
    /// <param name="w">The width of the output bitmap.</param>
    /// <param name="h">The height of the output bitmap.</param>
    /// <returns>A new grayscale bitmap.</returns>
    public static SKBitmap ArrayToBitmap(float[,] array, int w, int h)
    {
        var bitmap = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Opaque);
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            SetBrightness(bitmap, x, y, array[x, y]);
        return bitmap;
    }

    /// <summary>
    /// Creates a blank (black) bitmap with the same dimensions as the given image.
    /// </summary>
    /// <param name="source">The source bitmap whose dimensions to copy.</param>
    /// <returns>A new blank bitmap.</returns>
    public static SKBitmap CreateBlankBitmap(SKBitmap source) => CreateBlankBitmap(source.Width, source.Height);

    /// <summary>
    /// Creates a blank (black) bitmap with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the bitmap.</param>
    /// <param name="height">The height of the bitmap.</param>
    /// <returns>A new blank bitmap.</returns>
    public static SKBitmap CreateBlankBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Black);
        }

        return bitmap;
    }

    /// <summary>
    /// Returns a new bitmap with the image drawn on a light gray background with labeled axes.
    /// </summary>
    /// <returns>A new bitmap with axis markings.</returns>
    public SKBitmap GetBitmapWithAxes()
    {
        var widthWithAxes = image.Width + 40;
        var heightWithAxes = image.Height + 40;

        var axis = new SKBitmap(widthWithAxes, heightWithAxes, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(axis);
        canvas.Clear(SKColors.LightGray);

        // Draw the image
        canvas.DrawBitmap(image, 35, 5);

        // Draw the black border around the image
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        canvas.DrawRect(35, 5, image.Width, image.Height, borderPaint);

        // Set up text and tick drawing
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };
        using var font = new SKFont(SKTypeface.Default, 12);
        using var linePaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        // Draw X axis labels and ticks
        for (var ax = 0; ax < image.Width; ax += 50)
        {
            canvas.DrawText(ax.ToString(), ax + 35, axis.Height - 2, font, textPaint);
            canvas.DrawLine(ax + 35, image.Height + 5, ax + 35, image.Height + 15, linePaint);
        }

        // Draw Y axis labels and ticks
        for (var ay = 0; ay < image.Height; ay += 50)
        {
            canvas.DrawText(ay.ToString(), 3, ay + 15, font, textPaint);
            canvas.DrawLine(25, ay + 5, 35, ay + 5, linePaint);
        }

        return axis;
    }

    /// <summary>
    /// Sets a pixel to a grayscale value (0-255) on this photo's image.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <param name="value">The grayscale value (0 to 255).</param>
    public void SetBrightness(int x, int y, int value)
    {
        var clamped = (byte)Math.Min(255, Math.Max(0, value));
        var color = new SKColor(clamped, clamped, clamped);
        image.SetPixel(x, y, color);
    }

    /// <summary>
    /// Gets the perceived brightness of a pixel in this photo's image.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The brightness value (0.0 to 1.0).</returns>
    public float GetBrightness(int x, int y) => GetBrightness(image, x, y);

    /// <summary>
    /// Gets the saturation of a pixel in this photo's image.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The saturation value (0.0 to 1.0).</returns>
    public float GetSaturation(int x, int y) => GetSaturation(image, x, y);

    /// <summary>
    /// Gets the hue of a pixel in this photo's image.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The hue value (0.0 to 1.0).</returns>
    public float GetHue(int x, int y) => GetHue(image, x, y);

    /// <summary>
    /// Saves the image to a file in PNG format.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
    public void Save(string path)
    {
        using var img = SKImage.FromBitmap(image);
        using var data = img.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(path);
        data.SaveTo(stream);
    }

    /// <summary>
    /// Normalizes the brightness of all pixels using statistical thresholding.
    /// </summary>
    /// <param name="coef">The normalization coefficient.</param>
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

    /// <summary>
    /// Creates a deep copy of this photo.
    /// </summary>
    /// <returns>A new <see cref="Photo"/> with a duplicated bitmap.</returns>
    public Photo Duplicate() => new(DuplicateBitmap(image));

    /// <summary>
    /// Creates a clone of this photo (implements <see cref="ICloneable"/>).
    /// </summary>
    /// <returns>A boxed <see cref="Photo"/> clone.</returns>
    public object Clone() => Duplicate();

    #region Filters

    /// <summary>
    /// Resizes this photo's image in-place using linear interpolation.
    /// </summary>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    public void LinearResize(int width, int height) { image = LinearResizeImage(image, width, height); }

    /// <summary>
    /// Resizes this photo's image in-place using weighted average downscaling.
    /// Falls back to linear resize when the target is larger than the source.
    /// </summary>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    public void AverageResize(int width, int height) { image = AverageResizeImage(image, width, height); }

    /// <summary>
    /// Resizes a bitmap using weighted average downscaling. If the target dimensions are
    /// larger than the source, falls back to linear resize.
    /// </summary>
    /// <param name="origin">The original bitmap.</param>
    /// <param name="width">The target width.</param>
    /// <param name="height">The target height.</param>
    /// <returns>A new resized bitmap.</returns>
    public SKBitmap AverageResizeImage(SKBitmap origin, int width, int height)
    {
        // Average resize doesn't work well for zooming in; fall back to linear
        if (origin.Width < width || origin.Height < height)
            return LinearResizeImage(origin, width, height);

        var resized = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
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

                // Compute neighborhood average brightness
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

    /// <summary>
    /// Converts a bitmap to a 2D brightness array indexed as [x, y].
    /// </summary>
    /// <param name="source">The source bitmap.</param>
    /// <param name="width">The width to scan.</param>
    /// <param name="height">The height to scan.</param>
    /// <returns>A 2D float array of brightness values.</returns>
    public float[,] BitmapToArray(SKBitmap source, int width, int height)
    {
        var array = new float[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            array[x, y] = GetBrightness(source, x, y);

        return array;
    }

    /// <summary>
    /// Converts a bitmap to a 2D brightness array with a 1-pixel border of white (1.0) values.
    /// The result is indexed as [x+1, y+1] for original pixel data.
    /// </summary>
    /// <param name="source">The source bitmap.</param>
    /// <param name="width">The width to scan.</param>
    /// <param name="height">The height to scan.</param>
    /// <returns>A 2D float array of size (width+2, height+2) with border values set to 1.0.</returns>
    public float[,] BitmapToArrayWithBounds(SKBitmap source, int width, int height)
    {
        var array = new float[width + 2, height + 2];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            array[x + 1, y + 1] = GetBrightness(source, x, y);

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

    /// <summary>
    /// Sums two bitmaps pixel-by-pixel, clamping brightness to 1.0.
    /// </summary>
    /// <param name="image1">The first bitmap.</param>
    /// <param name="image2">The second bitmap.</param>
    /// <returns>A new bitmap where each pixel is the clamped sum of the two inputs.</returns>
    public SKBitmap SumBitmaps(SKBitmap image1, SKBitmap image2)
    {
        var outWidth = Math.Min(image1.Width, image2.Width);
        var outHeight = Math.Min(image1.Height, image2.Height);

        var outImage = new SKBitmap(outWidth, outHeight, SKColorType.Bgra8888, SKAlphaType.Opaque);
        for (var x = 0; x < outWidth; x++)
        for (var y = 0; y < outHeight; y++)
        {
            var brightness = Math.Min(1.0f, GetBrightness(image1, x, y) + GetBrightness(image2, x, y));
            SetBrightness(outImage, x, y, brightness);
        }

        return outImage;
    }

    /// <summary>
    /// Applies plain (global) thresholding to the image using brightness statistics.
    /// </summary>
    /// <param name="stat">The statistics object used for threshold computation.</param>
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

    /// <summary>
    /// Gets the ARGB integer representation of a pixel color.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <returns>The pixel color as a 32-bit ARGB integer.</returns>
    public int GetPixelColor(int x, int y)
    {
        var color = image.GetPixel(x, y);
        return (color.Alpha << 24) | (color.Red << 16) | (color.Green << 8) | color.Blue;
    }

    /// <summary>
    /// Applies adaptive thresholding to the image. If the configured radius is 0,
    /// falls back to plain thresholding. Otherwise, each pixel is compared against
    /// the average brightness of its neighborhood.
    /// </summary>
    public void AdaptiveThresholding()
    {
        var statistics = new Statistics(this);
        var radius = AnprConfig.Instance.Photo.AdaptiveThresholdingRadius;
        if (radius == 0)
        {
            PlainThresholding(statistics);
            return;
        }

        var width = image.Width;
        var height = image.Height;
        var sourceArray = BitmapToArray(image, width, height);
        var destinationArray = BitmapToArray(image, width, height);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            // Compute neighborhood average
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

        image = ArrayToBitmap(destinationArray, width, height);
    }

    /// <summary>
    /// Computes the Hough transformation for this image.
    /// </summary>
    /// <returns>A new <see cref="HoughTransformation"/> computed from the image brightness.</returns>
    public HoughTransformation GetHoughTransformation()
    {
        var hough = new HoughTransformation(Width, Height);
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            hough.AddLine(x, y, GetBrightness(x, y));

        return hough;
    }

    /// <summary>
    /// Disposes the underlying bitmap, releasing unmanaged resources.
    /// </summary>
    public void Dispose() { image.Dispose(); }

    #region Overrides

    /// <summary>
    /// Determines whether the specified object is equal to the current photo by comparing all pixel colors.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><c>true</c> if all pixels match; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns a hash code computed from the sum of all pixel color values.
    /// </summary>
    /// <returns>A hash code for the photo.</returns>
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
