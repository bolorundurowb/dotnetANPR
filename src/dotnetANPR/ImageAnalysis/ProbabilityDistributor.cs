using System;
using System.Collections.Generic;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Applies a probability distribution function to a list of peak values,
/// attenuating values based on their relative position and distance from a center point.
/// </summary>
/// <param name="center">The center position of the distribution (0.0 to 1.0).</param>
/// <param name="power">The power (strength) of the distribution attenuation.</param>
/// <param name="leftMargin">Number of values to zero out on the left edge.</param>
/// <param name="rightMargin">Number of values to zero out on the right edge.</param>
public class ProbabilityDistributor(float center, float power, int leftMargin, int rightMargin)
{
    private readonly int _leftMargin = Math.Max(1, leftMargin);
    private readonly int _rightMargin = Math.Max(1, rightMargin);

    /// <summary>
    /// Distributes the given peak values by applying the probability distribution function.
    /// Values within the left and right margins are zeroed out.
    /// </summary>
    /// <param name="peaks">The list of peak values to distribute.</param>
    /// <returns>A new list of distributed peak values.</returns>
    public List<float> Distribute(List<float> peaks)
    {
        var distributedPeaks = new List<float>();

        for (var i = 0; i < peaks.Count; i++)
        {
            if (i < _leftMargin || i > peaks.Count - _rightMargin)
                distributedPeaks.Add(0f);
            else
                distributedPeaks.Add(DistributionFunction(peaks[i], (float)i / peaks.Count));
        }

        return distributedPeaks;
    }

    private float DistributionFunction(float value, float positionPercentage) =>
        value * (1 - power * Math.Abs(positionPercentage - center));
}
