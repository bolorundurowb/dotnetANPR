using System;
using System.Collections.Generic;
using System.Linq;
using dotnetANPR.Configuration;

namespace dotnetANPR.ImageAnalysis;

internal sealed class BandGraph : Graph
{
    private readonly Band _handle;
    private readonly AnprSettings _settings;

    public BandGraph(Band handle, AnprSettings settings)
    {
        _handle = handle;
        _settings = settings;
    }

    public void FindPeaks(int count)
    {
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

            var leftIndex = IndexOfLeftPeakRel(maxIndex, _settings.BandGraphPeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, _settings.BandGraphPeakFootConstant);
            var diff = rightIndex - leftIndex;
            leftIndex -= (int)Math.Round(_settings.BandGraphPeakDiffMultiplicationConstant * diff);
            rightIndex += (int)Math.Round(_settings.BandGraphPeakDiffMultiplicationConstant * diff);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        List<Peak> outPeaksFiltered = [];
        outPeaksFiltered.AddRange(outPeaks.Where(p => p.Diff > 2 * _handle.Height && p.Diff < 15 * _handle.Height));
        outPeaksFiltered.Sort(new PeakComparator(YValues));
        Peaks = outPeaksFiltered;
    }
}
