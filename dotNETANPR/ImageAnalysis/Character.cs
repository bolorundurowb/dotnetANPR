using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
{
    public class Character : Photo
    {
        public float FullWidth { get; set; }
        public float PieceWidth { get; set; }
        public float StatisticAverageHue { get; set; }
        public float StatisticContrast { get; set; }
        public float StatisticAverageBrightness { get; set; }
        public float StatisticMinimumBrightness { get; set; }
        public float StatisticMaximumBrightness { get; set; }
        public float StatisticAverageSaturation { get; set; }
        public float FullHeight { get; set; }
        public float PieceHeight { get; set; }

        public bool normalized;
        public PositionInPlate positionInPlate;
        public Bitmap thresholdedImage;
        private PixelMap.Piece bestPiece;

        public Character()
        {
            Image = null;
            Init();
        }

        public Character(Bitmap bitmap) : this(bitmap, bitmap, null)
        {
            Init();
        }

        public Character(string filePath) : base(filePath)
        {
            Bitmap origin = DuplicateBitmap(Image);
            AdaptiveThresholding();
            thresholdedImage = Image;
            Image = origin;
            Init();
        }

        public Character(Bitmap clone, Bitmap bitmap, PositionInPlate positionInPlate) : base(clone)
        {
            thresholdedImage = bitmap;
            this.positionInPlate = positionInPlate;
            Init();
        }

        public new Character Clone()
        {
            return new Character((Bitmap)Image.Clone(), (Bitmap)thresholdedImage.Clone(), positionInPlate);
        }

        private void Init()
        {
            FullHeight = GetHeight();
            FullWidth = GetWidth();
        }

        public void Normalize()
        {
            if (normalized) return;

            Bitmap colorImage = (Bitmap)GetBitmap().Clone();
            Image = thresholdedImage;
            PixelMap pixelMap = GetPixelMap();

            bestPiece = pixelMap.GetBestPiece();

            colorImage = GetBestPieceInFullColor(colorImage, bestPiece);

            ComputeStatisticBrightness(colorImage);
            ComputeStatisticContrast(colorImage);
            this.ComputeStatisticHue(colorImage);
            this.ComputeStatisticSaturation(colorImage);

            Image = bestPiece.Render() ?? new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

            PieceWidth = GetWidth();
            PieceHeight = GetHeight();

            NormalizeResizeOnly();
            normalized = true;
        }

        private void NormalizeResizeOnly()
        {
            int x = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_x");
            int y = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_y");
            if (x == 0 || y == 0) return;

            if (Intelligence.Intelligence.Configurator.GetIntProperty("char_resizeMethod") == 0)
            {
                LinearResize(x, y);
            }
            else
            {
                AverageResize(x, y);
            }
            NormalizeBrightness(0.5f);
        }

        private Bitmap GetBestPieceInFullColor(Bitmap colorImage, PixelMap.Piece piece)
        {
             if (piece.Width <= 0 || piece.Height <= 0) return colorImage;
            return colorImage.Clone(new Rectangle(
                piece.mostLeftPoint,
                piece.mostTopPoint,
                piece.Width,
                piece.Height), PixelFormat.Format8bppIndexed);
        }

        private void ComputeStatisticContrast(Bitmap colorImage)
        {
            float sum = 0;
            int w = colorImage.Width;
            int h = colorImage.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += Math.Abs(StatisticAverageBrightness - GetBrightness(colorImage, x, y));
                }
            }
            StatisticContrast = sum / (w * h);
        }

        private void ComputeStatisticBrightness(Bitmap bi)
        {
            float sum = 0;
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float value = GetBrightness(bi, x, y);
                    sum += value;
                    min = Math.Min(min, value);
                    max = Math.Max(max, value);
                }
            }
            StatisticAverageBrightness = sum / (w * h);
            StatisticMinimumBrightness = min;
            StatisticMaximumBrightness = max;
        }

        private void ComputeStatisticHue(Bitmap bi)
        {
            float sum = 0;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += GetHue(bi, x, y);
                }
            }
            this.StatisticAverageHue = sum / (w * h);
        }

        private void ComputeStatisticSaturation(Bitmap bi)
        {
            float sum = 0;
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    sum += GetSaturation(bi, x, y);
                }
            }
            this.StatisticAverageSaturation = sum / (w * h);
        }

        public PixelMap GetPixelMap()
        {
            return new PixelMap(this);
        }
    }
}
