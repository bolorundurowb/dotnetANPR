using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Graph subclass for analyzing car snapshot images.
/// Finds horizontal bands by detecting peaks in the vertical brightness histogram.
/// </summary>
public class CarSnapshotGraph : Graph
{
    private static readonly double PeakFootConstant =
        AnprConfig.Instance.CarSnapshotGraph.PeakFootConstant;

    private static readonly double PeakDiffMultiplicationConstant =
        AnprConfig.Instance.CarSnapshotGraph.PeakDiffMultiplicationConstant;

    /// <summary>
    /// Finds the specified number of peaks in the histogram, sorted by peak intensity.
    /// </summary>
    /// <param name="count">The maximum number of peaks to find.</param>
    public void FindPeaks(int count)
    {
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

            var leftIndex = IndexOfLeftPeakRel(maxIndex, PeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, PeakFootConstant);
            var diff = rightIndex - leftIndex;
            leftIndex -= (int)Math.Round(PeakDiffMultiplicationConstant * diff);
            rightIndex += (int)Math.Round(PeakDiffMultiplicationConstant * diff);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;
    }
}
