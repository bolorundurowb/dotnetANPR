using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
//using System.

namespace dotNETANPR.ImageAnalysis
{
    public class Graph
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
                this.center = (left + right) / 2;
                this.right = right;
            }
            public int getLeft()
            {
                return this.left;
            }
            public int getRight()
            {
                return this.right;
            }
            public int getCenter()
            {
                return this.center;
            }
            public int getDiff()
            {
                return this.right - this.left;
            }
            public void setLeft(int left)
            {
                this.left = left;
            }
            public void setCenter(int center)
            {
                this.center = center;
            }
            public void setRight(int right)
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
                this.leftMargin = Math.Max(1, leftMargin);
                this.rightMargin = Math.Max(1, rightMargin);
            }

            private float distributionFunction(float value, float positionPercentage)
            {
                return value * (1 - this.power * Math.Abs(positionPercentage - this.center));
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
        // statistical informations
        private bool actualAverageValue = false; // su hodnoty aktualne ?
        private bool actualMaximumValue = false; // su hodnoty aktualne ?
        private bool actualMinimumValue = false; // su hodnoty aktualne ?
        private float averageValue;
        private float maximumValue;
        private float minimumValue;

        public void deActualizeFlags()
        {
            this.actualAverageValue = false;
            this.actualMaximumValue = false;
            this.actualMinimumValue = false;
        }

        // generic
        // methods for searching bands in image !
        public bool allowedInterval(List<Peak> peaks, int xPosition)
        {
            foreach (Peak peak in peaks)
                if (peak.left <= xPosition && xPosition <= peak.right) return false;
            return true;
        }
        public void addPeak(float value)
        {
            yValues.Add(value);
            this.deActualizeFlags();
        }
        public void applyProbabilityDistributor(Graph.ProbabilityDistributor probability)
        {
            this.yValues = probability.distribute(this.yValues);
            this.deActualizeFlags();
        }
        public void negate()
        {
            float max = this.getMaxValue();
            for (int i = 0; i < this.yValues.Count; i++)
                this.yValues[i] = max - this.yValues.ElementAt(i);

            this.deActualizeFlags();
        }

        //    public public class PeakComparer implements Comparator {
        //        int sortBy; // 0 = podla sirky, 1 = podla velkosti, 2 = z lava do prava
        //        List<float> yValues = null;
        //        
        //        public PeakComparer(List<float> yValues, int sortBy) {
        //            this.yValues = yValues;
        //            this.sortBy = sortBy;
        //        }
        //        
        //        private float getPeakValue(Object peak) {
        //            if (this.sortBy == 0) {
        //                return ((Peak)peak).diff();
        //            } else if (this.sortBy == 1) {
        //                return this.yValues.ElementAt( ((Peak)peak).center()  );
        //            } else if (this.sortBy == 2) {
        //                return ((Peak)peak).center();
        //            }
        //            return 0;
        //        }
        //        
        //        public int compare(Object peak1, Object peak2) { // Peak
        //            double comparison = this.getPeakValue(peak2) - this.getPeakValue(peak1);
        //            if (comparison < 0) return -1;
        //            if (comparison > 0) return 1;
        //            return 0;
        //        }
        //    }

        //    float getAverageValue() {
        //        if (!this.actualAverageValue) {
        //            float sum = 0.0f;
        //            for (float peak : this.yValues) sum += peak;
        //            this.averageValue = sum/this.yValues.Count;
        //            this.actualAverageValue = true;
        //        }
        //        return this.averageValue;
        //    }
        //    

        public float getAverageValue()
        {
            if (!this.actualAverageValue)
            {
                this.averageValue = getAverageValue(0, this.yValues.Count);
                this.actualAverageValue = true;
            }
            return this.averageValue;
        }

        public float getAverageValue(int a, int b)
        {
            float sum = 0.0f;
            for (int i = a; i < b; i++) sum += this.yValues.ElementAt(i);
            return sum / this.yValues.Count;
        }


        //    float getMaxValue() {
        //        if (!this.actualMaximumValue) {
        //            float maxValue = 0.0f;
        //            for (int i=0; i<yValues.Count; i++)
        //                maxValue = Math.Max(maxValue, yValues.ElementAt(i));
        //            this.MaximumValue = maxValue;
        //            this.actualMaximumValue = true;
        //        }
        //        return this.MaximumValue;
        //    }

        public float getMaxValue()
        {
            if (!this.actualMaximumValue)
            {
                this.maximumValue = this.getMaxValue(0, this.yValues.Count);
                this.actualMaximumValue = true;
            }
            return this.maximumValue;
        }
        public float getMaxValue(int a, int b)
        {
            float maxValue = 0.0f;
            for (int i = a; i < b; i++)
                maxValue = Math.Max(maxValue, yValues.ElementAt(i));
            return maxValue;
        }
        public float getMaxValue(float a, float b)
        {
            int ia = (int)(a * yValues.Count);
            int ib = (int)(b * yValues.Count);
            return getMaxValue(ia, ib);
        }

        public int getMaxValueIndex(int a, int b)
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

        //    float getMinValue() {
        //        if (!this.actualMinimumValue) {
        //            float minValue = float.PositiveInfinity;
        //            for (int i=0; i<yValues.Count; i++)
        //                minValue = Math.min(minValue, yValues.ElementAt(i));
        //            
        //            this.minimumValue = minValue;
        //            this.actualMinimumValue = true;
        //        }
        //        return this.minimumValue;
        //    }

        public float getMinValue()
        {
            if (!this.actualMinimumValue)
            {
                this.minimumValue = this.getMinValue(0, this.yValues.Count);
                this.actualMinimumValue = true;
            }
            return this.minimumValue;
        }
        public float getMinValue(int a, int b)
        {
            float minValue = float.PositiveInfinity;
            for (int i = a; i < b; i++)
                minValue = Math.Min(minValue, yValues.ElementAt(i));
            return minValue;
        }
        public float getMinValue(float a, float b)
        {
            int ia = (int)(a * yValues.Count);
            int ib = (int)(b * yValues.Count);
            return getMinValue(ia, ib);
        }


        public int getMinValueIndex(int a, int b)
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
        //    

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

            for (int i = 0; i < this.yValues.Count; i++)
            {
                x0 = x; y0 = y;
                x = (int)(((float)i / this.yValues.Count) * width);
                y = (int)(((float)1 - (this.yValues.ElementAt(i) / this.getMaxValue())) * height);
                graphicContent.DrawLine(graphicContentPen, x0, y0, x, y);
            }

            Font graphicContentFont = new Font("Consolas", 20F);
            if (this.peaks != null)
            { // uz boli vyhladane aj peaky, renderujeme aj tie
                //graphicContent.setColor(Color.Red);
                graphicContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double)width / this.yValues.Count;
                foreach (Peak p in this.peaks)
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

            for (int i = 0; i < this.yValues.Count; i++)
            {
                x0 = x; y0 = y;
                x = (int)(((float)i / this.yValues.Count) * height);
                y = (int)(((float)1 - (this.yValues.ElementAt(i) / this.getMaxValue())) * width);
                graphicContent.DrawLine(graphicContentPen, x0, y0, x, y);
            }

            Font graphicContentFont = new Font("Consolas", 20F);
            if (this.peaks != null)
            { // uz boli vyhladane aj peaky, renderujeme aj tie
                //graphicContent.setColor(Color.Red);
                graphicContentPen.Color = Color.Red;
                contentBrush.Color = Color.Red;
                int i = 0;
                double multConst = (double)height / this.yValues.Count;
                foreach (Peak p in this.peaks)
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

            /*for (int ax = 0; ax < content.Width; ax += 50)
            {
                graphicAxis.DrawString(ax.ToString(), graphicAxisFont, axisBrush, ax + 35, axis.Height - 10);
                graphicAxis.DrawLine(axisPen, ax + 35, content.Height + 5, ax + 35, content.Height + 15);
            }

            for (int ay = 0; ay < content.Height; ay += 20)
            {
                graphicAxis.DrawString(Convert.ToInt32(((1 - (float)ay) / content.Height) * 100).ToString() + "%", graphicContentFont, contentBrush, 1, ay + 15);
                graphicAxis.DrawLine(axisPen, 25, ay + 5, 35, ay + 5);
            }*/
            graphicContent.Dispose();
            graphicAxis.Dispose();
            return axis;
        }


        public void rankFilter(int size)
        {
            int halfSize = size / 2;
            //List<float> clone = (List<float>)this.yValues.clone();
            List<float> clone = new List<float>(this.yValues);

            for (int i = halfSize; i < this.yValues.Count - halfSize; i++)
            {
                float sum = 0;
                for (int ii = i - halfSize; ii < i + halfSize; ii++)
                {
                    sum += clone.ElementAt(ii);
                }
                this.yValues[i] = (sum / size);
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
        { // not used
            float sum = 0;
            foreach (Peak p in peaks)
                sum += p.getDiff();
            return sum / peaks.Count;
        }
        public float maximumPeakDiff(List<Peak> peaks, int from, int to)
        {
            float max = 0;
            for (int i = from; i <= to; i++)
                max = Math.Max(max, peaks.ElementAt(i).getDiff());
            return max;
        }

    }
}
