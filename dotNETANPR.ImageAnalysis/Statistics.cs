using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Statistics
    {
        public float maximum;
        public float minimum;
        public float average;
        public float dispersion;

        
        public Statistics(Bitmap photo)
        {
            float sum = 0;
            float sum2 = 0;
            int w = photo.Width;
            int h = photo.Height;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float pixelValue = photo.GetPixel(x, y).GetBrightness();
                    this.maximum = Math.Max(pixelValue, this.maximum);
                    this.minimum = Math.Min(pixelValue, this.minimum);
                    sum += pixelValue;
                    sum2 += (pixelValue * pixelValue);
                }
            }
            int count = (w * h);
            this.average = sum / count;
            // rozptyl = priemer stvorcov + stvorec priemeru
            this.dispersion = (sum2 / count) - (this.average * this.average);
        }

        public float thresholdBrightness(float value, float coef)
        {
        float outt;
        if (value > this.average)
            outt = coef + (1 - coef) * (value - this.average) / (this.maximum - this.average);
        else
            outt =  (1 - coef) * (value - this.minimum) / (this.average - this.minimum);
        return outt;
    }
    }
}
