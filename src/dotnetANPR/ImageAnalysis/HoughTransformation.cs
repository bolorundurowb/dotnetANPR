﻿using System;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

public class HoughTransformation
{
    public enum RenderType
    {
        RenderAll = 1,
        RenderTransformationOnly = 0
    }

    public enum ColorType
    {
        BlackAndWhite = 0,
        Hue = 1
    }

    private readonly float[,] _bitmap;
    private Point? _maxPoint;
    private readonly int _width;
    private readonly int _height;
    private float _angle;
    private float _dx;
    private float _dy;

    public float Dx => _dx;

    public float Dy => _dy;

    public float Angle => _angle;

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

    public Point GetMaxPoint()
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

    public SKBitmap Render(RenderType renderType, ColorType colorType)
    {
        var average = GetAverageValue();
        var output = new SKBitmap(_width, _height);

        var canvas = new SKCanvas(output);

        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
        {
            var value = (byte)Math.Max(0, Math.Min(255 * _bitmap[x, y] / average / 3, 255));
            var color = colorType == ColorType.BlackAndWhite
                ? new SKColor(value, value, value)
                : new SKColor((byte)(255 - (value * 2 / 3)), 255, 255);

            canvas.DrawPoint(x, y, new SKPaint { Color = color });
        }

        var maximumPoint = FindMaxPoint();
        canvas.DrawBitmap(output, new SKRect(0, 0, _width, _height));

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

        var paint = new SKPaint
        {
            Color = SKColors.Orange,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        canvas.DrawCircle(maximumPoint.X - 5, maximumPoint.Y - 5, 5, paint);

        if (renderType == RenderType.RenderAll)
        {
            canvas.DrawLine(0, _height / 2f - _dy / 2 - 1, _width, _height / 2f + _dy / 2 - 1, paint);
            canvas.DrawLine(0, _height / 2f - _dy / 2, _width, _height / 2f + _dy / 2, paint);
            canvas.DrawLine(0, _height / 2f - _dy / 2 + 1, _width, _height / 2f + _dy / 2 + 1, paint);
        }

        canvas.Dispose();
        return output;
    }


    private Point FindMaxPoint()
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

        return new Point(maxX, maxY);
    }
}
