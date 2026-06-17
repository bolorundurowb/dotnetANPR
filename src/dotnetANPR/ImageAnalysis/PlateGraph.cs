using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Graph subclass for character segmentation within a license plate.
/// Detects spaces between characters to determine character boundaries.
/// </summary>
public class PlateGraph : Graph
{
    /// <summary>
    /// Minimum relative peak size for a valid space detection.
    /// Smaller numbers have a tendency to cut characters, bigger to incorrectly merge them.
    /// </summary>
    private static readonly double _plategraphRelMinpeaksize =
        Configurator.Instance.Get<double>("plategraph_rel_minpeaksize");

    private static readonly double _peakFootConstant =
        Configurator.Instance.Get<double>("plategraph_peakfootconstant");

    private readonly Plate _handle;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlateGraph"/> class.
    /// </summary>
    /// <param name="plate">The <see cref="Plate"/> this graph is associated with.</param>
    public PlateGraph(Plate plate) { _handle = plate; }

    /// <summary>
    /// Finds character peaks by first detecting spaces, then deriving character positions
    /// from the gaps between spaces.
    /// </summary>
    /// <param name="count">The maximum number of space candidates to consider.</param>
    /// <returns>A list of peaks representing character positions.</returns>
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
                if (AllowedInterval(spacesTemp, i))
                {
                    if (!(YValues[i] >= maxValue))
                        continue;

                    maxValue = YValues[i];
                    maxIndex = i;
                }
            }

            if (YValues[maxIndex] < _plategraphRelMinpeaksize * MaxValue())
                break;

            var leftIndex = IndexOfLeftPeakRel(maxIndex, _peakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, _peakFootConstant);
            spacesTemp.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        // Filter candidates that don't have the right proportions
        var spaces = spacesTemp.Where(p => p.Diff < _handle.Height).ToList();

        // Sort spaces left to right
        spaces.Sort(new SpaceComparator());
        List<Peak> peaks = [];

        // Count the char to the left of the first space
        if (spaces.Count != 0)
        {
            var leftIndex = 0;
            var first = new Peak(leftIndex, spaces[0].Center);
            if (first.Diff > 0)
                peaks.Add(first);
        }

        for (var i = 0; i < spaces.Count - 1; i++)
        {
            var left = spaces[i].Center;
            var right = spaces[i + 1].Center;
            peaks.Add(new Peak(left, right));
        }

        // Character to the right of last space
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
