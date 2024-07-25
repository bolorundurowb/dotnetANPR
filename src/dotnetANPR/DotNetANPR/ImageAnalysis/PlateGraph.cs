using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class PlateGraph : Graph
{
    /**
    * 0.75: Smaller numbers have a tendency to cut characters, bigger have a tendency to incorrectly merge them.
    */
    private static double _plategraphRelMinpeaksize =
        Configurator.Instance.Get<double>("plategraph_rel_minpeaksize");

    private static double _peakFootConstant =
        Configurator.Instance.Get<double>("plategraph_peakfootconstant");

    private readonly Plate _handle;

    public PlateGraph(Plate plate) { _handle = plate; }

    // Find peaks in the PlateGraph.
    //
    // Graph changes before segmentation:
    // 1. Get the average value and maxval
    // 2. Change the minval; diffVal = average - (maxval - average) = 2average - maxval, val -= diffVal
    //
    // @param count number of peaks
    // @return a List of Peaks
    public List<Peak> FindPeaks(int count)
    {
        List<Peak> spacesTemp = [];
        var diffGVal = (2 * AverageValue()) - MaxValue();
        YValues = YValues.Select(f => f - diffGVal).ToList();
        
        DeActualizeFlags();
        
        for (var c = 0; c < count; c++)
        {
            var maxValue = 0.0f;
            var maxIndex = 0;
            for (var i = 0; i < YValues.Count; i++)
            {
                // left to right
                if (AllowedInterval(spacesTemp, i))
                {
                    if (!(YValues[i] >= maxValue))
                        continue;

                    maxValue = YValues[i];
                    maxIndex = i;
                }
            }

            // we found the biggest peak
            if (YValues[maxIndex] < (_plategraphRelMinpeaksize * MaxValue())) 
                break;

            // width of the detected space
            var leftIndex = IndexOfLeftPeakRel(maxIndex, _peakFootConstant);
            var rightIndex = IndexOfRightPeakRel(maxIndex, _peakFootConstant);
            spacesTemp.Add(new Peak(Math.Max(0, leftIndex), maxIndex, Math.Min(YValues.Count - 1, rightIndex)));
        }

        // we need to filter candidates that don't have the right proportions
        var spaces = spacesTemp.Where(p => p.Diff < _handle.Height).ToList();

        // List<Peak> spaces contains spaces, sort them left to right
        spaces.Sort(new SpaceComparator());
        List<Peak> peaks = [];
        // + + +++ +++ + + +++ + + + + + + + + + + + ++ + + + ++ +++ +++ | | 1 | 2 .... | +--> 1. local minimum
        // count the char to the left of the space
        if (spaces.Count != 0)
        {
            // detect the first local minimum on the graph
            var leftIndex = 0;
            var first = new Peak(leftIndex /* 0 */, spaces[0].Center);
            if (first.Diff > 0)
                peaks.Add(first);
        }

        for (var i = 0; i < (spaces.Count - 1); i++)
        {
            var left = spaces[i].Center;
            var right = spaces[i + 1].Center;
            peaks.Add(new Peak(left, right));
        }

        // character to the right of last space
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
