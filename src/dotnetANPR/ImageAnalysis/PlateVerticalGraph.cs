using System;
using System.Collections.Generic;

namespace dotnetANPR.ImageAnalysis
{
    public class PlateVerticalGraph : Graph
    {
        private static double peakFootConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("plateverticalgraph_peakfootconstant");

        Plate handle;

        public PlateVerticalGraph(Plate plate)
        {
            handle = plate;
        }

        public class PeakComparer : IComparer<Peak>
        {
            PlateVerticalGraph graphHandle = null;

            public PeakComparer(PlateVerticalGraph graph)
            {
                graphHandle = graph;
            }

            private float GetPeakValue(Peak peak)
            {
                return graphHandle.YValues[peak.Center];
            }

            public int Compare(Peak peak1, Peak peak2)
            { 
                double comparison = GetPeakValue(peak2) - GetPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> FindPeak(int count)
        {
            for (var i = 0; i < YValues.Count; i++)
            {
                YValues.Insert(i, YValues[i] - GetMinValue());
            }
            var outPeaks = new List<Peak>();
            for (var c = 0; c < count; c++)
            {
                var maxValue = 0.0f;
                var maxIndex = 0;
                for (var i = 0; i < YValues.Count; i++)
                {
                    if (AllowedInterval(outPeaks, i))
                    {
                        if (YValues[i] >= maxValue)
                        {
                            maxValue = YValues[i];
                            maxIndex = i;
                        }
                    }
                }

                if (YValues[maxIndex] < 0.05 * GetMaxValue())
                {
                    break;
                }
                var leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                var rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(YValues.Count - 1, rightIndex)
                ));
            }
            outPeaks.Sort(new PeakComparer(this));
            Peaks = outPeaks;
            return outPeaks;
        }
    }
}
