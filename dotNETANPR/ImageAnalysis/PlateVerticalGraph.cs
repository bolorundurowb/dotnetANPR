using System;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
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
                this.graphHandle = graph;
            }

            private float GetPeakValue(Peak peak)
            {
                return this.graphHandle.YValues[peak.Center];
            }

            public int Compare(Peak peak1, Peak peak2)
            { 
                double comparison = this.GetPeakValue(peak2) - this.GetPeakValue(peak1);
                if (comparison < 0) return -1;
                if (comparison > 0) return 1;
                return 0;
            }
        }

        public List<Peak> FindPeak(int count)
        {
            for (int i = 0; i < this.YValues.Count; i++)
            {
                this.YValues.Insert(i, this.YValues[i] - this.GetMinValue());
            }
            List<Peak> outPeaks = new List<Peak>();
            for (int c = 0; c < count; c++)
            {
                float maxValue = 0.0f;
                int maxIndex = 0;
                for (int i = 0; i < this.YValues.Count; i++)
                {
                    if (AllowedInterval(outPeaks, i))
                    {
                        if (this.YValues[i] >= maxValue)
                        {
                            maxValue = this.YValues[i];
                            maxIndex = i;
                        }
                    }
                }

                if (YValues[maxIndex] < 0.05 * base.GetMaxValue())
                {
                    break;
                }
                int leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                int rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(this.YValues.Count - 1, rightIndex)
                ));
            }
            outPeaks.Sort(new PeakComparer(this));
            base.Peaks = outPeaks;
            return outPeaks;
        }
    }
}
