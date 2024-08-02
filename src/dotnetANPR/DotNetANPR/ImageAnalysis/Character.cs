using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using DotNetANPR.Recognizer;

namespace DotNetANPR.ImageAnalysis;

public class Character : Photo
{
    public bool Normalized;
    public PositionInPlate? PositionInPlate;

    public int FullWidth, FullHeight, PieceWidth, PieceHeight;

    public float StatisticAverageBrightness;
    public float StatisticMinimumBrightness;
    public float StatisticMaximumBrightness;
    public float StatisticContrast;
    public float StatisticAverageHue;
    public float StatisticAverageSaturation;

    public readonly Bitmap ThresholdedImage;

    public PixelMap PixelMap => new(this);

    public Character(string fileName) : base(new Bitmap(fileName))
    {
        var origin = DuplicateBitmap(Image);
        AdaptiveThresholding();
        ThresholdedImage = Image;
        Image = origin;

        Init();
    }

    public Character(Bitmap image) : this(image, image, null) { }

    public Character(Bitmap image, Bitmap thresholdedImage, PositionInPlate? positionInPlate) : base(image)
    {
        ThresholdedImage = thresholdedImage;
        PositionInPlate = positionInPlate;

        Init();
    }

    public static List<string> AlphabetList(string directory)
    {
        const string alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
        var suffix = Suffix(directory);
        directory = directory.TrimEnd('/');
        List<string> filenames = [];
        filenames.AddRange(alphaString
            .Select(t => directory + Path.PathSeparator + t + suffix + ".jpg")
            .Where(File.Exists));

        return filenames;
    }

    public void Normalize()
    {
        if (Normalized)
            return;

        var colorImage = DuplicateBitmap(Image);
        Image = (ThresholdedImage);
        var pixelMap = PixelMap;
        var bestPiece = pixelMap.BestPiece();
        colorImage = BestPieceInFullColor(colorImage, bestPiece);

        // Compute statistics
        ComputeStatisticBrightness(colorImage);
        ComputeStatisticContrast(colorImage);
        ComputeStatisticHue(colorImage);
        ComputeStatisticSaturation(colorImage);

        Image = (bestPiece.Render()) ?? (new Bitmap(1, 1, PixelFormat.Format24bppRgb));

        PieceWidth = Width;
        PieceHeight = Height;
        NormalizeResizeOnly();
        Normalized = true;
    }

    public List<double> ExtractEdgeFeatures()
    {
        var width = Image.Width;
        var height = Image.Height;
        var array = BitmapToArrayWithBounds(Image, width, height);
        width += 2; // add edges
        height += 2;
        var features = CharacterRecognizer.Features;
        var output = new double[features.Length * 4];

        for (var f = 0; f < features.Length; f++)
        for (var my = 0; my < (height - 1); my++)
        for (var mx = 0; mx < (width - 1); mx++)
        {
            double featureMatch = 0;
            featureMatch += Math.Abs(array[mx, my] - features[f][0]);
            featureMatch += Math.Abs(array[mx + 1, my] - features[f][1]);
            featureMatch += Math.Abs(array[mx, my + 1] - features[f][2]);
            featureMatch += Math.Abs(array[mx + 1, my + 1] - features[f][3]);

            var bias = 0;
            if (mx >= (width / 2))
                bias += features.Length; // if we are in the right quadrant, move the bias by one class

            if (my >= (height / 2))
                bias += features.Length * 2; // if we are in the left quadrant, move the bias by two classes

            output[bias + f] += featureMatch < 0.05 ? 1 : 0;
        }

        return output.ToList();
    }

    public List<double> ExtractMapFeatures()
    {
        List<double> vectorInput = [];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            vectorInput.Add(GetBrightness(x, y));

        return vectorInput;
    }

    public List<double> ExtractFeatures()
    {
        var featureExtractionMethod = Configurator.Instance.Get<int>("char_featuresExtractionMethod");
        return featureExtractionMethod == 0 ? ExtractMapFeatures() : ExtractEdgeFeatures();
    }

    #region Private Helpers

    private void Init()
    {
        FullHeight = Height;
        FullWidth = Width;
    }

    private static string Suffix(string directoryName)
    {
        directoryName = directoryName.TrimEnd('/');
        return directoryName.Substring(directoryName.LastIndexOf('_'));
    }

    private Bitmap BestPieceInFullColor(Bitmap bi, PixelMap.Piece piece)
    {
        if ((piece.Width <= 0) || (piece.Height <= 0))
            return bi;

        return bi.SubImage(piece.MostLeftPoint, piece.MostTopPoint, piece.Width, piece.Height);
    }

    private void NormalizeResizeOnly()
    {
        // returns the same Char object
        var x = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
        var y = Configurator.Instance.Get<int>("char_normalizeddimensions_y");

        if ((x == 0) || (y == 0))
            return;

        if (Configurator.Instance.Get<int>("char_resizeMethod") == 0)
            LinearResize(x, y); // do a weighted average
        else
            AverageResize(x, y);

        NormalizeBrightness(0.5f);
    }

    private void ComputeStatisticContrast(Bitmap bi)
    {
        float sum = 0;
        var w = bi.Width;
        var h = bi.Height;
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            sum += Math.Abs(StatisticAverageBrightness - GetBrightness(bi, x, y));

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
        for (var y = 0; y < h; y++)
        {
            var value = GetBrightness(bi, x, y);
            sum += value;
            min = Math.Min(min, value);
            max = Math.Max(max, value);
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
        for (var y = 0; y < h; y++)
            sum += GetHue(bi, x, y);

        StatisticAverageHue = sum / (w * h);
    }

    private void ComputeStatisticSaturation(Bitmap bi)
    {
        float sum = 0;
        var w = bi.Width;
        var h = bi.Height;
        for (var x = 0; x < w; x++)
        for (var y = 0; y < h; y++)
            sum += GetSaturation(bi, x, y);

        StatisticAverageSaturation = sum / (w * h);
    }

    #endregion
}