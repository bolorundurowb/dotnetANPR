using System;
using System.Collections.Generic;

namespace dotnetANPR.ImageAnalysis
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
            var outPeaks = new List<Peak>();
            for (var c = 0; c < numOfCandidates; c++)
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
                var leftIndex = IndexOfLeftPeakRel(maxIndex, _peakFootConstant);
                var rightIndex = IndexOfRightPeakRel(maxIndex, _peakFootConstant);
                var diff = rightIndex - leftIndex;
                leftIndex -= (int) _peakDiffMultiplicationConstant * diff;
                rightIndex += (int) _peakDiffMultiplicationConstant * diff;

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(YValues.Count - 1, rightIndex)
                ));
            }
            var outPeaksFiltered = new List<Peak>();
            foreach (var peak in outPeaks)
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
            var index = peak;
            for (var i = peak; i >= 0; i--)
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
            var index = peak;
            for (var i = peak; i < YValues.Count; i++)
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
