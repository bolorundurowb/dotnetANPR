using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Base class for all graph types in the ANPR system. Stores a series of Y-values,
/// detects peaks, and provides statistical and rendering operations.
/// </summary>
public class Graph
{
    private List<Peak>? _peaks = null;

    /// <summary>
    /// The list of Y-axis values that make up the graph data.
    /// </summary>
    protected List<float> YValues = [];

    private bool _actualAverageValue; // are values up-to-date?
    private bool _actualMaximumValue; // are values up-to-date?
    private bool _actualMinimumValue; // are values up-to-date?
    private float _averageValue;
    private float _maximumValue;
    private float _minimumValue;

    /// <summary>
    /// Gets or sets the list of detected peaks in the graph.
    /// Returns an empty list if no peaks have been set.
    /// </summary>
    public List<Peak> Peaks
    {
        get => _peaks ?? [];
        protected set => _peaks = value;
    }

    /// <summary>
    /// Invalidates the cached average, maximum, and minimum values,
    /// forcing them to be recomputed on the next access.
    /// </summary>
    public void DeActualizeFlags()
    {
        _actualAverageValue = false;
        _actualMaximumValue = false;
        _actualMinimumValue = false;
    }

    /// <summary>
    /// Checks whether the given x-position falls outside all peak intervals.
    /// </summary>
    /// <param name="peaks">The list of peaks to check against.</param>
    /// <param name="xPosition">The x-position to test.</param>
    /// <returns><c>true</c> if the position is outside all peak intervals; otherwise, <c>false</c>.</returns>
    public bool AllowedInterval(List<Peak> peaks, int xPosition) =>
        peaks.All(peak => peak.Left > xPosition || xPosition > peak.Right);

    /// <summary>
    /// Applies a probability distribution to the Y-values, modifying them in place.
    /// </summary>
    /// <param name="probabilityDistributor">The distributor to apply.</param>
    public void ApplyProbabilityDistributor(ProbabilityDistributor probabilityDistributor)
    {
        YValues = probabilityDistributor.Distribute(YValues);
        DeActualizeFlags();
    }

    /// <summary>
    /// Negates all Y-values by subtracting each from the maximum value.
    /// </summary>
    public void Negate()
    {
        var max = MaxValue();

        for (var i = 0; i < YValues.Count; i++)
            YValues[i] = max - YValues[i];

        DeActualizeFlags();
    }

    /// <summary>
    /// Computes the average of all Y-values. The result is cached until invalidated.
    /// </summary>
    /// <returns>The average Y-value.</returns>
    public float AverageValue()
    {
        if (!_actualAverageValue)
        {
            _averageValue = AverageValue(0, YValues.Count);
            _actualAverageValue = true;
        }

        return _averageValue;
    }

    /// <summary>
    /// Computes the average Y-value over the range [a, b).
    /// </summary>
    /// <param name="a">The start index (inclusive).</param>
    /// <param name="b">The end index (exclusive).</param>
    /// <returns>The average Y-value in the specified range.</returns>
    public float AverageValue(int a, int b)
    {
        var sum = 0f;

        for (var i = a; i < b; i++)
            sum += YValues[i];

        return sum / YValues.Count;
    }

    /// <summary>
    /// Returns the maximum Y-value across all data. The result is cached until invalidated.
    /// </summary>
    /// <returns>The maximum Y-value.</returns>
    public float MaxValue()
    {
        if (!_actualMaximumValue)
        {
            _maximumValue = MaxValue(0, YValues.Count);
            _actualMaximumValue = true;
        }

        return _maximumValue;
    }

    /// <summary>
    /// Returns the maximum Y-value in the range [a, b).
    /// </summary>
    /// <param name="a">The start index (inclusive).</param>
    /// <param name="b">The end index (exclusive).</param>
    /// <returns>The maximum Y-value in the specified range.</returns>
    public float MaxValue(int a, int b)
    {
        var maxValue = 0.0f;

        for (var i = a; i < b; i++)
            maxValue = Math.Max(maxValue, YValues[i]);

        return maxValue;
    }

    /// <summary>
    /// Returns the maximum Y-value in a fractional range of the data.
    /// </summary>
    /// <param name="a">The start fraction (0.0 to 1.0).</param>
    /// <param name="b">The end fraction (0.0 to 1.0).</param>
    /// <returns>The maximum Y-value in the specified fractional range.</returns>
    public float MaxValue(float a, float b)
    {
        var ia = (int)(a * YValues.Count);
        var ib = (int)(b * YValues.Count);

        return MaxValue(ia, ib);
    }

