using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

public class Graph
{
    private List<Peak>? _peaks = null;
    protected List<float> YValues = [];

    private bool _actualAverageValue; // are values up-to-date?
    private bool _actualMaximumValue; // are values up-to-date?
    private bool _actualMinimumValue; // are values up-to-date?
    private float _averageValue;
    private float _maximumValue;
    private float _minimumValue;

    public List<Peak> Peaks
    {
        get => _peaks ?? [];
        protected set => _peaks = value;
    }

    public void DeActualizeFlags()
    {
        _actualAverageValue = false;
        _actualMaximumValue = false;
        _actualMinimumValue = false;
    }

    // TODO: change name
    public bool AllowedInterval(List<Peak> peaks, int xPosition) =>
        peaks.All(peak => peak.Left > xPosition || xPosition > peak.Right);

    public void ApplyProbabilityDistributor(ProbabilityDistributor probabilityDistributor)
    {
        YValues = probabilityDistributor.Distribute(YValues);
        DeActualizeFlags();
    }

    public void Negate()
    {
        var max = MaxValue();

        for (var i = 0; i < YValues.Count; i++)
            YValues[i] = max - YValues[i];

        DeActualizeFlags();
    }

    public float AverageValue()
    {
        if (!_actualAverageValue)
        {
            _averageValue = AverageValue(0, YValues.Count);
            _actualAverageValue = true;
        }

        return _averageValue;
    }

    public float AverageValue(int a, int b)
    {
        var sum = 0f;

        for (var i = a; i < b; i++)
            sum += YValues[i];

        return sum / YValues.Count;
    }

    public float MaxValue()
    {
        if (!_actualMaximumValue)
        {
            _maximumValue = MaxValue(0, YValues.Count);
            _actualMaximumValue = true;
        }

        return _maximumValue;
    }

    public float MaxValue(int a, int b)
    {
        var maxValue = 0.0f;

        for (var i = a; i < b; i++)
            maxValue = Math.Max(maxValue, YValues[i]);

        return maxValue;
    }

    public float MaxValue(float a, float b)
    {
        var ia = (int)(a * YValues.Count);
        var ib = (int)(b * YValues.Count);

        return MaxValue(ia, ib);
    }

