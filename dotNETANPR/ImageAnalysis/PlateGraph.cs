using System;
using System.Collections;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class PlateGraph : Graph
    {
        private Plate handle;

        private static double plateGraphRelMinPeakSize =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("plategraph_rel_minpeaksize");

        private static double peakFootConstant =
            Intelligence.Intelligence.Configurator.GetDoubleProperty("plategraph_peakfootconstant");

        public PlateGraph(Plate handle)
        {
            this.handle = handle;
        }

        public class SpaceComparer : IComparer<Peak>
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

            public int Compare(Peak x, Peak y)
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

        public List<Peak> FindPeaks(int count)
        {
            List<Peak> spacesTemp = new List<Peak>();
            float diffGVal = 2 * GetAverageValue() - GetMaxValue();
            List<float> yValuesNew = new List<float>();
            foreach (float yValue in this.YValues)
            {
                yValuesNew.Add(yValue - diffGVal);
            }
            this.YValues = yValuesNew;
            this.DeActualizeFlags();
            for (int c = 0; c < count; c++)
            {
                float maxValue = 0.0f;
                int maxIndex = 0;
                for (int i = 0; i < this.YValues.Count; i++)
                {
                    if (AllowedInterval(spacesTemp, i))
                    {
                        if (this.YValues[i] >= maxValue)
                        {
                            maxValue = this.YValues[i];
                            maxIndex = i;
                        }
                    }
                }
                if (this.YValues[maxIndex] < plateGraphRelMinPeakSize * GetMaxValue())
                {
                    break;
                }
                int leftIndex = IndexOfLeftPeakRel(maxIndex, peakFootConstant);
                int rightIndex = IndexOfRightPeakRel(maxIndex, peakFootConstant);
                spacesTemp.Add(new Peak(
                    Math.Max(0, leftIndex),
                    maxIndex,
                    Math.Min(YValues.Count - 1, rightIndex)
                ));
            }
            List<Peak> spaces = new List<Peak>();
            foreach (Peak peak in spacesTemp)
            {
                if (peak.GetDiff() < 1 * handle.GetHeight())
                {
                    spaces.Add(peak);
                }
            }
            spaces.Sort(new SpaceComparer(this.YValues));
            List<Peak> chars = new List<Peak>();
            if (spaces.Count != 0)
            {
                int minIndex = GetMinValueIndex(0, spaces[0].Center);
                int leftIndex = 0;
                Peak first = new Peak(leftIndex, spaces[0].Center);
                if (first.GetDiff() > 0)
                {
                    chars.Add(first);
                }
            }
            for (int i = 0; i < spaces.Count - 1; i++)
            {
                int left = spaces[i].Center;
                int right = spaces[i + 1].Center;
                chars.Add(new Peak(left, right));
            }

            if (spaces.Count != 0)
            {
                Peak last = new Peak(
                    spaces[spaces.Count - 1].Center,
                    this.YValues.Count -1
                );
                if (last.GetDiff() > 0) chars.Add(last);
            }
            base.Peaks = chars;
            return chars;
        }
    }
}
