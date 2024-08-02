using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class PlateVerticalGraph : Graph
{
    private static readonly double PeakFootConstant =
        Configurator.Instance.Get<double>("plateverticalgraph_peakfootconstant");

    public List<Peak> FindPeak(int count)
    {
        // lower the peak
        for (var i = 0; i < YValues.Count; i++)
            YValues[i] -= MinValue();

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

            // we found the biggest peak
            if (YValues[maxIndex] < 0.05 * MaxValue())
                break; // 0.4

            var leftIndex = IndexOfLeftPeakRel(maxIndex, PeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, PeakFootConstant);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;

        return outPeaks;
    }
}
