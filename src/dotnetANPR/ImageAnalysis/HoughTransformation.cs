using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotnetANPR.ImageAnalysis
{
    public class HoughTransformation
    {
        public static int RenderAll = 1;
        public static int RenderTransformOnly = 0;
        public static int ColorBw = 0;
        public static int ColorHue = 1;

        private float[,] bitmap;
        private Point maxPoint;
        private int width;
        private int height;

        public float Angle;
        public float Dx;
        public float Dy;

        public HoughTransformation(int width, int height)
        {
            this.width = width;
            this.height = height;
            bitmap = new float[width,height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    bitmap[x, y] = 0;
                }
            }
        }

        public void AddLine(int x, int y, float brightness)
        {
            var xf = 2 * (float) x / width - 1;
            var yf = 2 * (float) y / height - 1;

            for (var a = 0; a < width; a++)
            {
                var af = 2 * (float) a / width - 1;
                var bf = yf - af * xf;

                var b = (int) ((bf + 1) * height / 2);
                if (0 < b && b < height - 1)
                {
                    bitmap[a, b] += brightness;
                }
            }
        }

        private float GetMaxValue()
        {
            float maxValue = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    maxValue = Math.Max(maxValue, bitmap[x, y]);
                }
            }
            return maxValue;
        }

        private Point ComputeMaxPoint()
        {
            float max = 0;
            int maxX = 0, maxY = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var curr = bitmap[x, y];
                    if (curr >= max)
                    {
                        maxX = x;
                        maxY = y;
                        max = curr;
                    }
                }
            }
            return new Point(maxX, maxY);
        }

        private Point GetMaxPoint()
        {
            if (maxPoint == null)
            {
                maxPoint = ComputeMaxPoint();
            }
            return maxPoint;
        }

        private float GetAverageValue()
        {
            float sum = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    sum += bitmap[x, y];
                }
            }
            return sum / (width * height);
        }

        public Bitmap Render(int renderType, int colorType)
        {
            var average = GetAverageValue();
            var output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var g = Graphics.FromImage(output);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var value = (int)(255 * bitmap[x, y] / average / 3);
                    value = Math.Max(0, Math.Min(value, 255));
                    if (colorType == ColorBw)
                    {
                        output.SetPixel(x, y, Color.FromArgb(value, value, value));
                    }
                    else
                    {
                        output.SetPixel(x, y, HSBtoRGB(0.67f - (float)value / 255 * 2 / 3, 1.0f, 1.0f));
                    }
                }
            }
            maxPoint = ComputeMaxPoint();
            var gBrush = new SolidBrush(Color.Orange);
            var gPen = new Pen(Color.Orange);


            var a = 2 * (float)maxPoint.X / width - 1;
            var b = 2 * (float)maxPoint.Y / height - 1;
            float x0f = -1;
            var y0f = a * x0f + b;
            float x1f = 1;
            var y1f = a * x1f + b;


            var y0 = (int)((y0f + 1) * height / 2);
            var y1 = (int)((y1f + 1) * height / 2);

            var dx = width;
            var dy = y1 - y0;
            this.Dx = dx;
            this.Dy = dy;
            Angle = (float)(180 * Math.Atan(this.Dy / this.Dx) / Math.PI);

            if (renderType == RenderAll)
            {
                g.DrawEllipse(gPen, maxPoint.X - 5, maxPoint.Y - 5, 10, 10);
                g.DrawLine(gPen, 0, height / 2 - dy / 2 - 1, width, height / 2 + dy / 2 - 1);
                g.DrawLine(gPen, 0, height / 2 - dy / 2 + 0, width, height / 2 + dy / 2 + 0);
                g.DrawLine(gPen, 0, height / 2 - dy / 2 + 1, width, height / 2 + dy / 2 + 1);
            }

            return output;
        }

        //Code from CodeProject "Manipulating colors in .NET" by Guillaume Leparmentier
        public static Color HSBtoRGB(double hue, double saturation, double brightness)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (saturation == 0)
            {
                r = g = b = brightness;
            }
            else
            {
                var sectorPos = hue / 60.0;
                var sectorNumber = (int)Math.Floor(sectorPos);

                var fractionalSector = sectorPos - sectorNumber;

                var p = brightness * (1.0 - saturation);
                var q = brightness * (1.0 - saturation * fractionalSector);
                var t = brightness * (1.0 - saturation * (1 - fractionalSector));

                switch (sectorNumber)
                {
                    case 0:
                        r = brightness;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = brightness;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = brightness;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = brightness;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = brightness;
                        break;
                    case 5:
                        r = brightness;
                        g = p;
                        b = q;
                        break;
                }
            }

            return Color.FromArgb(
                (int)(r * 255.0 + 0.5),
                (int)(g * 255.0 + 0.5),
                (int)(b * 255.0 + 0.5));
        }
    }
}
