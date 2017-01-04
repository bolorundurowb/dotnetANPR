using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace dotNETANPR.ImageAnalysis
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
            Bitmap bitmap = new Bitmap(Image.Width + 40, Image.Height + 40, PixelFormat.Format8bppIndexed);
            Graphics graphics = Graphics.FromImage(bitmap);
            SolidBrush brush = new SolidBrush(Color.LightGray);
            Pen pen = new Pen(Color.LightGray);
            Rectangle rectangle = new Rectangle(0, 0, Image.Width, Image.Height);

            graphics.FillRectangle(brush, rectangle);
            graphics.DrawRectangle(pen, rectangle);
            graphics.DrawImage(Image, 35, 5);
            brush.Color = Color.Black;
            pen.Color = Color.Black;
            graphics.DrawRectangle(pen, 35, 5, Image.Width, Image.Height);

            Font font = new Font("Consolas", 20F);
            for (int i = 0; i < Image.Width; i += 50)
            {
                graphics.DrawString(i.ToString(), font, brush, 35 + i, bitmap.Height - 10);
                graphics.DrawLine(pen, i + 35, Image.Height + 5, i + 35, Image.Height + 15);
            }
            for (int i = 0; i < Image.Height; i++)
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
            int r = bitmap;
        }

        public static float GetSaturation(Bitmap bitmap, int x, int y)
        {
            int r = bitmap;
        }

        public static float GetHue(Bitmap bitmap, int x, int y)
        {
            int r = bitmap;
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
                Bitmap bitmap = new Bitmap(filePath);
                Bitmap image = new Bitmap(Image.Width, Image.Height, PixelFormat.Format8bppIndexed);
                Graphics graphics = Graphics.FromImage(image);
                graphics.DrawImage(bitmap, 0, 0);
                graphics.Dispose();
                Image = image;
            }
            catch (IOException)
            {
                throw;
            }
        }

        public void SaveImage(string filePath)
        {
            string type = filePath.Substring(filePath.LastIndexOf('.') + 1, filePath.Length).ToUpper();
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
            Statistics stats = new Statistics(Image);
            for (int x = 0; x < GetWidth(); x++)
            {
                for (int y = 0; y < GetHeight(); y++)
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
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            Graphics graphics = Graphics.FromImage(bitmap);
            float xScale = (float) width / bmp.Width;
            float yScale = (float) height / bmp.Height;
            Matrix matrix = new Matrix(xScale, 0, 0, yScale, 0, 0);
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
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            float xScale = (float) width / bmp.Width;
            float yScale = (float) height / bmp.Height;
            for (int i = 0; i < width; i++)
            {
                int x0min = (int) Math.Round(i * xScale);
                int x0max = (int)Math.Round((i + 1) * xScale);
                for (int j = 0; j < height; j++)
                {
                    int y0min = (int)Math.Round(j * yScale);
                    int y0max = (int)Math.Round((j + 1) * yScale);

                    float sum = 0;
                    int sumCount = 0;

                    for (int x0 = x0min; x0 < x0max; x0++)
                    {
                        for (int y0 = y0min; y0 < y0max; y0++)
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
            Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format8bppIndexed);
            bitmap.
        }
    }
}
