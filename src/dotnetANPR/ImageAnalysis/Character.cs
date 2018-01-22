using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using dotnetANPR.Recognizer;

namespace dotnetANPR.ImageAnalysis
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

        public bool Normalized;
        public PositionInPlate PositionInPlate;
        public Bitmap ThresholdedImage;
        private PixelMap.Piece _bestPiece;

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
            var origin = DuplicateBitmap(Image);
            AdaptiveThresholding();
            ThresholdedImage = Image;
            Image = origin;
            Init();
        }

        public Character(Bitmap clone, Bitmap bitmap, PositionInPlate positionInPlate) : base(clone)
        {
            ThresholdedImage = bitmap;
            this.PositionInPlate = positionInPlate;
            Init();
        }

        public new Character Clone()
        {
            return new Character((Bitmap)Image.Clone(), (Bitmap)ThresholdedImage.Clone(), PositionInPlate);
        }

        private void Init()
        {
            FullHeight = GetHeight();
            FullWidth = GetWidth();
        }

        public void Normalize()
        {
            if (Normalized) return;

            var colorImage = (Bitmap)GetBitmap().Clone();
            Image = ThresholdedImage;
            var pixelMap = GetPixelMap();

            _bestPiece = pixelMap.GetBestPiece();

            colorImage = GetBestPieceInFullColor(colorImage, _bestPiece);

            ComputeStatisticBrightness(colorImage);
            ComputeStatisticContrast(colorImage);
            ComputeStatisticHue(colorImage);
            ComputeStatisticSaturation(colorImage);

            Image = _bestPiece.Render() ?? new Bitmap(1, 1, PixelFormat.Format8bppIndexed);

            PieceWidth = GetWidth();
            PieceHeight = GetHeight();

            NormalizeResizeOnly();
            Normalized = true;
        }

        private void NormalizeResizeOnly()
        {
            var x = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_x");
            var y = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_y");
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
            var w = colorImage.Width;
            var h = colorImage.Height;
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    sum += Math.Abs(StatisticAverageBrightness - GetBrightness(colorImage, x, y));
                }
            }
            StatisticContrast = sum / (w * h);
        }

        private void ComputeStatisticBrightness(Bitmap bi)
        {
            float sum = 0;
            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;
            var w = bi.Width;
            var h = bi.Height;
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var value = GetBrightness(bi, x, y);
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
            var w = bi.Width;
            var h = bi.Height;
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    sum += GetHue(bi, x, y);
                }
            }
            StatisticAverageHue = sum / (w * h);
        }

        private void ComputeStatisticSaturation(Bitmap bi)
        {
            float sum = 0;
            var w = bi.Width;
            var h = bi.Height;
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    sum += GetSaturation(bi, x, y);
                }
            }
            StatisticAverageSaturation = sum / (w * h);
        }

        public PixelMap GetPixelMap()
        {
            return new PixelMap(this);
        }

        public List<double> ExtractEdgeFeatures()
        {
            var w = Image.Width;
            var h = Image.Height;
            double featureMatch;

            var array = BitmapToArrayWithBounds(Image, w, h);
            w += 2;
            h += 2;

            var features = CharacterRecognizer.features;
            var output = new double[features.Length * 4];

            for (var f = 0; f < features.Length; f++)
            {
                for (var my = 0; my < h - 1; my++)
                {
                    for (var mx = 0; mx < w - 1; mx++)
                    {
                        featureMatch = 0;
                        featureMatch += Math.Abs(array[mx, my] - features[f, 0]);
                        featureMatch += Math.Abs(array[mx + 1, my] - features[f, 1]);
                        featureMatch += Math.Abs(array[mx, my + 1] - features[f, 2]);
                        featureMatch += Math.Abs(array[mx + 1, my + 1] - features[f, 3]);

                        var bias = 0;
                        if (mx >= w / 2) bias += features.Length;
                        if (my >= h / 2) bias += features.Length * 2;
                        output[bias + f] += featureMatch < 0.05 ? 1 : 0;
                    }
                }
            }
            var outputList = new List<double>();
            foreach (var value in output) outputList.Add(value);
            return outputList;
        }

        public List<double> ExtractMapFeatures()
        {
            var vectorInput = new List<double>();
            for (var y = 0; y < GetHeight(); y++)
            for (var x = 0; x < GetWidth(); x++)
                vectorInput.Add(GetBrightness(x, y));
            return vectorInput;
        }

        public List<double> ExtractFeatures()
        {
            var featureExtractionMethod = Intelligence.Intelligence.Configurator.GetIntProperty("char_featuresExtractionMethod");
            if (featureExtractionMethod == 0)
                return ExtractMapFeatures();
            return ExtractEdgeFeatures();
        }
    }
}
