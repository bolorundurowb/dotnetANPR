using System;
using System.Collections.Generic;
using System.Linq;

namespace dotnetANPR.ImageAnalysis;

internal class Graph
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

    public bool IsOutsideAllPeaks(List<Peak> peaks, int xPosition) =>
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
