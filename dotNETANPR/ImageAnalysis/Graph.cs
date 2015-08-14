using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace dotNETANPR.ImageAnalysis
{
    class Graph
    {
        public class Peak
        {
            public int left, center, right;
            public Peak(int left, int center, int right)
            {
                this.left = left;
                this.center = center;
                this.right = right;
            }
            public Peak(int left, int right)
            {
                this.left = left;
                center = (left + right) / 2;
                this.right = right;
            }
            public int GetLeft()
            {
                return left;
            }
            public int GetRight()
            {
                return right;
            }
            public int GetCenter()
            {
                return center;
            }
            public int GetDiff()
            {
                return right - left;
            }
            public void SetLeft(int left)
            {
                this.left = left;
            }
            public void SetCenter(int center)
            {
                this.center = center;
            }
            public void SetRight(int right)
            {
                this.right = right;
            }
        }
        public class ProbabilityDistributor
        {
            float center;
            float power;
            int leftMargin;
            int rightMargin;
            public ProbabilityDistributor(float center, float power, int leftMargin, int rightMargin)
            {
                this.center = center;
                this.power = power;
                leftMargin = Math.Max(1, leftMargin);
                rightMargin = Math.Max(1, rightMargin);
            }

            private float distributionFunction(float value, float positionPercentage)
            {
                return value * (1 - power * Math.Abs(positionPercentage - center));
            }

            public List<float> distribute(List<float> peaks)
            {
                List<float> distributedPeaks = new List<float>();
                for (int i = 0; i < peaks.Count; i++)
                {
                    if (i < leftMargin || i > peaks.Count - rightMargin)
                    {
                        distributedPeaks.Add(0f);
                    }
                    else
                    {
                        distributedPeaks.Add(distributionFunction(peaks.ElementAt(i),
                                ((float)i / peaks.Count)
                                )
                                );
                    }
                }
                return distributedPeaks;
            }
        }

        public List<Peak> peaks = null;
        public List<float> yValues = new List<float>();
        // statistical information
        private bool actualAverageValue = false;
        private bool actualMaximumValue = false;
        private bool actualMinimumValue = false; 
        private float averageValue;
        private float maximumValue;
        private float minimumValue;

        public void deActualizeFlags()
        {
            actualAverageValue = false;
            actualMaximumValue = false;
            actualMinimumValue = false;
        }

        public bool allowedInterval(List<Peak> peaks, int xPosition)
        {
            foreach (Peak peak in peaks)
                if (peak.left <= xPosition && xPosition <= peak.right) return false;
            return true;
        }
        public void addPeak(float value)
        {
            yValues.Add(value);
            deActualizeFlags();
        }
        public void applyProbabilityDistributor(Graph.ProbabilityDistributor probability)
        {
            yValues = probability.distribute(yValues);
            deActualizeFlags();
        }
        public void negate()
        {
            float max = GetMaxValue();
            for (int i = 0; i < yValues.Count; i++)
                yValues[i] = max - yValues.ElementAt(i);

            deActualizeFlags();
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
            for (int i = a; i < b; i++) sum += yValues.ElementAt(i);
            return sum / yValues.Count;
        }

        public float GetMaxValue()
        {
            if (!actualMaximumValue)
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
                maxValue = Math.Max(maxValue, yValues.ElementAt(i));
            return maxValue;
        }

        public float GetMaxValue(float a, float b)
        {
            int ia = (int)(a * yValues.Count);
            int ib = (int)(b * yValues.Count);
            return GetMaxValue(ia, ib);
        }

        public int GetMaxValueIndex(int a, int b)
        {
            float maxValue = 0.0f;
            int maxIndex = a;
            for (int i = a; i < b; i++)
            {
                if (yValues.ElementAt(i) >= maxValue)
                {
                    maxValue = yValues.ElementAt(i);
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
                minValue = Math.Min(minValue, yValues.ElementAt(i));
            return minValue;
        }

        public float GetMinValue(float a, float b)
        {
            int ia = (int)(a * yValues.Count);
            int ib = (int)(b * yValues.Count);
            return GetMinValue(ia, ib);
        }

        public int GetMinValueIndex(int a, int b)
        {
            float minValue = float.PositiveInfinity;
            int minIndex = b;
            for (int i = a; i < b; i++)
            {
                if (yValues.ElementAt(i) <= minValue)
                {
                    minValue = yValues.ElementAt(i);
                    minIndex = i;
                }
            }
            return minIndex;
        }  

        public Bitmap renderHorizontally(int width, int height)
        {
            Bitmap content = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Bitmap axis = new Bitmap(width + 40, height + 40, PixelFormat.Format8bppIndexed);

            Graphics graphicContent = Graphics.FromImage(content);
            Graphics graphicAxis = Graphics.FromImage(axis);

            Rectangle backRect = new Rectangle(0, 0, width + 40, height + 40);
            SolidBrush axisBrush = new SolidBrush(Color.LightGray);
            Pen axisPen = new Pen(Color.LightGray);
            graphicAxis.FillRectangle(axisBrush, backRect);
            graphicAxis.DrawRectangle(axisPen, backRect);
            backRect = new Rectangle(0, 0, width, height);
            SolidBrush contentBrush = new SolidBrush(Color.White);
            Pen contentPen = new Pen(Color.White);
            graphicContent.FillRectangle(contentBrush, backRect);
            graphicContent.DrawRectangle(contentPen, backRect);


            int x, y, x0, y0;
            x = 0; y = 0;

            Pen graphicContentPen = new Pen(Color.Green);

            for (int i = 0; i < yValues.Count; i++)
            {
                x0 = x; y0 = y;
                x = (int)(((float)i / yValues.Count) * width);
                y = (int)(((float)1 - (yValues.ElementAt(i) / GetMaxValue())) * height);
                graphicContent.DrawLine(graphicContentPen, x0, y0, x, y);
            }

            Font graphicContentFont = new Font("Consolas", 20F);
            if (peaks != null)
            { 
                graphicContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double)width / yValues.Count;
                foreach (Peak p in peaks)
                {
                    graphicContent.DrawLine(graphicContentPen, (int)(p.left * multConst), 0, (int)(p.center * multConst), 30);
                    graphicContent.DrawLine(graphicContentPen, (int)(p.center * multConst), 30, (int)(p.right * multConst), 0);
                    graphicContent.DrawString((i++) + ".", graphicContentFont, contentBrush, (float)(p.center * multConst) - 5, 42F);
                }
            }

            graphicAxis.DrawImage(content, 35, 5);

            axisPen.Color = (Color.Black);
            axisBrush.Color = Color.Black;
            Font graphicAxisFont = new Font("Consolas", 20F);
            graphicAxis.DrawRectangle(axisPen, 35, 5, content.Width, content.Height);

            for (int ax = 0; ax < content.Width; ax += 50)
            {
                graphicAxis.DrawString(ax.ToString(), graphicAxisFont, axisBrush, ax + 35, axis.Height - 10);
                graphicAxis.DrawLine(axisPen, ax + 35, content.Height + 5, ax + 35, content.Height + 15);
            }

            for (int ay = 0; ay < content.Height; ay += 20)
            {
                graphicAxis.DrawString(Convert.ToInt32(((1 - (float)ay) / content.Height) * 100).ToString() + "%", graphicContentFont, contentBrush, 1, ay + 15);
                graphicAxis.DrawLine(axisPen, 25, ay + 5, 35, ay + 5);
            }
            graphicContent.Dispose();
            graphicAxis.Dispose();
            return axis;
        }

        public Bitmap renderVertically(int width, int height)
        {
            Bitmap content = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Bitmap axis = new Bitmap(width + 10, height + 40, PixelFormat.Format8bppIndexed);

            Graphics graphicContent = Graphics.FromImage(content);
            Graphics graphicAxis = Graphics.FromImage(axis);

            Rectangle backRect = new Rectangle(0, 0, width + 40, height + 40);
            SolidBrush axisBrush = new SolidBrush(Color.LightGray);
            Pen axisPen = new Pen(Color.LightGray);
            graphicAxis.FillRectangle(axisBrush, backRect);
            graphicAxis.DrawRectangle(axisPen, backRect);
            backRect = new Rectangle(0, 0, width, height);
            SolidBrush contentBrush = new SolidBrush(Color.White);
            Pen contentPen = new Pen(Color.White);
            graphicContent.FillRectangle(contentBrush, backRect);
            graphicContent.DrawRectangle(contentPen, backRect);


            int x, y, x0, y0;
            x = 0; y = 0;

            Pen graphicContentPen = new Pen(Color.Green);

            for (int i = 0; i < yValues.Count; i++)
            {
                x0 = x; y0 = y;
                x = (int)(((float)i / yValues.Count) * height);
                y = (int)(((float)1 - (yValues.ElementAt(i) / GetMaxValue())) * width);
                graphicContent.DrawLine(graphicContentPen, x0, y0, x, y);
            }

            Font graphicContentFont = new Font("Consolas", 20F);
            if (peaks != null)
            { 
                graphicContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double)height / yValues.Count;
                foreach (Peak p in peaks)
                {
                    graphicContent.DrawLine(graphicContentPen, width, (int)(p.left * multConst), width - 30, (int)(p.center * multConst));
                    graphicContent.DrawLine(graphicContentPen, width - 30, (int)(p.center * multConst), width, (int)(p.right * multConst));
                    graphicContent.DrawString((i++) + ".", graphicContentFont, contentBrush, width - 38, (float)(p.center * multConst) + 5);
                }
            }

            graphicAxis.DrawImage(content, 5, 5);

            axisPen.Color = (Color.Black);
            axisBrush.Color = Color.Black;
            Font graphicAxisFont = new Font("Consolas", 20F);
            graphicAxis.DrawRectangle(axisPen, 5, 5, content.Width, content.Height);
            graphicContent.Dispose();
            graphicAxis.Dispose();
            return axis;
        }


        public void rankFilter(int size)
        {
            int halfSize = size / 2;
            List<float> clone = new List<float>(yValues);

            for (int i = halfSize; i < yValues.Count - halfSize; i++)
            {
                float sum = 0;
                for (int ii = i - halfSize; ii < i + halfSize; ii++)
                {
                    sum += clone.ElementAt(ii);
                }
                yValues[i] = (sum / size);
            }

        }

        public int indexOfLeftPeakRel(int peak, double peakFootConstantRel)
        {
            int index = peak;
            for (int i = peak; i >= 0; i--)
            {
                index = i;
                if (yValues.ElementAt(index) < peakFootConstantRel * yValues.ElementAt(peak)) break;
            }
            return Math.Max(0, index);
        }
        public int indexOfRightPeakRel(int peak, double peakFootConstantRel)
        {
            int index = peak;
            for (int i = peak; i < yValues.Count; i++)
            {
                index = i;
                if (yValues.ElementAt(index) < peakFootConstantRel * yValues.ElementAt(peak)) break;
            }
            return Math.Min(yValues.Count, index);
        }


        public float averagePeakDiff(List<Peak> peaks)
        { 
            float sum = 0;
            foreach (Peak p in peaks)
                sum += p.GetDiff();
            return sum / peaks.Count;
        }
        public float maximumPeakDiff(List<Peak> peaks, int from, int to)
        {
            float max = 0;
            for (int i = from; i <= to; i++)
                max = Math.Max(max, peaks.ElementAt(i).GetDiff());
            return max;
        }

    }
}
