using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class PlateHorizontalGraph : Graph
{
    private static readonly int HorizontalDetectionType =
        Configurator.Instance.Get<int>("platehorizontalgraph_detectionType");

    public float Derivation(int index1, int index2) => YValues[index1] - YValues[index2];

    public List<Peak> FindPeak() => HorizontalDetectionType == 1 
        ? FindPeakEdgeDetection() 
        : FindPeakDerivative();

    public List<Peak> FindPeakDerivative()
    {
        var a = 2;
        var b = YValues.Count - 1 - 2;
        var maxVal = MaxValue();
        while (-Derivation(a, a + 4) < maxVal * 0.2 && a < YValues.Count - 2 - 2 - 4)
            a++;

        while (Derivation(b - 4, b) < maxVal * 0.2 && b > a + 2)
            b--;

        var outPeaks = new List<Peak> { new(a, b) };
        Peaks = outPeaks;
        return outPeaks;
    }

    public List<Peak> FindPeakEdgeDetection()
    {
        var average = AverageValue();
        var a = 0;
        var b = YValues.Count - 1;
        while (YValues[a] < average)
            a++;

        while (YValues[b] < average)
            b--;

        a = Math.Max(a - 5, 0);
        b = Math.Min(b + 5, YValues.Count);
        var outPeaks = new List<Peak> { new(a, b) };
        Peaks = outPeaks;
        return outPeaks;
    }
}
