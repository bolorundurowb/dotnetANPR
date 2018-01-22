using System;
using System.Collections.Generic;

namespace dotnetANPR.ImageAnalysis
{
    public class PlateHorizontalGraph : Graph
    {
        private static double peakFootConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("platehorizontalgraph_peakfootconstant");

        private static readonly int HorizontalDetectionType =
            Intelligence.Intelligence.Configurator.GetIntProperty("platehorizontalgraph_detectionType");

        Plate handle;

        public PlateHorizontalGraph(Plate plate)
        {
            handle = plate;
        }

        public float Derivation(int index1, int index2)
        {
            return YValues[index1] - YValues[index2];
        }

        public List<Peak> FindPeak(int i)
        {
            if (HorizontalDetectionType == 1)
            {
                return FindPeakEdgeDetection(i);
            }
            return FindPeakDerivate(i);
        }

        public List<Peak> FindPeakDerivate(int count)
        {
            int a, b;
            var maxVal = GetMaxValue();
            for (a = 2; -Derivation(a, a + 4) < maxVal * 0.2 && a < YValues.Count - 2 - 2 - 4; a++) ;
            for (b = YValues.Count - 1 - 2; Derivation(b - 4, b) < maxVal * 0.2 && b > a + 2; b--) ;

            var outPeaks = new List<Peak> {new Peak(a, b)};
            Peaks = outPeaks;
            return outPeaks;
        }

        public List<Peak> FindPeakEdgeDetection(int count)
        {
            var average = GetAverageValue();
            int a, b;
            for (a = 0; YValues[a] < average; a++) ;
            for (b = YValues.Count - 1; YValues[b] < average; b--) ;

            var outPeaks = new List<Peak>();
            a = Math.Max(a - 5, 0);
            b = Math.Min(b + 5, YValues.Count);

            outPeaks.Add(new Peak(a, b));
            Peaks = outPeaks;
            return outPeaks;
        }
    }
}
