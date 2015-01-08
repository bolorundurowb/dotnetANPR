using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using dotNETANPR.Configurator;
using System.IO;
using System.Drawing.Drawing2D;

namespace dotNETANPR.ImageAnalysis
{
    public class Photo
    {
        public Bitmap image;

        public Photo()
        {
            this.image = null;
        }
        public Photo(Bitmap bi)
        {
            this.image = bi;
        }
        public Photo(String filepath)
        {
            this.loadImage(filepath);
        }

        public Photo clone()
        {
            return new Photo(duplicateBitmap(this.image));
        }

        public int getWidth()
        {
            return this.image.Width;
        }
        public int getHeight()
        {
            return this.image.Height;
        }
        public int getSquare()
        {
            return this.getWidth() * this.getHeight();
        }

        public Bitmap getBi()
        {
            return this.image;
        }
        public Bitmap getBiWithAxes()
        {
            Bitmap axis = new Bitmap(this.image.Width + 40,
                    this.image.Height + 40, PixelFormat.Format8bppIndexed);
            Graphics graphicAxis = Graphics.FromImage(axis);

            SolidBrush axisBrush = new SolidBrush(Color.LightGray);
            Pen axisPen = new Pen(Color.LightGray);
            Rectangle backRect = new Rectangle(0, 0, this.image.Width + 40, this.image.Height + 40);
            graphicAxis.FillRectangle(axisBrush, backRect);
            graphicAxis.DrawRectangle(axisPen, backRect);

            graphicAxis.DrawImage(this.image, 35, 5);

            axisBrush.Color = (Color.Black);
            axisPen.Color = (Color.Black);
            graphicAxis.DrawRectangle(axisPen, 35, 5, this.image.Width, this.image.Height);
            Font graphicAxisFont = new Font("Consolas", 20F);
            for (int ax = 0; ax < this.image.Width; ax += 50)
            {
                graphicAxis.DrawString(ax.ToString(), graphicAxisFont, axisBrush, ax + 35, axis.Height - 10);
                graphicAxis.DrawLine(axisPen, ax + 35, this.image.Height + 5, ax + 35, this.image.Height + 15);
            }
            for (int ay = 0; ay < this.image.Height; ay += 50)
            {

                graphicAxis.DrawString(ay.ToString(), graphicAxisFont, axisBrush, 3, ay + 15);
                graphicAxis.DrawLine(axisPen, 25, ay + 5, 35, ay + 5);
            }
            graphicAxis.Dispose();
            return axis;
        }

        public void setBrightness(int x, int y, float value)
        {
            image.SetPixel(x, y, Color.FromArgb((int)value, (int)value, (int)value));
        }
        static public void setBrightness(Bitmap image, int x, int y, float value)
        {
            image.SetPixel(x, y, Color.FromArgb((int)value, (int)value, (int)value));
        }

        static public float getBrightness(Bitmap image, int x, int y)
        {
            int r = image.getRaster().getSample(x, y, 0);
            int g = image.getRaster().getSample(x, y, 1);
            int b = image.getRaster().getSample(x, y, 2);
            float[] hsb = new float[] { Color.FromArgb(r, g, b).GetHue(), Color.FromArgb(r, g, b).GetSaturation(), Color.FromArgb(r, g, b).GetBrightness() };
            return hsb[2];
        }
        static public float getSaturation(Bitmap image, int x, int y)
        {
            int r = image.getRaster().getSample(x, y, 0);
            int g = image.getRaster().getSample(x, y, 1);
            int b = image.getRaster().getSample(x, y, 2);

            float[] hsb = new float[] { Color.FromArgb(r, g, b).GetHue(), Color.FromArgb(r, g, b).GetSaturation(), Color.FromArgb(r, g, b).GetBrightness() };
            return hsb[1];
        }
        static public float getHue(Bitmap image, int x, int y)
        {
            int r = image.getRaster().getSample(x, y, 0);
            int g = image.getRaster().getSample(x, y, 1);
            int b = image.getRaster().getSample(x, y, 2);

            float[] hsb = new float[] { Color.FromArgb(r, g, b).GetHue(), Color.FromArgb(r, g, b).GetSaturation(), Color.FromArgb(r, g, b).GetBrightness() };
            return hsb[0];
        }

        public float getBrightness(int x, int y)
        {
            return getBrightness(image, x, y);
        }
        public float getSaturation(int x, int y)
        {
            return getSaturation(image, x, y);
        }
        public float getHue(int x, int y)
        {
            return getHue(image, x, y);
        }

