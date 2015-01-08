using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
{
    public class HoughTransformation
    {
        public static int RENDER_ALL = 1;
        public static int RENDER_TRANSFORMONLY = 0;
        public static int COLOR_BW = 0;
        public static int COLOR_HUE = 1;

        float[, ] bitmap;
        Point maxPoint;
        private int width;
        private int height;

        public float angle = 0;
        public float dx = 0;
        public float dy = 0;

        public HoughTransformation(int width, int height)
        {
            this.maxPoint = Point.Empty;
        this.bitmap = new float[width,height];
        this.width = width;
        this.height = height;
        for (int x=0; x<this.width; x++)
            for (int y=0; y<this.height; y++)
                this.bitmap[x, y] = 0;
    }

        public void addLine(int x, int y, float brightness)
        {
            // posunieme suradnicovu sustavu do stredu : -1 .. 1, -1 .. 1
            float xf = 2 * ((float)x) / this.width - 1;
            float yf = 2 * ((float)y) / this.height - 1;
            // y=ax + b
            // b = y - ax

            for (int a = 0; a < this.width; a++)
            {
                // posunieme a do stredu
                float af = 2 * ((float)a) / this.width - 1;
                // vypocitame b
                float bf = yf - af * xf;
                // b posumieme do povodneho suradnicoveho systemu
                int b = (int)((bf + 1) * this.height / 2);

                if (0 < b && b < this.height - 1)
                {
                    bitmap[a, b] += brightness;
                }
            }
        }

        private float getMaxValue()
        {
            float maxValue = 0;
            for (int x = 0; x < this.width; x++)
                for (int y = 0; y < this.height; y++)
                    maxValue = Math.Max(maxValue, this.bitmap[x, y]);
            return maxValue;
        }

        private Point computeMaxPoint()
        {
            float max = 0;
            int maxX = 0, maxY = 0;
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    float curr = this.bitmap[x, y];
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
        public Point getMaxPoint()
        {
            if (this.maxPoint == null) this.maxPoint = this.computeMaxPoint();
            return this.maxPoint;
        }

        private float getAverageValue()
        {
            float sum = 0;
            for (int x = 0; x < this.width; x++)
                for (int y = 0; y < this.height; y++)
                    sum += this.bitmap[x, y];
            return sum / (this.width * this.height);
        }

        public Bitmap render(int renderType, int colorType)
        {

            float average = this.getAverageValue();
            Bitmap output = new Bitmap(this.width, this.height, PixelFormat.Format8bppIndexed);
            Graphics g = Graphics.FromImage(output);

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    int value = (int)(255 * this.bitmap[x, y] / average / 3);
                    //int value = (int)Math.log(this.bitmap[x, y]*1000);
                    value = Math.Max(0, Math.Min(value, 255));
                    if (colorType == HoughTransformation.COLOR_BW)
                    {
                        output.SetPixel(x, y, Color.FromArgb(value, value, value));
                    }
                    else
                    {
                        output.SetPixel(x, y, HSBtoRGB(0.67f - ((float)value / 255) * 2 / 3, 1.0f, 1.0f));
                    }
                }
            }
            this.maxPoint = computeMaxPoint();
            SolidBrush gBrush = new SolidBrush(Color.Orange);
            Pen gPen = new Pen(Color.Orange);


            float a = 2 * ((float)this.maxPoint.X) / this.width - 1;
            float b = 2 * ((float)this.maxPoint.Y) / this.height - 1;
            //int b = this.MaxPoint.Y;
            float x0f = -1;
            float y0f = a * x0f + b;
            float x1f = 1;
            float y1f = a * x1f + b;


            int y0 = (int)((y0f + 1) * this.height / 2);
            int y1 = (int)((y1f + 1) * this.height / 2);

            int dx = this.width;
            int dy = y1 - y0;
            this.dx = dx;
            this.dy = dy;
            this.angle = (float)(180 * Math.Atan(this.dy / this.dx) / Math.PI);

            if (renderType == HoughTransformation.RENDER_ALL)
            {
                g.DrawEllipse(gPen, this.maxPoint.X - 5, this.maxPoint.Y - 5, 10, 10);
                g.DrawLine(gPen, 0, this.height / 2 - (int)dy / 2 - 1, this.width, this.height / 2 + (int)dy / 2 - 1);
                g.DrawLine(gPen, 0, this.height / 2 - (int)dy / 2 + 0, this.width, this.height / 2 + (int)dy / 2 + 0);
                g.DrawLine(gPen, 0, this.height / 2 - (int)dy / 2 + 1, this.width, this.height / 2 + (int)dy / 2 + 1);
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
                // The color wheel consists of 6 sectors. 
                // Figure out which sector you're in.
                //
                double sectorPos = hue / 60.0;
                int sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector
                double fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color. 
                double p = brightness * (1.0 - saturation);
                double q = brightness * (1.0 - (saturation * fractionalSector));
                double t = brightness * (1.0 - (saturation * (1 - fractionalSector)));

                // assign the fractional colors to r, g, and b 
                // based on the sector the angle is in.
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
