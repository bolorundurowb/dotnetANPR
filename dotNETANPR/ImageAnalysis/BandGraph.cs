using System;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class BandGraph : Graph
    {
        private Band _handle;

        private static double _peakFootConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("bandgraph_peakfootconstant");

        private static double _peakDiffMultiplicationConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("bandgraph_peakDiffMultiplicationConstant");

        public BandGraph(Band band)
        {
            _handle = band;
        }

        public class PeakComparer : IComparer<Peak>
        {
            private List<float> _yValues;

            public PeakComparer(List<float> yValues)
            {
                _yValues = yValues;
            }

            private float GetPeakValue(Peak peak)
            {
                return _yValues[peak.Center];
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
                for (int i = 0; i < YValues.Count; i++)
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
                int leftIndex = IndexOfLeftPeakRel(maxIndex, _peakFootConstant);
                int rightIndex = IndexOfRightPeakRel(maxIndex, _peakFootConstant);
                int diff = rightIndex - leftIndex;
                leftIndex -= (int) _peakDiffMultiplicationConstant * diff;
                rightIndex += (int) _peakDiffMultiplicationConstant * diff;

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(YValues.Count - 1, rightIndex)
                ));
            }
            List<Peak> outPeaksFiltered = new List<Peak>();
            foreach (Peak peak in outPeaks)
            {
                if (peak.GetDiff() > 2 * _handle.GetHeight()
                    && peak.GetDiff() < 15 * _handle.GetHeight())
                {
                    outPeaksFiltered.Add(peak);
                }
            }
            outPeaksFiltered.Sort(new PeakComparer(YValues));
            Peaks = outPeaksFiltered;
            return outPeaksFiltered;
        }

        public int IndexOfLeftPeakAbs(int peak, double peakFootConstantAbs)
        {
            int index = peak;
            for (int i = peak; i >= 0; i--)
            {
                index = i;
                if (YValues[index] < peakFootConstantAbs)
                {
                    break;
                }
            }
            return Math.Max(0, index);
        }

        public int IndexOfRightPeakAbs(int peak, double peakFootConstantAbs)
        {
            int index = peak;
            for (int i = peak; i < YValues.Count; i++)
            {
                index = i;
                if (YValues[index] < peakFootConstantAbs)
                {
                    break;
                }
            }
            return Math.Min(YValues.Count, index);
        }
    }
}
