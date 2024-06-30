using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetANPR.ImageAnalysis;

public class Graph
{
    private List<Peak>? _peaks = null;
    private List<float> yValues = new();


    private bool actualAverageValue = false; // are values up-to-date?
    private bool actualMaximumValue = false; // are values up-to-date?
    private bool actualMinimumValue = false; // are values up-to-date?
    private float averageValue;
    private float maximumValue;
    private float minimumValue;

    public void DeActualizeFlags()
    {
        actualAverageValue = false;
        actualMaximumValue = false;
        actualMinimumValue = false;
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
        yValues = probabilityDistributor.Distribute(yValues);
        DeActualizeFlags();
    }

    public void Negate()
    {
        var max = MaxValue();

        for (var i = 0; i < yValues.Count; i++)
            yValues[i] = max - yValues[i];

        DeActualizeFlags();
    }

    public float AverageValue()
    {
        if (!actualAverageValue)
        {
            averageValue = AverageValue(0, yValues.Count);
            actualAverageValue = true;
        }

        return averageValue;
    }

    public float AverageValue(int a, int b)
    {
        var sum = 0f;

        for (var i = a; i < b; i++)
            sum += yValues[i];

        return sum / yValues.Count;
    }

    public float MaxValue()
    {
        if (!actualMaximumValue)
        {
            maximumValue = MaxValue(0, yValues.Count);
            actualMaximumValue = true;
        }

        return maximumValue;
    }

    public float MaxValue(int a, int b)
    {
        var maxValue = 0.0f;

        for (var i = a; i < b; i++)
            maxValue = Math.Max(maxValue, yValues[i]);

        return maxValue;
    }

    public float MaxValue(float a, float b)
    {
        var ia = (int)(a * yValues.Count);
        var ib = (int)(b * yValues.Count);

        return MaxValue(ia, ib);
    }

    public int MaxValueIndex(int a, int b)
    {
        var maxValue = 0f;
        var maxIndex = a;

        for (var i = a; i < b; i++)
        {
            if (yValues[i] >= maxValue)
            {
                maxValue = yValues[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public float MinValue()
    {
        if (!actualMinimumValue)
        {
            minimumValue = MinValue(0, yValues.Count);
            actualMinimumValue = true;
        }

        return minimumValue;
    }

    public float MinValue(int a, int b)
    {
        var minValue = float.PositiveInfinity;

        for (var i = a; i < b; i++)
            minValue = Math.Min(minValue, yValues[i]);

        return minValue;
    }

    public float MinValue(float a, float b)
    {
        var ia = (int)(a * yValues.Count);
        var ib = (int)(b * yValues.Count);

        return MinValue(ia, ib);
    }

    public int MinValueIndex(int a, int b)
    {
        var minValue = float.PositiveInfinity;
        var maxIndex = a;

        for (var i = a; i < b; i++)
        {
            if (yValues[i] >= minValue)
            {
                minValue = yValues[i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public void RankFilter(int size)
    {
        var halfSize = size / 2;
        var clone = new List<float>(yValues);

        for (var i = halfSize; i < (yValues.Count - halfSize); i++)
        {
            float sum = 0;
            for (var ii = i - halfSize; ii < (i + halfSize); ii++)
                sum += clone[ii];
            yValues[i] = sum / size;
        }
    }

    public int IndexOfLeftPeakRel(int peak, double peakFootConstantRel)
    {
        var index = peak;
        while (index >= 0)
        {
            if (yValues[index] < (peakFootConstantRel * yValues[peak])) 
                break;

            index--;
        }

        return Math.Max(0, index);
    }

    public float AveragePeakDiff(List<Peak> peaks)
    {
        var sum = 0f;

        foreach (var peak in peaks) 
            sum += peak.Diff();

        return sum / peaks.Count;
    }

    public void AddPeak(float value)
    {
        yValues.Add(value);
        DeActualizeFlags();
    }
}
