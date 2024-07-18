using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using DotNetANPR.Recognizer;

namespace DotNetANPR.ImageAnalysis;

public class Character : Photo
{
    public bool normalized = false;
    public PositionInPlate? positionInPlate = null;

    public int fullWidth, fullHeight, pieceWidth, pieceHeight;

    public float statisticAverageBrightness;
    public float statisticMinimumBrightness;
    public float statisticMaximumBrightness;
    public float statisticContrast;
    public float statisticAverageHue;
    public float statisticAverageSaturation;

    public Bitmap thresholdedImage;

    public PixelMap PixelMap => new PixelMap(this);

    public Character(string fileName) : base(new Bitmap(fileName))
    {
        var origin = DuplicateBitmap(Image);
        AdaptiveThresholding();
        this.thresholdedImage = Image;
        Image = origin;

        Init();
    }

    public Character(Bitmap image) : this(image, image, null) { }

    public Character(Bitmap image, Bitmap thresholdedImage, PositionInPlate? positionInPlate) : base(image)
    {
        this.thresholdedImage = thresholdedImage;
        this.positionInPlate = positionInPlate;

        Init();
    }

    public static List<String> AlphabetList(String directory)
    {
        const String alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
        String suffix = Suffix(directory);
        directory = directory.TrimEnd('/');
        List<String> filenames = new();
        for (int i = 0; i < alphaString.Length; i++)
        {
            String s = directory + Path.PathSeparator + alphaString[i] + suffix + ".jpg";
            if (File.Exists(s))
            {
                filenames.Add(s);
            }
        }

        return filenames;
    }

    public void normalize()
    {
        if (normalized)
        {
            return;
        }

        Bitmap colorImage = DuplicateBitmap(Image);
        Image = (thresholdedImage);
        PixelMap pixelMap = PixelMap;
        PixelMap.Piece bestPiece = pixelMap.BestPiece();
        colorImage = BestPieceInFullColor(colorImage, bestPiece);

        // Compute statistics
        ComputeStatisticBrightness(colorImage);
        ComputeStatisticContrast(colorImage);
        ComputeStatisticHue(colorImage);
        ComputeStatisticSaturation(colorImage);

        Image = (bestPiece.Render());
        if (Image == null)
        {
            Image = (new Bitmap(1, 1, PixelFormat.Format24bppRgb));
        }

        pieceWidth = Width;
        pieceHeight = Height;
        NormalizeResizeOnly();
        normalized = true;
    }

    public List<Double> ExtractEdgeFeatures()
    {
        int width = Image.Width;
        int height = Image.Height;
        double featureMatch;
        float[][] array = BitmapToArrayWithBounds(Image, width, height);
        width += 2; // add edges
        height += 2;
        float[][] features = CharacterRecognizer.Features;
        double[] output = new double[features.Length * 4];

        for (int f = 0; f < features.Length; f++)
        {
            for (int my = 0; my < (height - 1); my++)
            {
                for (int mx = 0; mx < (width - 1); mx++)
                {
                    featureMatch = 0;
                    featureMatch += Math.Abs(array[mx][my] - features[f][0]);
                    featureMatch += Math.Abs(array[mx + 1][my] - features[f][1]);
                    featureMatch += Math.Abs(array[mx][my + 1] - features[f][2]);
                    featureMatch += Math.Abs(array[mx + 1][my + 1] - features[f][3]);

                    int bias = 0;
                    if (mx >= (width / 2))
                    {
                        bias += features.Length; // if we are in the right quadrant, move the bias by one class
                    }

                    if (my >= (height / 2))
                    {
                        bias += features.Length * 2; // if we are in the left quadrant, move the bias by two classes
                    }

                    output[bias + f] += featureMatch < 0.05 ? 1 : 0;
                }
            }
        }

        List<Double> outputList = new();
        foreach (Double value in output)
        {
            outputList.Add(value);
        }

        return outputList;
    }

    public List<Double> ExtractMapFeatures()
    {
        List<Double> vectorInput = new();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                vectorInput.Add((double)GetBrightness(x, y));
            }
        }

        return vectorInput;
    }

    public List<Double> ExtractFeatures()
    {
        int featureExtractionMethod = Configurator.Instance.Get<int>("char_featuresExtractionMethod");
        if (featureExtractionMethod == 0)
        {
            return ExtractMapFeatures();
        }
        else
        {
            return ExtractEdgeFeatures();
        }
    }

    #region Private Helpers

    private void Init()
    {
        fullHeight = Height;
        fullWidth = Width;
    }

    private static string Suffix(string directoryName)
    {
        directoryName = directoryName.TrimEnd('/');
        return directoryName.Substring(directoryName.LastIndexOf('_'));
    }

    private Bitmap BestPieceInFullColor(Bitmap bi, PixelMap.Piece piece)
    {
        if ((piece.Width <= 0) || (piece.Height <= 0))
        {
            return bi;
        }

        return bi.SubImage(piece.MostLeftPoint(), piece.MostTopPoint(), piece.Width, piece.Height);
    }

    private void NormalizeResizeOnly()
    {
        // returns the same Char object
        int x = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
        int y = Configurator.Instance.Get<int>("char_normalizeddimensions_y");
        if ((x == 0) || (y == 0))
        {
            return;
        }

        if (Configurator.Instance.Get<int>("char_resizeMethod") == 0)
        {
            LinearResize(x, y); // do a weighted average
        }
        else
        {
            AverageResize(x, y);
        }

        NormalizeBrightness(0.5f);
    }

    private void ComputeStatisticContrast(Bitmap bi)
    {
        float sum = 0;
        int w = bi.Width;
        int h = bi.Height;
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                sum += Math.Abs(statisticAverageBrightness - Photo.GetBrightness(bi, x, y));
            }
        }

        statisticContrast = sum / (w * h);
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
                float value = Photo.GetBrightness(bi, x, y);
                sum += value;
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        statisticAverageBrightness = sum / (w * h);
        statisticMinimumBrightness = min;
        statisticMaximumBrightness = max;
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
                sum += Photo.GetHue(bi, x, y);
            }
        }

        statisticAverageHue = sum / (w * h);
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
                sum += Photo.GetSaturation(bi, x, y);
            }
        }

        statisticAverageSaturation = sum / (w * h);
    }

    #endregion
}
