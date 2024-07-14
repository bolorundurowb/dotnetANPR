using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class CarSnapshotGraph : Graph
{
    private static readonly double peakFootConstant =
        Configurator.Instance.Get<double>("carsnapshotgraph_peakfootconstant"); // 0.55

    private static readonly double peakDiffMultiplicationConstant =
        Configurator.Instance.Get<double>("carsnapshotgraph_peakDiffMultiplicationConstant"); // 0.1

    public List<Peak> FindPeaks(int count)
    {
        List<Peak> outPeaks = new();
        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;
            for (var i = 0; i < YValues.Count; i++)
            {
                // left to right
                if (AllowedInterval(outPeaks, i))
                {
                    if (YValues[i] >= maxValue)
                    {
                        maxValue = YValues[i];
                        maxIndex = i;
                    }
                }
            }

            // we found the biggest peak
            var leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);
            var diff = rightIndex - leftIndex;
            leftIndex -= (int)Math.Round(peakDiffMultiplicationConstant * diff);
            rightIndex += (int)Math.Round(peakDiffMultiplicationConstant * diff);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;
        return outPeaks;
    }
}
