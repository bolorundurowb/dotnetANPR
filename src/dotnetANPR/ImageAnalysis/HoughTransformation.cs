using System;
using SkiaSharp;
using dotnetANPR.Extensions;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Implements the Hough transform for detecting lines in an image.
/// Used to determine the skew angle of a licence plate for de-skewing correction.
/// </summary>
internal class HoughTransformation
{
    /// <summary>Controls whether all lines or only the transformation are rendered.</summary>
    internal enum RenderType
    {
        RenderAll = 1,
        RenderTransformationOnly = 0
    }

    /// <summary>Controls the colour mode of rendered output.</summary>
    internal enum ColorType
    {
        BlackAndWhite = 0,
        Hue = 1
    }

    private readonly float[,] _bitmap;
    private (int X, int Y)? _maxPoint;
    private readonly int _width;
    private readonly int _height;
    private float _angle;
    private float _dx;
    private float _dy;

    /// <summary>Gets the horizontal delta of the detected line.</summary>
    public float Dx => _dx;

    /// <summary>Gets the vertical delta of the detected line.</summary>
    public float Dy => _dy;

    /// <summary>Gets the angle of the detected line in degrees.</summary>
    public float Angle => _angle;

    /// <summary>
    /// Creates a Hough transform accumulator of the specified dimensions.
    /// </summary>
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
    /// Adds a pixel at the given coordinates to the accumulator with the specified brightness weight.
    /// </summary>
    public void AddLine(int x, int y, float brightness)
    {
        // Normalize coordinates to -1..1 range
        var xf = 2f * x / _width - 1f;
        var yf = 2f * y / _height - 1f;

        for (var a = 0; a < _width; a++)
        {
            // Normalize a to -1..1 range
            var af = 2f * a / _width - 1f;
            // Calculate corresponding b value
            var bf = yf - af * xf;
            // Normalize b back to 0..height-1 range
            var b = (int)Math.Round((bf + 1f) * _height / 2f);

            if (b > 0 && b < _height - 1) _bitmap[a, b] += brightness;
        }
    }

    /// <summary>
    /// Returns the coordinates of the point with maximum accumulated intensity.
    /// </summary>
    public (int X, int Y) GetMaxPoint()
    {
        if (!_maxPoint.HasValue)
            _maxPoint = FindMaxPoint();

        return _maxPoint.Value;
    }

    private float GetAverageValue()
    {
        float sum = 0;
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                sum += _bitmap[x, y];

        return sum / (_width * _height);
    }

    /// <summary>
    /// Renders the Hough transform accumulator as a visual bitmap with optional line overlays.
    /// After calling this method, <see cref="Dx"/>, <see cref="Dy"/>, and <see cref="Angle"/> are populated.
    /// </summary>
    public SKBitmap Render(RenderType renderType, ColorType colorType)
    {
        var average = GetAverageValue();
        var output = new SKBitmap(_width, _height);

        // First pass: set pixels based on bitmap values
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
            {
                var value = (int)(255 * _bitmap[x, y] / average / 3);
                value = Math.Max(0, Math.Min(value, 255));

                var color = colorType == ColorType.BlackAndWhite
                    ? new SKColor((byte)value, (byte)value, (byte)value)
                    : ColorExtensions.HsbToRgb(0.67f - (float)value / 255 * 2 / 3, 1.0f, 1.0f);

                output.SetPixel(x, y, color);
            }

        // Second pass: draw overlays with canvas
        using var canvas = new SKCanvas(output);
        var maximumPoint = FindMaxPoint();

        // Draw orange circle at max point
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 165, 0), // Orange
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        canvas.DrawCircle(maximumPoint.X, maximumPoint.Y, 5, paint);

        // Calculate line parameters
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

        // Draw lines if requested
        if (renderType == RenderType.RenderAll)
        {
            using var linePaint = new SKPaint
            {
                Color = new SKColor(255, 165, 0), // Orange
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            canvas.DrawLine(0, _height / 2 - _dy / 2 - 1, _width, _height / 2 + _dy / 2 - 1, linePaint);
            canvas.DrawLine(0, _height / 2 - _dy / 2, _width, _height / 2 + _dy / 2, linePaint);
            canvas.DrawLine(0, _height / 2 - _dy / 2 + 1, _width, _height / 2 + _dy / 2 + 1, linePaint);
        }

        return output;
    }

    private (int X, int Y) FindMaxPoint()
    {
        float max = 0;
        int maxX = 0, maxY = 0;
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
            {
                var curr = _bitmap[x, y];

                if (!(curr >= max))
                    continue;

                maxX = x;
                maxY = y;
                max = curr;
            }

        return (maxX, maxY);
    }
}
