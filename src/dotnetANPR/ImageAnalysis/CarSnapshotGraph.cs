using System;
using System.Collections.Generic;
using dotnetANPR.Configuration;

namespace dotnetANPR.ImageAnalysis;

internal sealed class CarSnapshotGraph : Graph
{
    private readonly AnprSettings _settings;

    public CarSnapshotGraph(AnprSettings settings) => _settings = settings;

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

            var leftIndex = IndexOfLeftPeakRel(maxIndex, _settings.CarSnapshotGraphPeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, _settings.CarSnapshotGraphPeakFootConstant);
            var diff = rightIndex - leftIndex;
            leftIndex -= (int)Math.Round(_settings.CarSnapshotGraphPeakDiffMultiplicationConstant * diff);
            rightIndex += (int)Math.Round(_settings.CarSnapshotGraphPeakDiffMultiplicationConstant * diff);
            outPeaks.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        outPeaks.Sort(new PeakComparator(YValues));
        Peaks = outPeaks;
    }
}
