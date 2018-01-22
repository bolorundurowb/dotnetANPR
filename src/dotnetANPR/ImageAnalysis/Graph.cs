using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotnetANPR.ImageAnalysis
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

            public int GetDiff()
            {
                return Right - Left;
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
                var distributedPeaks = new List<float>();
                for (var i = 0; i < peaks.Count; i++)
                {
                    if (i < LeftMargin || i > peaks.Count - RightMargin)
                    {
                        distributedPeaks.Add(0f);
                    }
                    else
                    {
                        distributedPeaks.Add(DistributionFunction(peaks[i], (float) i / peaks.Count));
                    }
                }

                return distributedPeaks;
            }
        }

        public List<Peak> Peaks = null;

        public List<float> YValues = new List<float>();

        // Statistical Information
        private bool _actualAverageValue;

        private bool _actualMaximumValue;
        private bool _actualMinimumValue;
        private float _averageValue;
        private float _maximumValue;
        private float _minimumValue;

        public void DeActualizeFlags()
        {
            _actualAverageValue = false;
            _actualMaximumValue = false;
            _actualMinimumValue = false;
        }

        // Methods for searching bands in image
        public bool AllowedInterval(List<Peak> peaks, int xPosition)
        {
            foreach (var peak in peaks)
            {
                if (peak.Left <= xPosition && xPosition <= peak.Right)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddPeak(float value)
        {
            YValues.Add(value);
            DeActualizeFlags();
        }

        public void ApplyProbabilityDistributor(ProbabilityDistributor probabilityDistributor)
        {
            YValues = probabilityDistributor.Distribute(YValues);
            DeActualizeFlags();
        }

        public void Negate()
        {
            var max = GetMaxValue();
            for (var i = 0; i < YValues.Count; i++)
            {
                YValues[i] = max - YValues[i];
            }
        }

        public float GetAverageValue()
        {
            if (!_actualAverageValue)
            {
                _averageValue = GetAverageValue(0, YValues.Count);
                _actualAverageValue = true;
            }

            return _averageValue;
        }

        public float GetAverageValue(int a, int b)
        {
            var sum = 0.0f;
            for (var i = a; i < b; i++)
            {
                sum += YValues[i];
            }

            return sum / YValues.Count;
        }

        public float GetMaxValue()
        {
            if (_actualMaximumValue)
            {
                _maximumValue = GetMaxValue(0, YValues.Count);
                _actualMaximumValue = true;
            }

            return _maximumValue;
        }

        public float GetMaxValue(int a, int b)
        {
            var maxValue = 0.0f;
            for (var i = a; i < b; i++)
            {
                maxValue = Math.Max(maxValue, YValues[i]);
            }

            return maxValue;
        }

        public float GetMaxValue(float a, float b)
        {
            var ia = (int) (a * YValues.Count);
            var ib = (int) (b * YValues.Count);
            return GetMaxValue(ia, ib);
        }

        public int GetMaxValueIndex(int a, int b)
        {
            var maxValue = 0.0f;
            var maxIndex = a;
            for (var i = a; i < b; i++)
            {
                if (YValues[i] >= maxValue)
                {
                    maxValue = YValues[i];
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        public float GetMinValue()
        {
            if (!_actualMinimumValue)
            {
                _minimumValue = GetMinValue(0, YValues.Count);
                _actualMinimumValue = true;
            }

            return _minimumValue;
        }

        public float GetMinValue(int a, int b)
        {
            var minValue = float.PositiveInfinity;
            for (var i = a; i < b; i++)
            {
                minValue = Math.Min(minValue, YValues[i]);
            }

            return minValue;
        }

        public float GetMinValue(float a, float b)
        {
            var ia = (int) (a * YValues.Count);
            var ib = (int) (b * YValues.Count);
            return GetMinValue(ia, ib);
        }

        public int GetMinValueIndex(int a, int b)
        {
            var minValue = float.PositiveInfinity;
            var minIndex = b;
            for (var i = a; i < b; i++)
            {
                if (YValues[i] <= minValue)
                {
                    minValue = YValues[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        public Bitmap RenderHorizontally(int width, int height)
        {
            var content = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var axis = new Bitmap(width + 40, height + 40, PixelFormat.Format8bppIndexed);

            var graphicsContent = Graphics.FromImage(content);
            var graphicsAxis = Graphics.FromImage(axis);

            var backRect = new Rectangle(0, 0, width + 40, height + 40);
            var axisBrush = new SolidBrush(Color.LightGray);
            var axisPen = new Pen(Color.LightGray);
            graphicsAxis.FillRectangle(axisBrush, backRect);
            graphicsAxis.DrawRectangle(axisPen, backRect);

            backRect = new Rectangle(0, 0, width, height);
            var contentBrush = new SolidBrush(Color.White);
            var contentPen = new Pen(Color.White);
            graphicsContent.FillRectangle(contentBrush, backRect);
            graphicsContent.DrawRectangle(contentPen, backRect);

            var x = 0;
            var y = 0;

            var graphicsContentPen = new Pen(Color.Green);

            for (var i = 0; i < YValues.Count; i++)
            {
                var x0 = x;
                var y0 = y;
                x = (int) ((float) i / YValues.Count * width);
                y = (int) ((1 - YValues[i] / GetMaxValue()) * height);
                graphicsContent.DrawLine(graphicsContentPen, x0, y0, x, y);
            }

            var graphicsContentFont = new Font("Consolas", 20F);
            if (Peaks != null)
            {
                graphicsContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                var i = 0;
                var multConst = (double) width / YValues.Count;
                foreach (var p in Peaks)
                {
                    graphicsContent.DrawLine(graphicsContentPen, (int) (p.Left * multConst), 0,
                        (int) (p.Center * multConst), 30);
                    graphicsContent.DrawLine(graphicsContentPen, (int) (p.Center * multConst), 30,
                        (int) (p.Right * multConst), 0);
                    graphicsContent.DrawString(i++ + ".", graphicsContentFont, contentBrush,
                        (float) (p.Center * multConst) - 5, 42F);
                }
            }

            graphicsAxis.DrawImage(content, 35, 5);

            axisPen.Color = Color.Black;
            axisBrush.Color = Color.Black;
            var graphicsAxisFont = new Font("Consolas", 20F);
            graphicsAxis.DrawRectangle(axisPen, 35, 5, content.Width, content.Height);

            for (var ax = 0; ax < content.Width; ax += 50)
            {
                graphicsAxis.DrawString(ax.ToString(), graphicsAxisFont, axisBrush, ax + 35, axis.Height - 10);
                graphicsAxis.DrawLine(axisPen, ax + 35, content.Height + 5, ax + 35, content.Height + 15);
            }

            for (var ay = 0; ay < content.Height; ay += 20)
            {
                graphicsAxis.DrawString(Convert.ToInt32((1 - (float) ay) / content.Height * 100) + "%",
                    graphicsContentFont, contentBrush, 1, ay + 15);
                graphicsAxis.DrawLine(axisPen, 25, ay + 5, 35, ay + 5);
            }

            graphicsContent.Dispose();
            graphicsAxis.Dispose();
            return axis;
        }

        public Bitmap RenderVertically(int width, int height)
        {
            var content = new Bitmap(width, height);
            var axis = new Bitmap(width + 10, height + 40);

            var graphicsContent = Graphics.FromImage(content);
            var graphicsAxis = Graphics.FromImage(axis);

            var backRect = new Rectangle(0, 0, width + 40, height + 40);
            var axisBrush = new SolidBrush(Color.LightGray);
            var axisPen = new Pen(Color.LightGray);
            graphicsAxis.FillRectangle(axisBrush, backRect);
            graphicsAxis.DrawRectangle(axisPen, backRect);

            backRect = new Rectangle(0, 0, width, height);
            var contentBrush = new SolidBrush(Color.White);
            var contentPen = new Pen(Color.White);
            graphicsContent.FillRectangle(contentBrush, backRect);
            graphicsContent.DrawRectangle(contentPen, backRect);


            var x = 0;
            var y = 0;

            var graphicsContentPen = new Pen(Color.Green);

            for (var i = 0; i < YValues.Count; i++)
            {
                var x0 = x;
                var y0 = y;
                x = (int) ((float) i / YValues.Count * height);
                y = (int) ((1 - YValues[i] / GetMaxValue()) * width);
                graphicsContent.DrawLine(graphicsContentPen, x0, y0, x, y);
            }

            var graphicsContentFont = new Font("Consolas", 20F);
            if (Peaks != null)
            {
                graphicsContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                var i = 0;
                var multConst = (double) height / YValues.Count;
                foreach (var p in Peaks)
                {
                    graphicsContent.DrawLine(graphicsContentPen, width, (int) (p.Left * multConst), width - 30,
                        (int) (p.Center * multConst));
                    graphicsContent.DrawLine(graphicsContentPen, width - 30, (int) (p.Center * multConst), width,
                        (int) (p.Right * multConst));
                    graphicsContent.DrawString(i++ + ".", graphicsContentFont, contentBrush, width - 38,
                        (float) (p.Center * multConst) + 5);
                }
            }

            graphicsAxis.DrawImage(content, 5, 5);

            axisPen.Color = Color.Black;
            axisBrush.Color = Color.Black;
            graphicsAxis.DrawRectangle(axisPen, 5, 5, content.Width, content.Height);

            graphicsContent.Dispose();
            graphicsAxis.Dispose();
            return axis;
        }

        public void RankFilter(int size)
        {
            var halfSize = size / 2;
            var clone = new List<float>(YValues);
            for (var i = halfSize; i < YValues.Count - halfSize; i++)
            {
                float sum = 0;
                for (var ii = i - halfSize; ii < i + halfSize; ii++)
                {
                    sum += clone[ii];
                }

                YValues[i] = sum / size;
            }
        }

        public int IndexOfLeftPeakRel(int peak, double peakFootConstantRel)
        {
            var index = peak;
            for (var i = peak; i >= 0; i--)
            {
                index = i;
                if (YValues[index] < peakFootConstantRel * YValues[peak]) break;
            }

            return Math.Max(0, index);
        }

        public int IndexOfRightPeakRel(int peak, double peakFootConstantRel)
        {
            var index = peak;
            for (var i = peak; i < YValues.Count; i++)
            {
                index = i;
                if (YValues[index] < peakFootConstantRel * YValues[peak]) break;
            }

            return Math.Min(YValues.Count, index);
        }


        public float AveragePeakDiff(List<Peak> peaks)
        {
            float sum = 0;
            foreach (var p in peaks)
                sum += p.GetDiff();
            return sum / peaks.Count;
        }

        public float MaximumPeakDiff(List<Peak> peaks, int from, int to)
        {
            float max = 0;
            for (var i = from; i <= to; i++)
                max = Math.Max(max, peaks[i].GetDiff());
            return max;
        }
    }
}