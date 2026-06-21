using System;
using System.Collections.Generic;
using dotnetANPR.Configuration;

namespace dotnetANPR.ImageAnalysis;

internal sealed class PlateVerticalGraph : Graph
{
    private readonly AnprSettings _settings;

    public PlateVerticalGraph(AnprSettings settings) => _settings = settings;

    public List<Peak> FindPeak(int count, AnprSettings settings)
    {
        for (var i = 0; i < YValues.Count; i++)
            YValues[i] -= MinValue();

        List<Peak> outPeaks = [];
        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;

            for (var i = 0; i < YValues.Count; i++)
                if (IsOutsideAllPeaks(outPeaks, i) && YValues[i] >= maxValue)
                {
                    maxValue = YValues[i];
                    maxIndex = i;
                }

            if (YValues[maxIndex] < 0.05 * MaxValue())
                break;

            var leftIndex = IndexOfLeftPeakRel(maxIndex, settings.PlateVerticalGraphPeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, settings.PlateVerticalGraphPeakFootConstant);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;
        return outPeaks;
    }
}
