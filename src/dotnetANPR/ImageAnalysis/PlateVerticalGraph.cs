using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Graph subclass for vertical analysis of a license plate.
/// Used to crop the top and bottom edges of the plate image by detecting
/// the vertical extent of content.
/// </summary>
public class PlateVerticalGraph : Graph
{
    private static readonly double PeakFootConstant =
        Configurator.Instance.Get<double>("plateverticalgraph_peakfootconstant");

    /// <summary>
    /// Finds peaks representing the vertical extent of plate content.
    /// Values are first shifted by subtracting the minimum, then peaks are detected
    /// and sorted by their center Y-value intensity.
    /// </summary>
    /// <param name="count">The maximum number of peaks to find.</param>
    /// <returns>A sorted list of detected peaks.</returns>
    public List<Peak> FindPeak(int count)
    {
        // Lower the peak by subtracting the minimum value
        for (var i = 0; i < YValues.Count; i++)
            YValues[i] -= MinValue();

        List<Peak> outPeaks = [];
        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;

            for (var i = 0; i < YValues.Count; i++)
                if (AllowedInterval(outPeaks, i))
                    if (YValues[i] >= maxValue)
                    {
                        maxValue = YValues[i];
                        maxIndex = i;
                    }

            if (YValues[maxIndex] < 0.05 * MaxValue())
                break;

            var leftIndex = IndexOfLeftPeakRel(maxIndex, PeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, PeakFootConstant);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;

        return outPeaks;
    }
}