        public void loadImage(String filepath)
        {
            try
            {
                Bitmap image = new  Bitmap(filepath);
                Bitmap outimage = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);
                Graphics g = Graphics.FromImage(outimage);
                g.DrawImage(image, 0, 0);
                g.Dispose();
                this.image = outimage;
            }
            catch (IOException ex)
            {
                throw new IOException("{Error in image loader} Couldn't read input file " + filepath);
            }
        }
        public void saveImage(String filepath)
        {
            string type = filepath.Substring(filepath.LastIndexOf('.') + 1, filepath.Length).ToUpper();
            if (!type.Equals("BMP") &&
                    !type.Equals("JPG") &&
                    !type.Equals("JPEG") &&
                    !type.Equals("PNG")
                    ) throw new IOException("Unsupported file format");
            this.image.Save(filepath, new ImageFormat(new Guid(type)));
        }

        public void normalizeBrightness(float coef)
        {
            Statistics stats = new Statistics(this.image);
            for (int x = 0; x < this.getWidth(); x++)
            {
                for (int y = 0; y < this.getHeight(); y++)
                {
                    setBrightness(this.image, x, y,
                            stats.thresholdBrightness(getBrightness(this.image, x, y), coef)
                            );
                }
            }
        }

        // FILTERS
        public void linearResize(int width, int height)
        {
            this.image = linearResizeBi(this.image, width, height);
        }
        static public Bitmap linearResizeBi(Bitmap origin, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Graphics g = Graphics.FromImage(resizedImage);
            float xScale = (float)width / origin.Width;
            float yScale = (float)height / origin.Height;
            Matrix at = new Matrix(xScale, 0, 0, yScale, 0, 0);
            g.Transform = at;
            g.DrawImage(origin, 0, 0);
            g.Dispose();
            return resizedImage;
        }
        public void averageResize(int width, int height)
        {
            this.image = averageResizeBi(this.image, width, height);
        }
        // TODO : nefunguje dobre pre znaky podobnej velkosti ako cielvoa velkost
        public Bitmap averageResizeBi(Bitmap origin, int width, int height)
        {

            if (origin.Width < width || origin.Height < height)
                return linearResizeBi(origin, width, height); // average height sa nehodi
            // na zvacsovanie, preto ak zvacsujeme v smere x alebo y, pouzijeme
            // radsej linearnu transformaciu

            /* java api standardne zmensuje obrazky bilinearnou metodou, resp. linear mapping.
             * co so sebou prinasa dost velku stratu informacie. Idealna by bola fourierova
             * transformacia, ale ta neprichadza do uvahy z dovodu velkej cesovej narocnosti
             * preto sa ako optimalna javi metoda WEIGHTED AVERAGE
             */
            Bitmap resized = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            float xScale = (float)origin.Width / width;
            float yScale = (float)origin.Height / height;

            for (int x = 0; x < width; x++)
            {
                int x0min = (int)Math.Round(x * xScale);
                int x0max = (int)Math.Round((x + 1) * xScale);

                for (int y = 0; y < height; y++)
                {
                    int y0min = (int)Math.Round(y * yScale);
                    int y0max = (int)Math.Round((y + 1) * yScale);

                    // spravit priemer okolia a ulozit do resizedImage;
                    float sum = 0;
                    int sumCount = 0;

                    for (int x0 = x0min; x0 < x0max; x0++)
                    {
                        for (int y0 = y0min; y0 < y0max; y0++)
                        {
                            sum += getBrightness(origin, x0, y0);
                            sumCount++;
                        }
                    }
                    sum /= sumCount;
                    setBrightness(resized, x, y, sum);
                    //
                }
            }
            return resized;
        }

