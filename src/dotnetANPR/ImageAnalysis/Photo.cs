using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using dotnetANPR.ImageAnalysis.Convolution;
using dotnetANPR.ImageAnalysis.LookUp;

namespace dotnetANPR.ImageAnalysis
{
    public class Photo
    {
        public Bitmap Image { get; set; }

        public Photo()
        {
            Image = null;
        }

        public Photo(Bitmap bitmap)
        {
            Image = bitmap;
        }

        public Photo(string filePath)
        {
            LoadImage(filePath);
        }

        public Photo Clone()
        {
            return new Photo(DuplicateBitmap(Image));
        }

        public int GetWidth()
        {
            return Image.Width;
        }

        public int GetHeight()
        {
            return Image.Height;
        }

        public int GetSquare()
        {
            return GetHeight() * GetWidth();
        }

        public Bitmap GetBitmap()
        {
            return Image;
        }

        public Bitmap GetBitmapWithAxes()
        {
            var bitmap = new Bitmap(Image.Width + 40, Image.Height + 40, PixelFormat.Format8bppIndexed);
            var graphics = Graphics.FromImage(bitmap);
            var brush = new SolidBrush(Color.LightGray);
            var pen = new Pen(Color.LightGray);
            var rectangle = new Rectangle(0, 0, Image.Width, Image.Height);

            graphics.FillRectangle(brush, rectangle);
            graphics.DrawRectangle(pen, rectangle);
            graphics.DrawImage(Image, 35, 5);
            brush.Color = Color.Black;
            pen.Color = Color.Black;
            graphics.DrawRectangle(pen, 35, 5, Image.Width, Image.Height);

            var font = new Font("Consolas", 20F);
            for (var i = 0; i < Image.Width; i += 50)
            {
                graphics.DrawString(i.ToString(), font, brush, 35 + i, bitmap.Height - 10);
                graphics.DrawLine(pen, i + 35, Image.Height + 5, i + 35, Image.Height + 15);
            }
            for (var i = 0; i < Image.Height; i++)
            {
                graphics.DrawString(i.ToString(), font, brush, 3, i + 15);
                graphics.DrawLine(pen, 25, i + 5, 35, i + 5);
            }
            graphics.Dispose();
            return bitmap;
        }

        public void SetBrightness(int x, int y, float value)
        {
            Image.SetPixel(x, y, Color.FromArgb((int) value, (int) value, (int) value));
        }

        public static void SetBrightness(Bitmap bitmap, int x, int y, float value)
        {
            bitmap.SetPixel(x, y, Color.FromArgb((int) value, (int) value, (int) value));
        }

        public static float GetBrightness(Bitmap bitmap, int x, int y)
        {
            var color = bitmap.GetPixel(x, y);
            return color.GetBrightness();
        }

        public static float GetSaturation(Bitmap bitmap, int x, int y)
        {
            var color = bitmap.GetPixel(x, y);
            return color.GetSaturation();
        }

        public static float GetHue(Bitmap bitmap, int x, int y)
        {
            var color = bitmap.GetPixel(x, y);
            return color.GetHue();
        }

        public float GetBrightness(int x, int y)
        {
            return GetBrightness(Image, x, y);
        }

        public float GetSaturation(int x, int y)
        {
            return GetSaturation(Image, x, y);
        }

        public float GetHue(int x, int y)
        {
            return GetHue(Image, x, y);
        }

        public void LoadImage(string filePath)
        {
            try
            {
                var bitmap = new Bitmap(filePath);
                var image = new Bitmap(bitmap.Width, bitmap.Height);
                var graphics = Graphics.FromImage(image);
                graphics.DrawImage(bitmap, 0, 0);
                graphics.Dispose();
                Image = image;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occurred in dotNETANPR.\nMessage:{ex.Message}");
                throw;
            }
        }

