using System;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class BandGraph : Graph
    {
        private Band handle;

        private static double peakFootConstant =
            Intelligence.Configurator.GetDoubleProperty("bandgraph_peakfootconstant");

        private static double peakDiffMultiplicationConstant =
            Intelligence.Configurator.GetDoubleProperty("bandgraph_peakDiffMultiplicationConstant");

        public BandGraph(Band band)
        {
            handle = band;
        }

        public class PeakComparer : IComparer<Peak>
        {
            private List<float> yValues = null;

            public PeakComparer(List<float> yValues)
            {
                this.yValues = yValues;
            }

            private float GetPeakValue(Peak peak)
            {
                return this.yValues[peak.Center];
            }

            public int Compare(Peak x, Peak y)
            {
                double difference = GetPeakValue(y) - GetPeakValue(x);
                if (difference < 0)
                {
                    return -1;
                }
                if (difference > 0)
                {
                    return 1;
                }
                return 0;
            }
        }

        public List<Peak> FindPeaks(int numOfCandidates)
        {
            List<Peak> outPeaks = new List<Peak>();
            for (int c = 0; c < numOfCandidates; c++)
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
                int leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                int rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);
                int diff = rightIndex - leftIndex;
                leftIndex -= (int) peakDiffMultiplicationConstant * diff;
                rightIndex += (int) peakDiffMultiplicationConstant * diff;

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(this.YValues.Count - 1, rightIndex)
                ));
            }
            List<Peak> outPeaksFiltered = new List<Peak>();
            foreach (Peak peak in outPeaks)
            {
                if (peak.GetDiff() > 2 * handle.GetHeight()
                    && peak.GetDiff() < 15 * handle.GetHeight())
                {
                    outPeaksFiltered.Add(peak);
                }
            }
            outPeaksFiltered.Sort(new PeakComparer(this.YValues));
            base.Peaks = outPeaksFiltered;
            return outPeaksFiltered;
        }
    }
}