    /// <summary>
    /// Returns the index of the maximum Y-value in the range [a, b).
    /// </summary>
    /// <param name="a">The start index (inclusive).</param>
    /// <param name="b">The end index (exclusive).</param>
    /// <returns>The index of the maximum Y-value.</returns>
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

    /// <summary>
    /// Returns the minimum Y-value across all data. The result is cached until invalidated.
    /// </summary>
    /// <returns>The minimum Y-value.</returns>
    public float MinValue()
    {
        if (!_actualMinimumValue)
        {
            _minimumValue = MinValue(0, YValues.Count);
            _actualMinimumValue = true;
        }

        return _minimumValue;
    }

    /// <summary>
    /// Returns the minimum Y-value in the range [a, b).
    /// </summary>
    /// <param name="a">The start index (inclusive).</param>
    /// <param name="b">The end index (exclusive).</param>
    /// <returns>The minimum Y-value in the specified range.</returns>
    public float MinValue(int a, int b)
    {
        var minValue = float.PositiveInfinity;

        for (var i = a; i < b; i++)
            minValue = Math.Min(minValue, YValues[i]);

        return minValue;
    }

    /// <summary>
    /// Returns the minimum Y-value in a fractional range of the data.
    /// </summary>
    /// <param name="a">The start fraction (0.0 to 1.0).</param>
    /// <param name="b">The end fraction (0.0 to 1.0).</param>
    /// <returns>The minimum Y-value in the specified fractional range.</returns>
    public float MinValue(float a, float b)
    {
        var ia = (int)(a * YValues.Count);
        var ib = (int)(b * YValues.Count);

        return MinValue(ia, ib);
    }

    /// <summary>
    /// Returns the index of the minimum Y-value in the range [a, b).
    /// </summary>
    /// <param name="a">The start index (inclusive).</param>
    /// <param name="b">The end index (exclusive).</param>
    /// <returns>The index of the minimum Y-value.</returns>
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

    /// <summary>
    /// Renders the graph as a horizontal chart with axes and optional peak markers.
    /// </summary>
    /// <param name="width">The width of the content area in pixels.</param>
    /// <param name="height">The height of the content area in pixels.</param>
    /// <returns>A new <see cref="SKBitmap"/> containing the rendered graph with axes.</returns>
    public SKBitmap RenderHorizontally(int width, int height)
    {
        // Create content bitmap
        var content = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        var axis = new SKBitmap(width + 40, height + 40, SKColorType.Bgra8888, SKAlphaType.Opaque);

        using var contentCanvas = new SKCanvas(content);
        using var axisCanvas = new SKCanvas(axis);

        // Draw backgrounds
        axisCanvas.Clear(SKColors.LightGray);
        contentCanvas.Clear(SKColors.White);

        // Draw line graph on content
        using var greenPaint = new SKPaint
        {
            Color = SKColors.Green,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        int x = 0, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            x = (int)((float)i / YValues.Count * width);
            y = (int)((1 - YValues[i] / MaxValue()) * height);
            contentCanvas.DrawLine(x0, y0, x, y, greenPaint);
        }

        // Draw peaks if they exist
        if (_peaks != null)
        {
            using var redPaint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };
            using var redTextPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            };
            using var peakFont = new SKFont(SKTypeface.Default, 12);