        public void SaveImage(string filePath)
        {
            var type = filePath.Substring(filePath.LastIndexOf('.') + 1, filePath.Length).ToUpper();
            if (!type.Equals("BMP") &&
                !type.Equals("JPG") &&
                !type.Equals("JPEG") &&
                !type.Equals("PNG")
            )
            {
                throw new IOException("Unsupported file format");
            }
            Image.Save(filePath, new ImageFormat(new Guid(type)));
        }

        public void NormalizeBrightness(float coef)
        {
            var stats = new Statistics(Image);
            for (var x = 0; x < GetWidth(); x++)
            {
                for (var y = 0; y < GetHeight(); y++)
                {
                    SetBrightness(Image, x, y, stats.ThresholdBrightness(GetBrightness(Image, x, y), coef));
                }
            }
        }

        public void LinearResize(int width, int height)
        {
            Image = LinearResizeBitmap(Image, width, height);
        }

        public static Bitmap LinearResizeBitmap(Bitmap bmp, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var graphics = Graphics.FromImage(bitmap);
            var xScale = (float) width / bmp.Width;
            var yScale = (float) height / bmp.Height;
            var matrix = new Matrix(xScale, 0, 0, yScale, 0, 0);
            graphics.Transform = matrix;
            graphics.DrawImage(bmp, 0, 0);
            graphics.Dispose();
            return bitmap;
        }

        public void AverageResize(int width, int height)
        {
            Image = AverageResizeBitmap(Image, width, height);
        }

        public static Bitmap AverageResizeBitmap(Bitmap bmp, int width, int height)
        {
            if (bmp.Width < width || bmp.Height < height)
            {
                return LinearResizeBitmap(bmp, width, height);
            }
            var bitmap = new Bitmap(width, height);
            var xScale = (float) width / bmp.Width;
            var yScale = (float) height / bmp.Height;
            for (var i = 0; i < width; i++)
            {
                var x0min = (int) Math.Round(i * xScale);
                var x0max = (int) Math.Round((i + 1) * xScale);
                for (var j = 0; j < height; j++)
                {
                    var y0min = (int) Math.Round(j * yScale);
                    var y0max = (int) Math.Round((j + 1) * yScale);

                    float sum = 0;
                    var sumCount = 0;

                    for (var x0 = x0min; x0 < x0max; x0++)
                    {
                        for (var y0 = y0min; y0 < y0max; y0++)
                        {
                            sum += GetBrightness(bmp, x0, y0);
                            sumCount++;
                        }
                    }
                    sum /= sumCount;
                    SetBrightness(bitmap, i, j, sum);
                }
            }
            return bitmap;
        }

        public Photo Duplicate()
        {
            return new Photo(DuplicateBitmap(Image));
        }

        public static Bitmap DuplicateBitmap(Bitmap bmp)
        {
            var bitmap = new Bitmap(bmp);
            return bitmap;
        }

        public static void Thresholding(Bitmap bitmap)
        {
            var threshold = new int[256];
            for (short i = 0; i < 36; i++)
            {
                threshold[i] = 0;
            }
            for (short i = 36; i < 256; i++)
            {
                threshold[i] = i;
            }
            var lookupOp = new LookupOp {LookupTable = threshold};
            var result = lookupOp.Filter(bitmap);
        }

        public void VerticalEdgeDetector(Bitmap source)
        {
            var destination = DuplicateBitmap(source);
            int[,] datset1 =
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            };

            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel
            {
                Size = 3,
                Matrix = datset1
            };
            destination = convolveOp.Convolve(source, kernel);
        }

