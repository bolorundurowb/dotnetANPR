using System;
using System.Collections;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class PlateGraph : Graph
    {
        private Plate handle;

        private static double plateGraphRelMinPeakSize =
            Intelligence.Configurator.GetDoubleProperty("plategraph_rel_minpeaksize");

        private static double peakFootConstant =
            Intelligence.Configurator.GetDoubleProperty("plategraph_peakfootconstant");

        public PlateGraph(Plate handle)
        {
            this.handle = handle;
        }

        public class SpaceComparer : IComparer
        {
            private List<float> yValues = null;

            public SpaceComparer(List<float> yValues)
            {
                this.yValues = yValues;
            }

            private float GetPeakValue(object peak)
            {
                return ((Peak) peak).Center;
            }

            public int Compare(object x, object y)
            {
                double difference = GetPeakValue(x) - GetPeakValue(y);
                if (difference < 0)
                {
                    return 1;
                }
                if (difference > 0)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
