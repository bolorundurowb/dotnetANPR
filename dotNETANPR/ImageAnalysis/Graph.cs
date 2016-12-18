using System;
using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class Graph
    {
        public class Peak
        {
            public int Left { get; set; }
            public int Center { get; set; }
            public int Right { get; set; }

            public Peak(int left, int center, int right)
            {
                Left = left;
                Center = center;
                Right = right;
            }

            public Peak(int left, int right)
            {
                Left = left;
                Center = (left + right) / 2;
                Right = right;
            }
        }

        public class ProbabilityDistributor
        {
            public float Center { get; set; }
            public float Power { get; set; }
            public int LeftMargin { get; set; }
            public int RightMargin { get; set; }

            public ProbabilityDistributor(float center, float power, int leftMargin, int rightMargin)
            {
                Center = center;
                Power = power;
                LeftMargin = Math.Max(1, leftMargin);
                RightMargin = Math.Max(1, rightMargin);
            }

            private float DistributionFunction(float value, float positionPercentage)
            {
                return value * (1 - Power * Math.Abs(positionPercentage - Center));
            }

            public List<float> Distribute(List<float> peaks)
            {
                List<float> distributedPeaks = new List<float>();
                for (int i = 0; i < peaks.Count; i++)
                {
                    if (i < LeftMargin || i > peaks.Count - RightMargin)
                    {
                        distributedPeaks.Add(0f);
                    }
                    else
                    {
                        distributedPeaks.Add(DistributionFunction(peaks[i], (float)i/peaks.Count));
                    }
                }
                return distributedPeaks;
            }
        }

        public List<Peak> peaks = null;
        public List<float> yValues = new List<float>();
        // Statistical Information
        private bool actualAverageValue;
        private bool actualMaximumValue;
        private bool actualMinimumValue;
        private float averageValue;
        private float maximumValue;
        private float minimumValue;

        public void DeActualizeFlags()
        {
            actualAverageValue = false;
            actualMaximumValue = false;
            actualMinimumValue = false;
        }

        // Methodds for searching bands in image
        public bool AllowedInterval(List<Peak> peaks, int xPosition)
        {
            foreach (Peak peak in peaks)
            {
                if (peak.Left <= xPosition && xPosition<=peak.Right)
                {
                    return false;
                }
                return true;
            }
        }

        public void AddPeak(float value)
        {
            yValues.Add(value);
            DeActualizeFlags();
        }

        public void ApplyProbabilityDistributor(ProbabilityDistributor probabilityDistributor)
        {
            yValues = probabilityDistributor.Distribute(yValues);
            DeActualizeFlags();
        }

        public void Negate()
        {
            float max = GetMaxValue();
            for (int i = 0; i < yValues.Count; i++)
            {
                yValues[i] = max - yValues[i];
            }
        }

        public float GetAverageValue()
        {
            if (!actualAverageValue)
            {
                averageValue = GetAverageValue(0, yValues.Count);
                actualAverageValue = true;
            }
            return averageValue;
        }

        public float GetAverageValue(int a, int b)
        {
            float sum = 0.0f;
            for (int i = a; i < b; i++)
            {
                sum += yValues[i];
            }
            return sum / yValues.Count;
        }

        public float GetMaxValue()
        {
            if (actualMaximumValue)
            {
                maximumValue = GetMaxValue(0, yValues.Count);
                actualMaximumValue = true;
            }
            return maximumValue;
        }

        public float GetMaxValue(int a, int b)
        {
            float maxValue = 0.0f;
            for (int i = a; i < b; i++)
            {
                maxValue = Math.Max(maxValue, yValues[i]);
            }
            return maxValue;
        }

        public float GetMaxValue(float a, float b)
        {
            int ia = (int) (a * yValues.Count);
            int ib = (int) (b * yValues.Count);
            return GetMaxValue(ia, ib);
        }

        public int GetMaxValueIndex(int a, int b)
        {
            float maxValue = 0.0f;
            int maxIndex = a;
            for (int i = a; i < b; i++)
            {
                if (yValues[i] >= maxValue)
                {
                    maxValue = yValues[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        public float GetMinValue()
        {
            if (!actualMinimumValue)
            {
                minimumValue = GetMinValue(0, yValues.Count);
                actualMinimumValue = true;
            }
            return minimumValue;
        }

        public float GetMinValue(int a, int b)
        {
            float minValue = float.PositiveInfinity;
            for (int i = a; i < b; i++)
            {
                minValue = Math.Min(minValue, yValues[i]);
            }
            return minValue;
        }

        public float GetMinValue(float a, float b)
        {
            int ia = (int) (a * yValues.Count);
            int ib = (int) (b * yValues.Count);
            return GetMinValue(ia, ib);
        }

        public int GetMinValueIndex(int a, int b)
        {
            float minValue = float.PositiveInfinity;
            int minIndex = b;
            for (int i = a; i < b; i++)
            {
                if (yValues[i] <= minValue)
                {
                    minValue = yValues[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }
    }
}
