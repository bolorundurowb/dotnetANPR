using System;
using DotNetANPR.Extensions;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Implements the Hough line transformation for detecting the dominant skew angle
/// in a license plate image.
/// </summary>
public class HoughTransformation
{
    /// <summary>
    /// Specifies whether to render the full visualization (with line overlay) or only the transform.
    /// </summary>
    public enum RenderType
    {
        /// <summary>
        /// Render the full visualization including detected line overlay.
        /// </summary>
        RenderAll = 1,

        /// <summary>
        /// Render only the Hough transform accumulator.
        /// </summary>
        RenderTransformationOnly = 0
    }

    /// <summary>
    /// Specifies the color mode for rendering the Hough transform.
    /// </summary>
    public enum ColorType
    {
        /// <summary>
        /// Render in black and white (grayscale).
        /// </summary>
        BlackAndWhite = 0,

        /// <summary>
        /// Render using hue-based coloring.
        /// </summary>
        Hue = 1
    }

    private readonly float[,] _bitmap;
    private (int X, int Y)? _maxPoint;
    private readonly int _width;
    private readonly int _height;
    private float _angle;
    private float _dx;
    private float _dy;

    /// <summary>
    /// Gets the horizontal displacement of the detected line.
    /// </summary>
    public float Dx => _dx;

    /// <summary>
    /// Gets the vertical displacement of the detected line.
    /// </summary>
    public float Dy => _dy;

    /// <summary>
    /// Gets the angle of the detected line in degrees.
    /// </summary>
    public float Angle => _angle;

    /// <summary>
    /// Initializes a new instance of the <see cref="HoughTransformation"/> class with the given dimensions.
    /// </summary>
    /// <param name="width">The width of the Hough accumulator.</param>
    /// <param name="height">The height of the Hough accumulator.</param>
    public HoughTransformation(int width, int height)
    {
        _maxPoint = null;
        _bitmap = new float[width, height];
        _width = width;
        _height = height;

        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                _bitmap[x, y] = 0;
    }

    /// <summary>
    /// Adds a line contribution to the Hough accumulator for the given point and brightness.
    /// </summary>
    /// <param name="x">The x-coordinate of the point.</param>
    /// <param name="y">The y-coordinate of the point.</param>
    /// <param name="brightness">The brightness value of the point.</param>
    public void AddLine(int x, int y, float brightness)
    {
        var xf = 2f * x / _width - 1f;
        var yf = 2f * y / _height - 1f;

        for (var a = 0; a < _width; a++)
        {
            var af = 2f * a / _width - 1f;
            var bf = yf - af * xf;
            var b = (int)((bf + 1f) * _height / 2f);

            if (b > 0 && b < _height - 1)
                _bitmap[a, b] += brightness;
        }
    }

    /// <summary>
    /// Gets the point in the accumulator with the maximum value.
    /// </summary>
    /// <returns>A tuple containing the (X, Y) coordinates of the maximum point.</returns>
    public (int X, int Y) GetMaxPoint()
    {
        _maxPoint ??= FindMaxPoint();
        return _maxPoint.Value;
    }

    /// <summary>
    /// Renders the Hough transform accumulator as a bitmap with optional line overlay.
    /// </summary>
    /// <param name="renderType">The render type (full or transform only).</param>
    /// <param name="colorType">The color mode (black-and-white or hue).</param>
    /// <returns>A bitmap visualization of the Hough transform.</returns>
    public SKBitmap Render(RenderType renderType, ColorType colorType)
    {
        var average = GetAverageValue();
        var output = new SKBitmap(_width, _height, SKColorType.Bgra8888, SKAlphaType.Opaque);

        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
            {
                var value = (int)(255 * _bitmap[x, y] / average / 3);
                value = Math.Max(0, Math.Min(value, 255));

                if (colorType == ColorType.BlackAndWhite)
                {
                    output.SetPixel(x, y, new SKColor((byte)value, (byte)value, (byte)value));
                }
                else
                {
                    output.SetPixel(x, y, ColorExtensions.HsbToRgb(0.67f - (float)value / 255 * 2 / 3, 1.0f, 1.0f));
                }
            }

        var maximumPoint = FindMaxPoint();
        _maxPoint = maximumPoint;

        using var canvas = new SKCanvas(output);

        var a = 2 * (float)maximumPoint.X / _width - 1;
        var b = 2 * (float)maximumPoint.Y / _height - 1;
        const float x0F = -1;
        var y0F = a * x0F + b;
        const float x1F = 1;
        var y1F = a * x1F + b;
        var y0 = (int)((y0F + 1) * _height / 2);
        var y1 = (int)((y1F + 1) * _height / 2);
        _dx = _width;
        _dy = y1 - y0;
        _angle = (float)(180 * Math.Atan(_dy / _dx) / Math.PI);

        if (renderType == RenderType.RenderAll)
        {
            using var orangePaint = new SKPaint
            {
                Color = SKColors.Orange,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };
            using var orangeFillPaint = new SKPaint
            {
                Color = SKColors.Orange,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawOval(maximumPoint.X - 5, maximumPoint.Y - 5, 10, 10, orangeFillPaint);
            canvas.DrawLine(0, _height / 2 - (int)_dy / 2 - 1, _width, _height / 2 + (int)_dy / 2 - 1,
                orangePaint);
            canvas.DrawLine(0, _height / 2 - (int)_dy / 2, _width, _height / 2 + (int)_dy / 2, orangePaint);
            canvas.DrawLine(0, _height / 2 - (int)_dy / 2 + 1, _width, _height / 2 + (int)_dy / 2 + 1,
                orangePaint);
        }

        return output;
    }

    #region Private Helpers

    private float GetAverageValue()
    {
        float sum = 0;
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                sum += _bitmap[x, y];

        return sum / (_width * _height);
    }

    private (int X, int Y) FindMaxPoint()
    {
        float max = 0;
        int maxX = 0, maxY = 0;
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
            {
                var curr = _bitmap[x, y];
                if (curr >= max)
                {
                    maxX = x;
                    maxY = y;
                    max = curr;
                }
            }

        return (maxX, maxY);
    }

    #endregion
}
