using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.Recognizer;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a single character extracted from a licence plate image.
/// Handles normalisation, feature extraction, and statistical analysis of brightness, contrast, hue, and saturation.
/// </summary>
public class Character : Photo
{
    /// <summary>Whether the character has been normalised.</summary>
    public bool Normalized;

    /// <summary>The position of this character within the plate.</summary>
    public PositionInPlate? PositionInPlate;

    /// <summary>Full width and height of the character region.</summary>
    public int FullWidth, FullHeight;

    /// <summary>Width and height of the character after cropping to its best connected piece.</summary>
    public int PieceWidth, PieceHeight;

    /// <summary>Average brightness of the character region.</summary>
    public float StatisticAverageBrightness;

    /// <summary>Minimum brightness of the character region.</summary>
    public float StatisticMinimumBrightness;

    /// <summary>Maximum brightness of the character region.</summary>
    public float StatisticMaximumBrightness;

    /// <summary>Contrast of the character region.</summary>
    public float StatisticContrast;

    /// <summary>Average hue of the character region.</summary>
    public float StatisticAverageHue;

    /// <summary>Average saturation of the character region.</summary>
    public float StatisticAverageSaturation;

    /// <summary>The thresholded (binary) version of the character image.</summary>
    public readonly SKBitmap ThresholdedImage;

    /// <summary>Gets a pixel map representation for connected-component analysis.</summary>
    public PixelMap PixelMap => new(this);

    /// <summary>
    /// Creates a character from an image file, applying adaptive thresholding.
    /// </summary>
    public Character(string fileName) : base(SkiaSharpAdapter.LoadBitmap(fileName))
    {
        var origin = DuplicateBitmap(Image);
        AdaptiveThresholding();
        ThresholdedImage = Image;
        Image = origin;

        Init();
    }

    public Character(SKBitmap image) : this(image, image, null) { }

    /// <summary>
    /// Creates a character from an image and optional thresholded version with a position in the plate.
    /// </summary>
    public Character(SKBitmap image, SKBitmap thresholdedImage, PositionInPlate? positionInPlate) : base(image)
    {
        ThresholdedImage = thresholdedImage;
        PositionInPlate = positionInPlate;

        Init();
    }

    /// <summary>
    /// Lists all available alphabet image files in the specified directory.
    /// </summary>
    public static List<string> AlphabetList(string directory)
    {
        const string alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
        var suffix = Suffix(directory);
        directory = directory.TrimEnd('/');
        List<string> filenames = [];
        filenames.AddRange(alphaString
            .Select(t => Path.Combine(directory, t + suffix + ".jpg"))
            .Where(File.Exists));

        return filenames;
    }

    /// <summary>
    /// Normalises the character by cropping to the best connected piece, computing statistics,
    /// and resizing to the configured normalised dimensions.
    /// </summary>
    public void Normalize()
    {
        if (Normalized)
            return;

        var colorImage = DuplicateBitmap(Image);
        Image = ThresholdedImage;
        var pixelMap = PixelMap;
        var bestPiece = pixelMap.BestPiece();

        // Crop to the best piece region if it has valid dimensions
        SKBitmap statsImage;
        bool ownStatsImage;
        if (bestPiece.Width > 0 && bestPiece.Height > 0)
        {
            statsImage = colorImage.SubImage(
                bestPiece.MostLeftPoint, bestPiece.MostTopPoint,
                bestPiece.Width, bestPiece.Height);
            ownStatsImage = true;
        }
        else
        {
            statsImage = colorImage;
            ownStatsImage = false;
        }

        // Compute statistics
        ComputeStatisticBrightness(statsImage);
        ComputeStatisticContrast(statsImage);
        ComputeStatisticHue(statsImage);
        ComputeStatisticSaturation(statsImage);

        if (ownStatsImage)
            statsImage.Dispose();
        colorImage.Dispose();

        Image = bestPiece.Render() ?? new SKBitmap(1, 1);

        PieceWidth = Width;
        PieceHeight = Height;
        NormalizeResizeOnly();
        Normalized = true;
    }

    /// <summary>
    /// Extracts edge-based features from the character for classification.
    /// </summary>
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
            for (var my = 0; my < height - 1; my++)
                for (var mx = 0; mx < width - 1; mx++)
                {
                    double featureMatch = 0;
                    featureMatch += Math.Abs(array[mx, my] - features[f][0]);
                    featureMatch += Math.Abs(array[mx + 1, my] - features[f][1]);
                    featureMatch += Math.Abs(array[mx, my + 1] - features[f][2]);
                    featureMatch += Math.Abs(array[mx + 1, my + 1] - features[f][3]);

                    var bias = 0;
                    if (mx >= width / 2)
                        bias += features.Length; // if we are in the right quadrant, move the bias by one class

                    if (my >= height / 2)
                        bias += features.Length * 2; // if we are in the left quadrant, move the bias by two classes

                    output[bias + f] += featureMatch < 0.05 ? 1 : 0;
                }

        return output.ToList();
    }

    /// <summary>
    /// Extracts pixel-map features (brightness values) from the character for classification.
    /// </summary>
    public List<double> ExtractMapFeatures()
    {
        List<double> vectorInput = [];
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                vectorInput.Add(GetBrightness(x, y));

        return vectorInput;
    }

    /// <summary>
    /// Extracts features from the character using the configured extraction method (map or edge).
    /// </summary>
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

    private void NormalizeResizeOnly()
    {
        // returns the same Char object
        var x = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
        var y = Configurator.Instance.Get<int>("char_normalizeddimensions_y");

        if (x == 0 || y == 0)
            return;

        if (Configurator.Instance.Get<int>("char_resizeMethod") == 0)
            LinearResize(x, y); // do a weighted average
        else
            AverageResize(x, y);

        NormalizeBrightness(0.5f);
    }

    private void ComputeStatisticContrast(SKBitmap bi)
    {
        float sum = 0;
        var w = bi.Width;
        var h = bi.Height;
        for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                sum += Math.Abs(StatisticAverageBrightness - GetBrightness(bi, x, y));

        StatisticContrast = sum / (w * h);
    }

    private void ComputeStatisticBrightness(SKBitmap bi)
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

    private void ComputeStatisticHue(SKBitmap bi)
    {
        float sum = 0;
        var w = bi.Width;
        var h = bi.Height;
        for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                sum += GetHue(bi, x, y);

        StatisticAverageHue = sum / (w * h);
    }

    private void ComputeStatisticSaturation(SKBitmap bi)
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