            var multConst = (double)width / YValues.Count;
            var i = 0;
            foreach (var peak in _peaks)
            {
                contentCanvas.DrawLine((int)(peak.Left * multConst), 0, (int)(peak.Center * multConst), 30,
                    redPaint);
                contentCanvas.DrawLine((int)(peak.Center * multConst), 30, (int)(peak.Right * multConst), 0,
                    redPaint);
                contentCanvas.DrawText(i + ".", (int)(peak.Center * multConst) - 5, 42, peakFont, redTextPaint);
                i++;
            }
        }

        // Draw content on axis image
        axisCanvas.DrawBitmap(content, 35, 5);

        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        axisCanvas.DrawRect(35, 5, content.Width, content.Height, borderPaint);

        // Draw axis labels and ticks
        using var labelFont = new SKFont(SKTypeface.Default, 12);
        using var blackTextPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };
        using var linePaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };

        for (var ax = 0; ax < content.Width; ax += 50)
        {
            axisCanvas.DrawText(ax.ToString(), ax + 35, axis.Height - 2, labelFont, blackTextPaint);
            axisCanvas.DrawLine(ax + 35, content.Height + 5, ax + 35, content.Height + 15, linePaint);
        }

        for (var ay = 0; ay < content.Height; ay += 20)
        {
            axisCanvas.DrawText(((1 - (float)ay / content.Height) * 100).ToString("F0") + "%", 1, ay + 15,
                labelFont, blackTextPaint);
            axisCanvas.DrawLine(25, ay + 5, 35, ay + 5, linePaint);
        }

        content.Dispose();
        return axis;
    }

    /// <summary>
    /// Renders the graph as a vertical chart with axes and optional peak markers.
    /// </summary>
    /// <param name="width">The width of the content area in pixels.</param>
    /// <param name="height">The height of the content area in pixels.</param>
    /// <returns>A new <see cref="SKBitmap"/> containing the rendered graph with axes.</returns>
    public SKBitmap RenderVertically(int width, int height)
    {
        // Create content bitmap
        var content = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        var axis = new SKBitmap(width + 10, height + 40, SKColorType.Bgra8888, SKAlphaType.Opaque);

        using var contentCanvas = new SKCanvas(content);
        using var axisCanvas = new SKCanvas(axis);

        // Draw backgrounds
        axisCanvas.Clear(SKColors.LightGray);
        contentCanvas.Clear(SKColors.White);

        // Draw line graph on content
        using var greenPaint = new SKPaint
        {
            Color = SKColors.Green,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        int x = width, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            y = (int)((float)i / YValues.Count * height);
            x = (int)(YValues[i] / MaxValue() * width);
            contentCanvas.DrawLine(x0, y0, x, y, greenPaint);
        }

        // Draw peaks if they exist
        if (_peaks != null)
        {
            using var redPaint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };
            using var redTextPaint = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true
            };
            using var peakFont = new SKFont(SKTypeface.Default, 12);

            var multConst = (double)height / YValues.Count;
            var i = 0;
            foreach (var p in _peaks)
            {
                contentCanvas.DrawLine(width, (int)(p.Left * multConst), width - 30,
                    (int)(p.Center * multConst), redPaint);
                contentCanvas.DrawLine(width - 30, (int)(p.Center * multConst), width,
                    (int)(p.Right * multConst), redPaint);
                contentCanvas.DrawText(i + ".", width - 38, (int)(p.Center * multConst) + 5, peakFont,
                    redTextPaint);
                i++;
            }
        }

        // Draw content on axis image
        axisCanvas.DrawBitmap(content, 5, 5);

        using var borderPaint = new SKPaint
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1
        };
        axisCanvas.DrawRect(5, 5, content.Width, content.Height, borderPaint);

        content.Dispose();
        return axis;
    }

    /// <summary>
    /// Applies a moving average (rank) filter to smooth the Y-values.
    /// </summary>
    /// <param name="size">The window size for the filter.</param>
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

    /// <summary>
    /// Finds the index of the left foot of a peak, where the value drops below a
    /// relative threshold of the peak value.
    /// </summary>
    /// <param name="peak">The index of the peak center.</param>
    /// <param name="peakFootConstantRel">The relative threshold (0.0 to 1.0).</param>
    /// <returns>The index of the left foot of the peak.</returns>
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

    /// <summary>
    /// Finds the index of the right foot of a peak, where the value drops below a
    /// relative threshold of the peak value.
    /// </summary>
    /// <param name="peak">The index of the peak center.</param>
    /// <param name="peakFootConstantRel">The relative threshold (0.0 to 1.0).</param>
    /// <returns>The index of the right foot of the peak.</returns>
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

    /// <summary>
    /// Computes the average width (diff) of the given peaks.
    /// </summary>
    /// <param name="peaks">The list of peaks.</param>
    /// <returns>The average peak width.</returns>
    public float AveragePeakDiff(List<Peak> peaks)
    {
        var sum = 0f;

        foreach (var peak in peaks)
            sum += peak.Diff;

        return sum / peaks.Count;
    }

    /// <summary>
    /// Returns the maximum width (diff) among peaks in the range [from, to].
    /// </summary>
    /// <param name="peaks">The list of peaks.</param>
    /// <param name="from">The start index (inclusive).</param>
    /// <param name="to">The end index (inclusive).</param>
    /// <returns>The maximum peak width in the range.</returns>
    public float MaximumPeakDiff(List<Peak> peaks, int from, int to)
    {
        float max = 0;

        for (var i = from; i <= to; i++)
            max = Math.Max(max, peaks[i].Diff);

        return max;
    }

    /// <summary>
    /// Adds a Y-value to the graph and invalidates cached statistics.
    /// </summary>
    /// <param name="value">The Y-value to add.</param>
    public void AddPeak(float value)
    {
        YValues.Add(value);
        DeActualizeFlags();
    }
}
