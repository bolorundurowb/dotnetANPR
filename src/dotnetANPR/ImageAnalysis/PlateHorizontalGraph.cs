using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Graph subclass for horizontal edge analysis of a license plate.
/// Used to crop the left and right edges of the plate image.
/// </summary>
public class PlateHorizontalGraph : Graph
{
    private static readonly int HorizontalDetectionType =
        AnprConfig.Instance.PlateHorizontalGraph.DetectionType;

    /// <summary>
    /// Computes the derivation (difference) between two Y-values at the given indices.
    /// </summary>
    /// <param name="index1">The first index.</param>
    /// <param name="index2">The second index.</param>
    /// <returns>The difference between the Y-values at the two indices.</returns>
    public float Derivation(int index1, int index2) => YValues[index1] - YValues[index2];

    /// <summary>
    /// Finds a single peak representing the plate boundaries using the configured detection method.
    /// </summary>
    /// <returns>A list containing a single peak representing the left and right plate boundaries.</returns>
    public List<Peak> FindPeak() => HorizontalDetectionType == 1
        ? FindPeakEdgeDetection()
        : FindPeakDerivative();

    /// <summary>
    /// Finds the plate boundaries using derivative-based detection.
    /// Scans from left and right to find significant brightness changes.
    /// </summary>
    /// <returns>A list containing a single peak.</returns>
    public List<Peak> FindPeakDerivative()
    {
        var a = 2;
        var b = YValues.Count - 1 - 2;
        var maxVal = MaxValue();
        while (-Derivation(a, a + 4) < maxVal * 0.2 && a < YValues.Count - 2 - 2 - 4)
            a++;

        while (Derivation(b - 4, b) < maxVal * 0.2 && b > a + 2)
            b--;

        var outPeaks = new List<Peak> { new(a, b) };
        Peaks = outPeaks;
        return outPeaks;
    }

    /// <summary>
    /// Finds the plate boundaries using edge detection.
    /// Scans from left and right until the average brightness threshold is exceeded.
    /// </summary>
    /// <returns>A list containing a single peak.</returns>
    public List<Peak> FindPeakEdgeDetection()
    {
        var average = AverageValue();
        var a = 0;
        var b = YValues.Count - 1;
        while (YValues[a] < average)
            a++;

        while (YValues[b] < average)
            b--;

        a = Math.Max(a - 5, 0);
        b = Math.Min(b + 5, YValues.Count);
        var outPeaks = new List<Peak> { new(a, b) };
        Peaks = outPeaks;
        return outPeaks;
    }
}
