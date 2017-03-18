﻿using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class CarSnapshotGraph : Graph
    {
        private static double peakFootConstant=
            Intelligence.Configurator.GetDoubleProperty("carsnapshotgraph_peakfootconstant");
        private static double peakDiffMultiplicationConstant =
            Intelligence.Configurator.GetDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant");
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
                double comparison = GetPeakValue(y) - GetPeakValue(x);
                if (comparison < 0)
                {
                    return -1;
                }
                return comparison > 0 ? 1 : 0;
            }
        }

        public void FindPeaks(int numberOfCandidates)
        {
            throw new System.NotImplementedException();
        }
    }
}