    public int MaxValueIndex(int a, int b)
    {
        var maxValue = 0f;
        var maxIndex = a;

        for (var i = a; i < b; i++)
        {
            if (YValues[i] >= maxValue)
            {
                maxValue = YValues[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public float MinValue()
    {
        if (!_actualMinimumValue)
        {
            _minimumValue = MinValue(0, YValues.Count);
            _actualMinimumValue = true;
        }

        return _minimumValue;
    }

    public float MinValue(int a, int b)
    {
        var minValue = float.PositiveInfinity;

        for (var i = a; i < b; i++)
            minValue = Math.Min(minValue, YValues[i]);

        return minValue;
    }

    public float MinValue(float a, float b)
    {
        var ia = (int)(a * YValues.Count);
        var ib = (int)(b * YValues.Count);

        return MinValue(ia, ib);
    }

    public int MinValueIndex(int a, int b)
    {
        var minValue = float.PositiveInfinity;
        var maxIndex = a;

        for (var i = a; i < b; i++)
        {
            if (YValues[i] >= minValue)
            {
                minValue = YValues[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public SKBitmap RenderHorizontally(int width, int height)
    {
        // Create the content and axis bitmaps
        using var content = new SKBitmap(width, height);
        using var axis = new SKBitmap(width + 40, height + 40);

        // Create canvas to draw on the content and axis bitmaps
        using var contentCanvas = new SKCanvas(content);
        using var axisCanvas = new SKCanvas(axis);

        // Draw the axis background
        var backRect = new SKRect(0, 0, width + 40, height + 40);
        axisCanvas.Clear(SKColors.LightGray);
        axisCanvas.DrawRect(backRect, new SKPaint { Color = SKColors.LightGray });

        // Draw the content background
        backRect = new SKRect(0, 0, width, height);
        contentCanvas.Clear(SKColors.White);
        contentCanvas.DrawRect(backRect, new SKPaint { Color = SKColors.White });

        // Draw the line graph
        var graphPaint = new SKPaint { Color = SKColors.Green, IsStroke = true, StrokeWidth = 1 };
        int x = 0, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            x = (int)(((float)i / YValues.Count) * width);
            y = (int)((1 - (YValues[i] / MaxValue())) * height);
            contentCanvas.DrawLine(x0, y0, x, y, graphPaint);
        }

        // Draw the peaks if they exist
        if (Peaks != null)
        {
            graphPaint.Color = SKColors.Red;
            var multConst = (double)width / YValues.Count;
            var i = 0;
            foreach (var p in Peaks)
            {
                contentCanvas.DrawLine((float)(p.Left * multConst), 0, (float)(p.Center * multConst), 30, graphPaint);
                contentCanvas.DrawLine((float)(p.Center * multConst), 30, (float)(p.Right * multConst), 0, graphPaint);
                contentCanvas.DrawText($"{i}.", (float)(p.Center * multConst) - 5, 42,
                    new SKPaint { Color = SKColors.Red, TextSize = 12 });
                i++;
            }
        }

        // Draw the content image onto the axis
        axisCanvas.DrawBitmap(content, 35, 5);

        // Draw the axis borders and labels
        var axisPaint = new SKPaint { Color = SKColors.Black, IsStroke = true, StrokeWidth = 1 };
        axisCanvas.DrawRect(new SKRect(35, 5, 35 + content.Width, 5 + content.Height), axisPaint);

        var textPaint = new SKPaint { Color = SKColors.Black, TextSize = 12 };
        for (var ax = 0; ax < content.Width; ax += 50)
        {
            axisCanvas.DrawText(ax.ToString(), ax + 35, axis.Height - 10, textPaint);
            axisCanvas.DrawLine(ax + 35, content.Height + 5, ax + 35, content.Height + 15, axisPaint);
        }

        for (var ay = 0; ay < content.Height; ay += 20)
        {
            axisCanvas.DrawText($"{(int)((1 - ((float)ay / content.Height)) * 100)}%", 1, ay + 15, textPaint);
            axisCanvas.DrawLine(25, ay + 5, 35, ay + 5, axisPaint);
        }

        return axis;
    }

    public SKBitmap RenderVertically(int width, int height)
    {
        using var content = new SKBitmap(width, height);
        using var axis = new SKBitmap(width + 10, height + 40);

        using var contentCanvas = new SKCanvas(content);
        using var axisCanvas = new SKCanvas(axis);

        var backRect = new SKRect(0, 0, width + 40, height + 40);
        axisCanvas.Clear(SKColors.LightGray);
        axisCanvas.DrawRect(backRect, new SKPaint { Color = SKColors.LightGray });

        backRect = new SKRect(0, 0, width, height);
        contentCanvas.Clear(SKColors.White);
        contentCanvas.DrawRect(backRect, new SKPaint { Color = SKColors.White });

        var graphPaint = new SKPaint { Color = SKColors.Green, IsStroke = true, StrokeWidth = 1 };
        int x = width, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            y = (int)(((float)i / YValues.Count) * height);
            x = (int)((YValues[i] / MaxValue()) * width);
            contentCanvas.DrawLine(x0, y0, x, y, graphPaint);
        }

        if (Peaks != null)
        {
            graphPaint.Color = SKColors.Red;
            var multConst = (double)height / YValues.Count;
            var i = 0;
            foreach (var p in Peaks)
            {
                contentCanvas.DrawLine(width, (float)(p.Left * multConst), width - 30, (float)(p.Center * multConst),
                    graphPaint);
                contentCanvas.DrawLine(width - 30, (float)(p.Center * multConst), width, (float)(p.Right * multConst),
                    graphPaint);
                contentCanvas.DrawText($"{i}.", width - 38, (float)(p.Center * multConst) + 5,
                    new SKPaint { Color = SKColors.Red, TextSize = 12 });
                i++;
            }
        }

        axisCanvas.DrawBitmap(content, 5, 5);

        var axisPaint = new SKPaint { Color = SKColors.Black, IsStroke = true, StrokeWidth = 1 };
        axisCanvas.DrawRect(new SKRect(5, 5, 5 + content.Width, 5 + content.Height), axisPaint);

        contentCanvas.Dispose();
        axisCanvas.Dispose();

        return axis;
    }

    public void RankFilter(int size)
    {
        var halfSize = size / 2;
        var clone = new List<float>(YValues);

        for (var i = halfSize; i < YValues.Count - halfSize; i++)
        {
            float sum = 0;
            for (var ii = i - halfSize; ii < i + halfSize; ii++)
                sum += clone[ii];
            YValues[i] = sum / size;
        }
    }

    public int IndexOfLeftPeakRel(int peak, double peakFootConstantRel)
    {
        var index = peak;
        while (index >= 0)
        {
            if (YValues[index] < peakFootConstantRel * YValues[peak])
                break;

            index--;
        }

        return Math.Max(0, index);
    }

    public int IndexOfRightPeakRel(int peak, double peakFootConstantRel)
    {
        var index = peak;
        while (index < YValues.Count)
        {
            if (YValues[index] < peakFootConstantRel * YValues[peak])
                break;

            index++;
        }

        return Math.Min(YValues.Count, index);
    }

    public float AveragePeakDiff(List<Peak> peaks)
    {
        var sum = 0f;

        foreach (var peak in peaks)
            sum += peak.Diff;

        return sum / peaks.Count;
    }

    public float MaximumPeakDiff(List<Peak> peaks, int from, int to)
    {
        float max = 0;

        for (var i = from; i <= to; i++)
            max = Math.Max(max, peaks[i].Diff);

        return max;
    }


    public void AddPeak(float value)
    {
        YValues.Add(value);
        DeActualizeFlags();
    }
}
