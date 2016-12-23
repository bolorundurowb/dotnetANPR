using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
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

        private float angle;
        private float dx;
        private float dy;

        public HoughTransformation(int width, int height)
        {
            this.width = width;
            this.height = height;
            bitmap = new float[width,height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bitmap[x, y] = 0;
                }
            }
        }

        public void AddLine(int x, int y, float brightness)
        {
            float xf = 2 * (float) x / width - 1;
            float yf = 2 * (float) y / height - 1;

            for (int a = 0; a < width; a++)
            {
                float af = 2 * (float) a / width - 1;
                float bf = yf - af * xf;

                int b = (int) ((bf + 1) * height / 2);
                if (0 < b && b < height - 1)
                {
                    bitmap[a, b] += brightness;
                }
            }
        }

        private float GetMaxValue()
        {
            float maxValue = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
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
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float curr = bitmap[x, y];
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
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    sum += bitmap[x, y];
                }
            }
            return sum / (width * height);
        }

        public Bitmap Render(int renderType, int colorType)
        {
            float average = GetAverageValue();
            Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Graphics g = Graphics.FromImage(output);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = (int)(255 * bitmap[x, y] / average / 3);
                    value = Math.Max(0, Math.Min(value, 255));
                    if (colorType == ColorBw)
                    {
                        output.SetPixel(x, y, Color.FromArgb(value, value, value));
                    }
                    else
                    {
                        output.SetPixel(x, y, HSBtoRGB(0.67f - ((float)value / 255) * 2 / 3, 1.0f, 1.0f));
                    }
                }
            }
            maxPoint = ComputeMaxPoint();
            SolidBrush gBrush = new SolidBrush(Color.Orange);
            Pen gPen = new Pen(Color.Orange);


            float a = 2 * ((float)maxPoint.X) / width - 1;
            float b = 2 * ((float)maxPoint.Y) / height - 1;
            float x0f = -1;
            float y0f = a * x0f + b;
            float x1f = 1;
            float y1f = a * x1f + b;


            int y0 = (int)((y0f + 1) * height / 2);
            int y1 = (int)((y1f + 1) * height / 2);

            int dx = width;
            int dy = y1 - y0;
            this.dx = dx;
            this.dy = dy;
            angle = (float)(180 * Math.Atan(this.dy / this.dx) / Math.PI);

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
                double sectorPos = hue / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));

                double fractionalSector = sectorPos - sectorNumber;

                double p = brightness * (1.0 - saturation);
                double q = brightness * (1.0 - (saturation * fractionalSector));
                double t = brightness * (1.0 - (saturation * (1 - fractionalSector)));

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
