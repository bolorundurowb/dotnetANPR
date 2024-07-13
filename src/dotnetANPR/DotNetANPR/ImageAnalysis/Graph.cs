using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetANPR.ImageAnalysis;

public class Graph
{
    public  List<Peak>? Peaks = null;
    protected List<float> YValues = new();

    private bool _actualAverageValue; // are values up-to-date?
    private bool _actualMaximumValue; // are values up-to-date?
    private bool _actualMinimumValue; // are values up-to-date?
    private float _averageValue;
    private float _maximumValue;
    private float _minimumValue;

    public void DeActualizeFlags()
    {
        _actualAverageValue = false;
        _actualMaximumValue = false;
        _actualMinimumValue = false;
    }

    // TODO: change name
    public bool AllowedInterval(List<Peak> peaks, int xPosition)
    {
        foreach (var peak in peaks)
            if (peak.Left <= xPosition && xPosition <= peak.Right)
                return false;

        return true;
    }

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

    public Bitmap RenderHorizontally(int width, int height)
    {
        // Create images
        var content = new Bitmap(width, height);
        var axis = new Bitmap(width + 40, height + 40);

        using var graphicContent = Graphics.FromImage(content);
        using var graphicAxis = Graphics.FromImage(axis);

        // Draw background for axis image
        graphicAxis.Clear(Color.LightGray);
        graphicAxis.FillRectangle(Brushes.LightGray, new Rectangle(0, 0, width + 40, height + 40));

        // Draw background for content image
        graphicContent.Clear(Color.White);
        graphicContent.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));

        // Draw line graph on content image
        graphicContent.SmoothingMode = SmoothingMode.AntiAlias;
        var greenPen = new Pen(Color.Green);
        int x = 0, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            x = (int)((float)i / YValues.Count * width);
            y = (int)((1 - (YValues[i] / MaxValue())) * height);
            graphicContent.DrawLine(greenPen, x0, y0, x, y);
        }

        // Draw peaks if they exist
        if (Peaks != null)
        {
            var redPen = new Pen(Color.Red);
            var redBrush = Brushes.Red;
            var font = new Font("Arial", 12);
            var multConst = (double)width / YValues.Count;
            var i = 0;
            foreach (var peak in Peaks)
            {
                graphicContent.DrawLine(redPen, (int)(peak.Left * multConst), 0, (int)(peak.Center * multConst),
                    30);
                graphicContent.DrawLine(redPen, (int)(peak.Center * multConst), 30, (int)(peak.Right * multConst),
                    0);
                graphicContent.DrawString(i + ".", font, redBrush,
                    new PointF((int)(peak.Center * multConst) - 5, 42));
                i++;
            }
        }

        // Draw content image on axis image
        graphicAxis.DrawImage(content, 35, 5);
        graphicAxis.DrawRectangle(Pens.Black, 35, 5, content.Width, content.Height);

        // Draw axis labels and ticks
        var labelFont = new Font("Arial", 12);
        var blackBrush = Brushes.Black;

        for (var ax = 0; ax < content.Width; ax += 50)
        {
            graphicAxis.DrawString(ax.ToString(), labelFont, blackBrush, new PointF(ax + 35, axis.Height - 10));
            graphicAxis.DrawLine(Pens.Black, ax + 35, content.Height + 5, ax + 35, content.Height + 15);
        }

        for (var ay = 0; ay < content.Height; ay += 20)
        {
            graphicAxis.DrawString(((1 - ((float)ay / content.Height)) * 100).ToString("F0") + "%", labelFont,
                blackBrush, new PointF(1, ay + 15));
            graphicAxis.DrawLine(Pens.Black, 25, ay + 5, 35, ay + 5);
        }

        return axis;
    }

    public Bitmap RenderVertically(int width, int height)
    {
        // Create images
        var content = new Bitmap(width, height);
        var axis = new Bitmap(width + 10, height + 40);

        using var graphicContent = Graphics.FromImage(content);
        using var graphicAxis = Graphics.FromImage(axis);

        // Draw background for axis image
        graphicAxis.Clear(Color.LightGray);
        graphicAxis.FillRectangle(Brushes.LightGray, new Rectangle(0, 0, width + 10, height + 40));

        // Draw background for content image
        graphicContent.Clear(Color.White);
        graphicContent.FillRectangle(Brushes.White, new Rectangle(0, 0, width, height));

        // Draw line graph on content image
        graphicContent.SmoothingMode = SmoothingMode.AntiAlias;
        var greenPen = new Pen(Color.Green);
        int x = width, y = 0;
        for (var i = 0; i < YValues.Count; i++)
        {
            var x0 = x;
            var y0 = y;
            y = (int)((float)i / YValues.Count * height);
            x = (int)(YValues[i] / MaxValue() * width);
            graphicContent.DrawLine(greenPen, x0, y0, x, y);
        }

        // Draw peaks if they exist
        if (Peaks != null)
        {
            var redPen = new Pen(Color.Red);
            var redBrush = Brushes.Red;
            var font = new Font("Arial", 12);
            var multConst = (double)height / YValues.Count;
            var i = 0;
            foreach (var p in Peaks)
            {
                graphicContent.DrawLine(redPen, width, (int)(p.Left * multConst), width - 30,
                    (int)(p.Center * multConst));
                graphicContent.DrawLine(redPen, width - 30, (int)(p.Center * multConst), width,
                    (int)(p.Right * multConst));
                graphicContent.DrawString(i + ".", font, redBrush,
                    new PointF(width - 38, (int)(p.Center * multConst) + 5));
                i++;
            }
        }

        // Draw content image on axis image
        graphicAxis.DrawImage(content, 5, 5);
        graphicAxis.DrawRectangle(Pens.Black, 5, 5, content.Width, content.Height);

        return axis;
    }

    public void RankFilter(int size)
    {
        var halfSize = size / 2;
        var clone = new List<float>(YValues);

        for (var i = halfSize; i < (YValues.Count - halfSize); i++)
        {
            float sum = 0;
            for (var ii = i - halfSize; ii < (i + halfSize); ii++)
                sum += clone[ii];
            YValues[i] = sum / size;
        }
    }

    public int IndexOfLeftPeakRel(int peak, double peakFootConstantRel)
    {
        var index = peak;
        while (index >= 0)
        {
            if (YValues[index] < (peakFootConstantRel * YValues[peak]))
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
            if (YValues[index] < (peakFootConstantRel * YValues[peak]))
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
