﻿using System;
 using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class CarSnapshotGraph : Graph
    {
        private static double peakFootConstant=
            Intelligence.Intelligence.Configurator.GetDoubleProperty("carsnapshotgraph_peakfootconstant");
        private static double peakDiffMultiplicationConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant");
        CarSnapshot handle;

        public CarSnapshotGraph(CarSnapshot carSnapshot)
        {
            this.handle = carSnapshot;
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
                return yValues[peak.Center];
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
                return 0;           }
        }

        public List<Peak> FindPeaks(int numberOfCandidates)
        {
            var outPeaks = new List<Peak>();
            for (var c = 0; c < numberOfCandidates; c++)
            {
                var maxValue = 0.0f;
                var maxIndex = 0;
                for (var i = 0; i < this.YValues.Count; i++)
                {
                    if (!AllowedInterval(outPeaks, i)) continue;
                    if (!(this.YValues[i] >= maxValue)) continue;
                    maxValue = this.YValues[i];
                    maxIndex = i;
                }
                var leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                var rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);
                var diff = rightIndex - leftIndex;
                leftIndex -= (int) peakDiffMultiplicationConstant * diff;
                rightIndex += (int) peakDiffMultiplicationConstant * diff;

                outPeaks.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(this.YValues.Count - 1, rightIndex)
                ));
            }
            outPeaks.Sort(new PeakComparer(this.YValues));
            base.Peaks = outPeaks;
            return outPeaks;
        }
    }
}