        public Photo duplicate()
        {
            return new Photo(duplicateBitmap(this.image));
        }
        static public Bitmap duplicateBitmap(Bitmap image)
        {
            Bitmap imageCopy = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);
            imageCopy.setData(image.getData());
            return imageCopy;
        }

        public static void thresholding(Bitmap bi)
        { // TODO: optimalizovat
            short[] threshold = new short[256];
            for (short i = 0; i < 36; i++) threshold[i] = 0;
            for (short i = 36; i < 256; i++) threshold[i] = i;
            BitmapOp thresholdOp = new LookupOp(new ShortLookupTable(0, threshold), null);
            thresholdOp.filter(bi, bi);
        }

        public void verticalEdgeDetector(Bitmap source)
        {
            Bitmap destination = duplicateBitmap(source);

            float[] data1 = new float[] {
            -1,0,1,
            -2,0,2,
            -1,0,1,
        };

            float[] data2 = {
            1,0,-1,
            2,0,-2,
            1,0,-1,
        };

            new ConvolveOp(new Kernel(3, 3, data1), ConvolveOp.EDGE_NO_OP, null).filter(destination, source);
        }


        public float[,] BitmapToArray(Bitmap image, int w, int h)
        {
            float[,] array = new float[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    array[x, y] = Photo.getBrightness(image, x, y);
                }
            }
            return array;
        }

        public float[,] BitmapToArrayWithBounds(Bitmap image, int w, int h)
        {
            float[,] array = new float[w + 2, h + 2];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    array[x + 1, y + 1] = Photo.getBrightness(image, x, y);
                }
            }
            // vynulovat hrany :
            for (int x = 0; x < w + 2; x++)
            {
                array[x, 0] = 1;
                array[x, h + 1] = 1;
            }
            for (int y = 0; y < h + 2; y++)
            {
                array[0, y] = 1;
                array[w + 1, y] = 1;
            }
            return array;
        }

        static public Bitmap arrayToBitmap(float[,] array, int w, int h)
        {
            Bitmap bi = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Photo.setBrightness(bi, x, y, array[x, y]);
                }
            }
            return bi;
        }


        static public Bitmap createBlankBi(Bitmap image)
        {
            Bitmap imageCopy = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);
            return imageCopy;
        }
        public Bitmap createBlankBi(int width, int height)
        {
            Bitmap imageCopy = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            return imageCopy;
        }

        public Bitmap sumBi(Bitmap bi1, Bitmap bi2)
        { //used by edgeDetectors
            Bitmap outt = new Bitmap(Math.Min(bi1.Width, bi2.Width),
                    Math.Min(bi1.Height, bi2.Height),
                    PixelFormat.Format8bppIndexed);

            for (int x = 0; x < outt.Width; x++)
                for (int y = 0; y < outt.Height; y++)
                {
                    setBrightness(outt, x, y, (float)Math.Min(1.0, getBrightness(bi1, x, y) + getBrightness(bi2, x, y)));
                }
            return outt;
        }

        public void plainThresholding(Statistics stat)
        {
            int w = this.getWidth();
            int h = this.getHeight();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    this.setBrightness(x, y, stat.thresholdBrightness(this.getBrightness(x, y), 1.0f));
                }
            }
        }

        /**ADAPTIVE THRESHOLDING CEZ GETNEIGHBORHOOD - deprecated*/
        public void adaptiveThresholding()
        { // jedine pouzitie tejto funkcie by malo byt v konstruktore znacky 
            Statistics stat = new Statistics(this.image);
            Configurator.Configurator config = new Configurator.Configurator();
            int radius = config.getIntProperty("photo_adaptivethresholdingradius");
            if (radius == 0)
            {
                plainThresholding(stat);
                return;
            }

            ///
            int w = this.getWidth();
            int h = this.getHeight();

            float[,] sourceArray = this.BitmapToArray(this.image, w, h);
            float[,] destinationArray = this.BitmapToArray(this.image, w, h);

            int count;
            float neighborhood;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    // compute neighborhood
                    count = 0;
                    neighborhood = 0;
                    for (int ix = x - radius; ix <= x + radius; ix++)
                    {
                        for (int iy = y - radius; iy <= y + radius; iy++)
                        {
                            if (ix >= 0 && iy >= 0 && ix < w && iy < h)
                            {
                                neighborhood += sourceArray[ix, iy];
                                count++;
                            }
                            /********/
                            //                        else {
                            //                            neighborhood += stat.average;
                            //                            count++;
                            //                        }
                            /********/
                        }
                    }
                    neighborhood /= count;
                    //
                    if (destinationArray[x, y] < neighborhood)
                    {
                        destinationArray[x, y] = 0f;
                    }
                    else
                    {
                        destinationArray[x, y] = 1f;
                    }
                }
            }
            this.image = arrayToBitmap(destinationArray, w, h);
        }

        public HoughTransformation getHoughTransformation()
        {
            HoughTransformation hough = new HoughTransformation(this.getWidth(), this.getHeight());
            for (int x = 0; x < this.getWidth(); x++)
            {
                for (int y = 0; y < this.getHeight(); y++)
                {
                    hough.addLine(x, y, this.getBrightness(x, y));
                }
            }
            return hough;
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
