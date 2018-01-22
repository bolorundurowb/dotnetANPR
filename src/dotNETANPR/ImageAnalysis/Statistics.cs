using System;
using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Statistics
    {
        public float Maximum { get; set; }
        public float Minimum { get; set; }
        public float Average { get; set; }
        public float Dispersion { get; set; }

        public Statistics(Bitmap photo)
        {
            float sum = 0;
            float squaresSum = 0;
            var w = photo.Width;
            var h = photo.Height;

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var pixelValue = photo.GetPixel(x, y).GetBrightness();
                    Maximum = Math.Max(pixelValue, Maximum);
                    Minimum = Math.Min(pixelValue, Minimum);
                    sum += pixelValue;
                    squaresSum += pixelValue * pixelValue;
                }
            }
            var count = w * h;
            Average = sum / count;
            Dispersion = squaresSum / count - Average * Average;
        }

        public float ThresholdBrightness(float value, float coef)
        {
            float output;
            if (value > Average)
                output = coef + (1 - coef) * (value - Average) / (Maximum - Average);
            else
                output = (1 - coef) * (value - Minimum) / (Average - Minimum);
            // HACK: to prevent NaN being returned
            if (float.IsNaN(output))
            {
                return 0f;
            }
            return output;
        }
    }
}
