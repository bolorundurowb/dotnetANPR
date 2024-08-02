using System;
using System.Collections.Generic;

namespace DotNetANPR.ImageAnalysis;

public class ProbabilityDistributor(float center, float power, int leftMargin, int rightMargin)
{
    private readonly int _leftMargin = Math.Max(1, leftMargin);
    private readonly int _rightMargin = Math.Max(1, rightMargin);

    public List<float> Distribute(List<float> peaks)
    {
        var distributedPeaks = new List<float>();

        for (var i = 0; i < peaks.Count; i++)
        {
            if ((i < _leftMargin) || (i > (peaks.Count - _rightMargin)))
                distributedPeaks.Add(0f);
            else
                distributedPeaks.Add(DistributionFunction(peaks[i], ((float)i / peaks.Count)));
        }

        return distributedPeaks;
    }

    private float DistributionFunction(float value, float positionPercentage) =>
        value * (1 - (power * Math.Abs(positionPercentage - center)));
}
