using System;
using System.Collections.Generic;
using System.Linq;
using dotnetANPR.Configuration;

namespace dotnetANPR.ImageAnalysis;

internal sealed class PlateGraph : Graph
{
    private readonly Plate _handle;
    private readonly AnprSettings _settings;

    public PlateGraph(Plate plate, AnprSettings settings)
    {
        _handle = plate;
        _settings = settings;
    }

    public List<Peak> FindPeaks(int count)
    {
        List<Peak> spacesTemp = [];
        var diffGVal = 2 * AverageValue() - MaxValue();
        YValues = YValues.Select(f => f - diffGVal).ToList();
        DeActualizeFlags();

        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;
            for (var i = 0; i < YValues.Count; i++)
            {
                if (IsOutsideAllPeaks(spacesTemp, i))
                {
                    if (!(YValues[i] >= maxValue))
                        continue;

                    maxValue = YValues[i];
                    maxIndex = i;
                }
            }

            if (YValues[maxIndex] < _settings.PlateGraphRelMinPeakSize * MaxValue())
                break;

            var leftIndex = IndexOfLeftPeakRel(maxIndex, _settings.PlateGraphPeakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, _settings.PlateGraphPeakFootConstant);
            spacesTemp.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        var spaces = spacesTemp.Where(p => p.Diff < _handle.Height).ToList();
        spaces.Sort(new SpaceComparator());
        List<Peak> peaks = [];

        if (spaces.Count != 0)
        {
            var leftIndex = 0;
            var first = new Peak(leftIndex, spaces[0].Center);
            if (first.Diff > 0)
                peaks.Add(first);
        }

        for (var i = 0; i < spaces.Count - 1; i++)
            peaks.Add(new Peak(spaces[i].Center, spaces[i + 1].Center));

        if (spaces.Count != 0)
        {
            var last = new Peak(spaces[spaces.Count - 1].Center, YValues.Count - 1);
            if (last.Diff > 0)
                peaks.Add(last);
        }

        Peaks = peaks;
        return peaks;
    }
}
