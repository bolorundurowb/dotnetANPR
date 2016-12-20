using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

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

        // Methods for searching bands in image
        public bool AllowedInterval(List<Peak> peaks, int xPosition)
        {
            foreach (Peak peak in peaks)
            {
                if (peak.Left <= xPosition && xPosition<=peak.Right)
                {
                    return false;
                }
            }
            return true;
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

        public Bitmap RenderHorizontally(int width, int height)
        {
            Bitmap content = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Bitmap axis = new Bitmap(width + 40, height + 40, PixelFormat.Format8bppIndexed);

            Graphics graphicsContent = Graphics.FromImage(content);
            Graphics graphicsAxis = Graphics.FromImage(axis);

            Rectangle backRect = new Rectangle(0, 0, width + 40, height + 40);
            SolidBrush axisBrush = new SolidBrush(Color.LightGray);
            Pen axisPen = new Pen(Color.LightGray);
            graphicsAxis.FillRectangle(axisBrush, backRect);
            graphicsAxis.DrawRectangle(axisPen, backRect);

            backRect = new Rectangle(0, 0, width, height);
            SolidBrush contentBrush = new SolidBrush(Color.White);
            Pen contentPen = new Pen(Color.White);
            graphicsContent.FillRectangle(contentBrush, backRect);
            graphicsContent.DrawRectangle(contentPen, backRect);

            int x, y;
            int y0;
            x = 0;
            y = 0;

            Pen graphicsContentPen = new Pen(Color.Green);

            for (int i = 0; i < yValues.Count; i++)
            {
                var x0 = x;
                y0 = y;
                x = (int) ((float) i / yValues.Count * width);
                y = (int) ((1 - yValues[i] / GetMaxValue()) * height);
                graphicsContent.DrawLine(graphicsContentPen, x0, y0, x, y);
            }

            Font graphicsContentFont = new Font("Consolas", 20F);
            if (peaks != null)
            {
                graphicsContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double) width / yValues.Count;
                foreach (Peak p in peaks)
                {
                    graphicsContent.DrawLine(graphicsContentPen, (int) (p.Left * multConst), 0,
                        (int) (p.Center * multConst), 30);
                    graphicsContent.DrawLine(graphicsContentPen, (int) (p.Center * multConst), 30,
                        (int) (p.Right * multConst), 0);
                    graphicsContent.DrawString((i++) + ".", graphicsContentFont, contentBrush,
                        (float) (p.Center * multConst) - 5, 42F);
                }
            }

            graphicsAxis.DrawImage(content, 35, 5);

            axisPen.Color = (Color.Black);
            axisBrush.Color = Color.Black;
            Font graphicsAxisFont = new Font("Consolas", 20F);
            graphicsAxis.DrawRectangle(axisPen, 35, 5, content.Width, content.Height);

            for (int ax = 0; ax < content.Width; ax += 50)
            {
                graphicsAxis.DrawString(ax.ToString(), graphicsAxisFont, axisBrush, ax + 35, axis.Height - 10);
                graphicsAxis.DrawLine(axisPen, ax + 35, content.Height + 5, ax + 35, content.Height + 15);
            }

            for (int ay = 0; ay < content.Height; ay += 20)
            {
                graphicsAxis.DrawString(Convert.ToInt32(((1 - (float) ay) / content.Height) * 100) + "%",
                    graphicsContentFont, contentBrush, 1, ay + 15);
                graphicsAxis.DrawLine(axisPen, 25, ay + 5, 35, ay + 5);
            }
            graphicsContent.Dispose();
            graphicsAxis.Dispose();
            return axis;
        }

        public Bitmap RenderVertically(int width, int height)
        {
            Bitmap content = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Bitmap axis = new Bitmap(width + 10, height + 40, PixelFormat.Format8bppIndexed);

            Graphics graphicsContent = Graphics.FromImage(content);
            Graphics graphicsAxis = Graphics.FromImage(axis);

            Rectangle backRect = new Rectangle(0, 0, width + 40, height + 40);
            SolidBrush axisBrush = new SolidBrush(Color.LightGray);
            Pen axisPen = new Pen(Color.LightGray);
            graphicsAxis.FillRectangle(axisBrush, backRect);
            graphicsAxis.DrawRectangle(axisPen, backRect);

            backRect = new Rectangle(0, 0, width, height);
            SolidBrush contentBrush = new SolidBrush(Color.White);
            Pen contentPen = new Pen(Color.White);
            graphicsContent.FillRectangle(contentBrush, backRect);
            graphicsContent.DrawRectangle(contentPen, backRect);


            int x, y, x0;
            x = 0; y = 0;

            Pen graphicsContentPen = new Pen(Color.Green);

            for (int i = 0; i < yValues.Count; i++)
            {
                x0 = x; var y0 = y;
                x = (int)((float)i / yValues.Count * height);
                y = (int)((1 - yValues[i] / GetMaxValue()) * width);
                graphicsContent.DrawLine(graphicsContentPen, x0, y0, x, y);
            }

            Font graphicsContentFont = new Font("Consolas", 20F);
            if (peaks != null)
            {
                graphicsContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double)height / yValues.Count;
                foreach (Peak p in peaks)
                {
                    graphicsContent.DrawLine(graphicsContentPen, width, (int) (p.Left * multConst), width - 30,
                        (int) (p.Center * multConst));
                    graphicsContent.DrawLine(graphicsContentPen, width - 30, (int) (p.Center * multConst), width,
                        (int) (p.Right * multConst));
                    graphicsContent.DrawString((i++) + ".", graphicsContentFont, contentBrush, width - 38,
                        (float) (p.Center * multConst) + 5);
                }
            }

            graphicsAxis.DrawImage(content, 5, 5);

            axisPen.Color = (Color.Black);
            axisBrush.Color = Color.Black;
            graphicsAxis.DrawRectangle(axisPen, 5, 5, content.Width, content.Height);

            graphicsContent.Dispose();
            graphicsAxis.Dispose();
            return axis;
        }

        public void RankFilter(int size)
        {
            int halfSize = size / 2;
            List<float> clone = new List<float>(yValues);
            for (int i = halfSize; i < yValues.Count - halfSize; i++)
            {
                float sum = 0;
                for (int ii = i - halfSize; ii < i + halfSize; ii++)
                {
                    sum += clone[ii];
                }
                yValues[i] = (sum / size);
            }
        }
    }
}
