using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class BandGraph(Band handle) : Graph
{
    private static readonly double PeakFootConstant =
        Configurator.Instance.Get<double>("bandgraph_peakfootconstant"); // 0.75

    private static readonly double PeakDiffMultiplicationConstant =
        Configurator.Instance.Get<double>("bandgraph_peakDiffMultiplicationConstant"); // 0.2

    /**
     * The Band to which this Graph is related.
     */
    private readonly Band _handle = handle;

    public List<Peak> FindPeaks(int count)
    {
        List<Peak> outPeaks = [];
        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;
            for (var i = 0; i < YValues.Count; i++)
                // left to right
                if (AllowedInterval(outPeaks, i))
                    if (YValues[i] >= maxValue)
                    {
                        maxValue = YValues[i];
                        maxIndex = i;
                    }

            // we found the biggest peak, let's do the first cut
            var leftIndex = IndexOfLeftPeakRel(maxIndex, PeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, PeakFootConstant);
            var diff = rightIndex - leftIndex;
            leftIndex -= (int)Math.Round(PeakDiffMultiplicationConstant * diff);
            rightIndex += (int)Math.Round(PeakDiffMultiplicationConstant * diff);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        // filter the candidates that don't correspond with plate proportions
        List<Peak> outPeaksFiltered = [];
        outPeaksFiltered.AddRange(outPeaks.Where(p => (p.Diff > (2 * _handle.Height)) && (p.Diff < (15 * _handle.Height))));
        outPeaksFiltered.Sort(new PeakComparator(YValues));
        Peaks = outPeaksFiltered;
        return outPeaksFiltered;
    }

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
