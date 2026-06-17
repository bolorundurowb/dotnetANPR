using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Graph subclass for analyzing horizontal bands within a car snapshot.
/// Provides peak detection to identify candidate license plate regions.
/// </summary>
/// <param name="handle">The <see cref="Band"/> this graph is associated with.</param>
public class BandGraph(Band handle) : Graph
{
    private static readonly double PeakFootConstant =
        Configurator.Instance.Get<double>("bandgraph_peakfootconstant"); // 0.75

    private static readonly double PeakDiffMultiplicationConstant =
        Configurator.Instance.Get<double>("bandgraph_peakDiffMultiplicationConstant"); // 0.2

    /// <summary>
    /// The Band to which this Graph is related.
    /// </summary>
    private readonly Band _handle = handle;

    /// <summary>
    /// Finds the specified number of peaks in the graph, filtering by plate proportion constraints.
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

        // Filter candidates that don't match plate proportions
        List<Peak> outPeaksFiltered = [];
        outPeaksFiltered.AddRange(outPeaks.Where(p => p.Diff > 2 * _handle.Height && p.Diff < 15 * _handle.Height));
        outPeaksFiltered.Sort(new PeakComparator(YValues));
        Peaks = outPeaksFiltered;
    }

    /// <summary>
    /// Finds the index of the left boundary of a peak using an absolute threshold.
    /// </summary>
    /// <param name="peak">The index of the peak center.</param>
    /// <param name="peakFootConstantAbs">The absolute threshold value.</param>
    /// <returns>The index of the left boundary.</returns>
    public int IndexOfLeftPeakAbs(int peak, double peakFootConstantAbs)
    {
        var index = peak;
        for (var i = peak; i >= 0; i--)
        {
            index = i;
            if (YValues[index] < peakFootConstantAbs)
                break;
        }

        return Math.Max(0, index);
    }

    /// <summary>
    /// Finds the index of the right boundary of a peak using an absolute threshold.
    /// </summary>
    /// <param name="peak">The index of the peak center.</param>
    /// <param name="peakFootConstantAbs">The absolute threshold value.</param>
    /// <returns>The index of the right boundary.</returns>
    public int IndexOfRightPeakAbs(int peak, double peakFootConstantAbs)
    {
        var index = peak;
        for (var i = peak; i < YValues.Count; i++)
        {
            index = i;
            if (YValues[index] < peakFootConstantAbs)
                break;
        }

        return Math.Min(YValues.Count, index);
    }
}
