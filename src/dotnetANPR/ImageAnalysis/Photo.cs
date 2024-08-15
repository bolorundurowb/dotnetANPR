using System;
using System.Drawing;
using DotNetANPR.Configuration;
using ImageMagick;

namespace DotNetANPR.ImageAnalysis;

public class Photo(MagickImage image) : IDisposable, ICloneable
{
    public int Width => image.Width;

    public int Height => image.Height;

    public float Brightness => GetBrightness(image, Width / 2, Height / 2);

    public float Saturation => GetSaturation(image, Width / 2, Height / 2);

    public float Hue => GetHue(image, Width / 2, Height / 2);

    public MagickImage Image
    {
        get => image;
        // TODO: fix this travesty
        // protected set => image = value;
        internal set => image = value;
    }

    public static void SetBrightness(MagickImage image, int x, int y, float value)
    {
        var brightness = (int)(value * 255);
        image.SetPixel(x, y, Color.FromArgb(brightness, brightness, brightness));
    }

    public static float GetBrightness(MagickImage image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        return Color.FromArgb(color.R, color.G, color.B).GetBrightness();
    }

    public static float GetSaturation(MagickImage image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        return Color.FromArgb(color.R, color.G, color.B).GetSaturation();
    }

    public static float GetHue(MagickImage image, int x, int y)
    {
        var color = image.GetPixel(x, y);
        return Color.FromArgb(color.R, color.G, color.B).GetHue() / 360f;
    }

    public static MagickImage LinearResizeImage(MagickImage origin, int width, int height)
    {
        var resizedImage = new MagickImage(width, height);
        using var graphics = Graphics.FromImage(resizedImage);
        var xScale = (float)width / origin.Width;
        var yScale = (float)height / origin.Height;

        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.ScaleTransform(xScale, yScale);
        graphics.DrawImage(origin, new Rectangle(0, 0, origin.Width, origin.Height));
        return resizedImage;
    }

    public static MagickImage DuplicateMagickImage(MagickImage image)
    {
        var imageCopy = new MagickImage(image.Width, image.Height, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(imageCopy);
        graphics.DrawImage(image, 0, 0, image.Width, image.Height);

        return imageCopy;
    }

    public static void Thresholding(MagickImage bitmap)
    {
        // Define the threshold array
        var threshold = new byte[256];
        for (var i = 0; i < 36; i++)
            threshold[i] = 0;
        for (var i = 36; i < 256; i++) 
            threshold[i] = (byte)i;

        // Lock the bitmap's bits
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

        // Get the address of the first line
        var ptr = bmpData.Scan0;

        // Declare an array to hold the bytes of the bitmap
        var bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
        var rgbValues = new byte[bytes];

        // Copy the RGB values into the array
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

        // Apply the thresholding
        for (var i = 0; i < rgbValues.Length; i++) 
            rgbValues[i] = threshold[rgbValues[i]];

        // Copy the RGB values back to the bitmap
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

        // Unlock the bits
        bitmap.UnlockBits(bmpData);
    }

    public static MagickImage ArrayToMagickImage(float[,] array, int w, int h)
    {
        var bitmap = new MagickImage(w, h, PixelFormat.Format24bppRgb);
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            SetBrightness(bitmap, x, y, array[x, y]);
        return bitmap;
    }

    public static MagickImage CreateBlankMagickImage(MagickImage image) => CreateBlankMagickImage(image.Width, image.Height);

    public static MagickImage CreateBlankMagickImage(int width, int height) => new(width, height, PixelFormat.Format24bppRgb);

    public MagickImage GetMagickImageWithAxes()
    {
        var widthWithAxes = image.Width + 40;
        var heightWithAxes = image.Height + 40;

        var axis = new MagickImage(widthWithAxes, heightWithAxes, PixelFormat.Format24bppRgb);
        using var graphicAxis = Graphics.FromImage(axis);
        // Set the background color to light gray
        graphicAxis.Clear(Color.LightGray);

        // Draw the image
        graphicAxis.DrawImage(image, 35, 5, image.Width, image.Height);

        // Draw the black border around the image
        graphicAxis.DrawRectangle(Pens.Black, 35, 5, image.Width, image.Height);

        // Draw the X axis labels and ticks
        for (var ax = 0; ax < image.Width; ax += 50)
        {
            graphicAxis.DrawString(ax.ToString(), SystemFonts.DefaultFont, Brushes.Black, ax + 35,
                axis.Height - 10);
            graphicAxis.DrawLine(Pens.Black, ax + 35, image.Height + 5, ax + 35, image.Height + 15);
        }

        // Draw the Y axis labels and ticks
        for (var ay = 0; ay < image.Height; ay += 50)
        {
            graphicAxis.DrawString(ay.ToString(), SystemFonts.DefaultFont, Brushes.Black, 3, ay + 15);
            graphicAxis.DrawLine(Pens.Black, 25, ay + 5, 35, ay + 5);
        }

        return axis;
    }

    public void SetBrightness(int x, int y, int value)
    {
        var color = Color.FromArgb(value, value, value);
        image.SetPixel(x, y, color);
    }

    public float GetBrightness(int x, int y) => GetBrightness(image, x, y);

    public float GetSaturation(int x, int y) => GetSaturation(image, x, y);

    public float GetHue(int x, int y) => GetHue(image, x, y);

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

    public Photo Duplicate() => new(DuplicateMagickImage(image));

    public object Clone() => Duplicate();

    #region Filters

    public void LinearResize(int width, int height) { image = LinearResizeImage(image, width, height); }

    public void AverageResize(int width, int height) { image = AverageResizeImage(image, width, height); }

    public MagickImage AverageResizeImage(MagickImage origin, int width, int height)
    {
        // TODO: Doesn't work well for characters of size similar to the target size
        if (origin.Width < width || origin.Height < height)
            // Average height doesn't play well with zooming in; if we are zooming in in direction x or y,
            // use linear transformation
            return LinearResizeImage(origin, width, height);

        // Java traditionally makes images smaller with the bilinear method (linear mapping), which brings large
        // information loss. Fourier transformation would be ideal, but it is too slow.
        // Therefore we use the method of weighted average.
        var resized = new MagickImage(width, height, PixelFormat.Format24bppRgb);
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

    public float[,] MagickImageToArray(MagickImage image, int width, int height)
    {
        var array = new float[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            array[x, y] = GetBrightness(image, x, y);

        return array;
    }

    public float[,] MagickImageToArrayWithBounds(MagickImage image, int width, int height)
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

    public MagickImage SumMagickImages(MagickImage image1, MagickImage image2)
    {
        var outWidth = Math.Min(image1.Width, image2.Width);
        var outHeight = Math.Min(image1.Height, image2.Height);

        var outImage = new MagickImage(outWidth, outHeight, PixelFormat.Format24bppRgb);
        for (var x = 0; x < outWidth; x++)
        for (var y = 0; y < outHeight; y++)
        {
            var brightness = Math.Min(1.0f, GetBrightness(image1, x, y) + GetBrightness(image2, x, y));
            SetBrightness(outImage, x, y, brightness);
        }

        return outImage;
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

    public int GetPixelColor(int x, int y) => image.GetPixel(x, y).ToArgb();

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
        var sourceArray = MagickImageToArray(image, width, height);
        var destinationArray = MagickImageToArray(image, width, height);

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

        image = ArrayToMagickImage(destinationArray, width, height);
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