        public float[,] BitmapToArray(Bitmap bitmap, int width, int height)
        {
            var array = new float[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    array[x, y] = GetBrightness(Image, x, y);
                }
            }
            return array;
        }

        public float[,] BitmapToArrayWithBounds(Bitmap bitmap, int width, int height)
        {
            var array = new float[width, height];
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    array[i + 1, j + 1] = GetBrightness(bitmap, i, j);
                }
            }
            for (var i = 0; i < width + 2; i++)
            {
                array[i, 0] = 1;
                array[i, height + 1] = 1;
            }
            for (var j = 0; j < height + 2; j++)
            {
                array[0, j] = 1;
                array[width + 1, j] = 1;
            }
            return array;
        }

        public static Bitmap ArrayToBitmap(float[,] array, int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    SetBrightness(bitmap, width, height, array[i, j]);
                }
            }
            return bitmap;
        }

        public Bitmap CreateBlankBitmap(Bitmap bitmap)
        {
            var copy = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format8bppIndexed);
            return copy;
        }

        public Bitmap CreateBlankBitmap(int width, int height)
        {
            var copy = new Bitmap(width, height);
            return copy;
        }

        public Bitmap SumBitmaps(Bitmap bitmap1, Bitmap bitmap2)
        {
            var output = new Bitmap(Math.Min(bitmap1.Width, bitmap2.Width), Math.Min(bitmap1.Height, bitmap2.Height),
                PixelFormat.Format8bppIndexed);
            for (var i = 0; i < output.Width; i++)
            {
                for (var j = 0; j < output.Height; j++)
                {
                    SetBrightness(output, i, j,
                        (float) Math.Min(1.0, GetBrightness(bitmap1, i, j) + GetBrightness(bitmap2, i, j)));
                }
            }
            return output;
        }

        public void PlainThresholding(Statistics statistics)
        {
            var width = GetWidth();
            var height = GetHeight();
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    SetBrightness(i, j, statistics.ThresholdBrightness(GetBrightness(i, j), 1.0f));
                }
            }
        }

        public void AdaptiveThresholding()
        {
            var statistics = new Statistics(Image);
            var configurator = new Configurator.Configurator();
            var radius = configurator.GetIntProperty("photo_adaptivethresholdingradius");
            if (radius == 0)
            {
                PlainThresholding(statistics);
                return;
            }
            var width = GetWidth();
            var height = GetHeight();

            var source = BitmapToArray(Image, width, height);
            var destination = BitmapToArray(Image, width, height);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var count = 0;
                    var neighbourhood = 0.0f;
                    for (var k = i - radius; k <= i + radius; k++)
                    {
                        for (var l = j - radius; l <= j + radius; l++)
                        {
                            if (k >= 0 && l >= 0 && k < width && l < height)
                            {
                                neighbourhood += source[k, l];
                                count++;
                            }
                        }
                    }
                    neighbourhood /= count;
                    if (destination[i, j] < neighbourhood)
                    {
                        destination[i, j] = 0f;
                    }
                    else
                    {
                        destination[i, j] = 1f;
                    }
                }
            }
            Image = ArrayToBitmap(destination, width, height);
        }

        public HoughTransformation GetHoughTransformation()
        {
            var houghTransformation = new HoughTransformation(GetWidth(), GetHeight());
            for (var i = 0; i < GetWidth(); i++)
            {
                for (var j = 0; j < GetHeight(); j++)
                {
                    houghTransformation.AddLine(i, j, GetBrightness(i, j));
                }
            }
            return houghTransformation;
        }

        //Code from CodeProject "Manipulating colors in .NET" by Guillaume Leparmentier
        public static Color HsbToRgb(double hue, double saturation, double brightness)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            if (saturation.Equals(0f))
            {
                r = g = b = brightness;
            }
            else
            {
                // The color wheel consists of 6 sectors.
                // Figure out which sector you're in.
                var sectorPos = hue / 60.0;
                var sectorNumber = (int)Math.Floor(sectorPos);

                // get the fractional part of the sector
                var fractionalSector = sectorPos - sectorNumber;

                // calculate values for the three axes of the color.
                var p = brightness * (1.0 - saturation);
                var q = brightness * (1.0 - saturation * fractionalSector);
                var t = brightness * (1.0 - saturation * (1 - fractionalSector));

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
                (int)(b * 255.0 + 0.5)
            );
        }
    }
}
