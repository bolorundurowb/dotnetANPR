using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a single character extracted from a license plate.
/// Provides normalization, statistics computation, and feature extraction for recognition.
/// </summary>
public class Character : Photo
{
    /// <summary>
    /// Indicates whether the character has been normalized.
    /// </summary>
    public bool Normalized;

    /// <summary>
    /// The position of this character within the plate image.
    /// </summary>
    public PositionInPlate? PositionInPlate;

    /// <summary>
    /// The full width of the character before normalization.
    /// </summary>
    public int FullWidth;

    /// <summary>
    /// The full height of the character before normalization.
    /// </summary>
    public int FullHeight;

    /// <summary>
    /// The width of the best piece after connected component analysis.
    /// </summary>
    public int PieceWidth;

    /// <summary>
    /// The height of the best piece after connected component analysis.
    /// </summary>
    public int PieceHeight;

    /// <summary>
    /// The average brightness of the character in full color.
    /// </summary>
    public float StatisticAverageBrightness;

    /// <summary>
    /// The minimum brightness of the character in full color.
    /// </summary>
    public float StatisticMinimumBrightness;

    /// <summary>
    /// The maximum brightness of the character in full color.
    /// </summary>
    public float StatisticMaximumBrightness;

    /// <summary>
    /// The contrast of the character, computed as average absolute deviation from mean brightness.
    /// </summary>
    public float StatisticContrast;

    /// <summary>
    /// The average hue of the character in full color.
    /// </summary>
    public float StatisticAverageHue;

    /// <summary>
    /// The average saturation of the character in full color.
    /// </summary>
    public float StatisticAverageSaturation;

    /// <summary>
    /// The thresholded (binary) version of this character image.
    /// </summary>
    public readonly SKBitmap ThresholdedImage;

    /// <summary>
    /// Creates a new <see cref="PixelMap"/> from this character's current image.
    /// </summary>
    public PixelMap PixelMap => new(this);

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class by loading an image from a file.
    /// The loaded image is adaptively thresholded to produce the binary version.
    /// </summary>
    /// <param name="fileName">The path to the character image file.</param>
    public Character(string fileName) : this(SKBitmap.Decode(fileName), applyThresholding: true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class by loading an image from a stream.
    /// The loaded image is adaptively thresholded to produce the binary version.
    /// </summary>
    /// <param name="stream">The stream containing the character image.</param>
    public Character(Stream stream) : this(SKBitmap.Decode(stream), applyThresholding: true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class from a bitmap.
    /// The same bitmap is used as both the original and thresholded image.
    /// </summary>
    /// <param name="image">The character bitmap.</param>
    public Character(SKBitmap image) : this(image, image, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class with separate original
    /// and thresholded images, and an optional position within the plate.
    /// </summary>
    /// <param name="image">The original character bitmap.</param>
    /// <param name="thresholdedImage">The thresholded (binary) character bitmap.</param>
    /// <param name="positionInPlate">The position of this character within the plate, or null.</param>
    public Character(SKBitmap image, SKBitmap thresholdedImage, PositionInPlate? positionInPlate) : base(image)
    {
        ThresholdedImage = thresholdedImage;
        PositionInPlate = positionInPlate;

        Init();
    }

    private Character(SKBitmap image, bool applyThresholding) : base(image)
    {
        if (applyThresholding)
        {
            var origin = DuplicateBitmap(Image);
            AdaptiveThresholding();
            ThresholdedImage = Image;
            Image = origin;
        }
        else
        {
            ThresholdedImage = image;
        }

        Init();
    }

    /// <summary>
    /// Returns a list of paths for all alphabet character images found in the specified directory
    /// or embedded resource prefix.
    /// </summary>
    /// <param name="directory">The directory or logical resource path containing alphabet image files.</param>
    /// <returns>A list of paths to existing character images.</returns>
    public static List<string> AlphabetList(string directory)
    {
        const string alphaString = "0123456789abcdefghijklmnopqrstuvwxyz";
        var suffix = Suffix(directory);
        directory = directory.TrimEnd('/', '\\');

        var candidates = alphaString
            .Select(t => directory + "/" + t + suffix + ".jpg")
            .ToList();

        // Prefer embedded resources; fall back to the file system if none are embedded.
        var embedded = candidates.Where(ResourceHelper.Exists).ToList();
        if (embedded.Count != 0)
            return embedded;

        return candidates
            .Select(p => p.Replace('/', Path.DirectorySeparatorChar))
            .Where(File.Exists)
            .ToList();
    }

    /// <summary>
    /// Normalizes this character by extracting the best connected component,
    /// computing statistics, and resizing to standard dimensions.
    /// </summary>
    public void Normalize()
    {
        if (Normalized)
            return;

        var colorImage = DuplicateBitmap(Image);
        Image = ThresholdedImage;
        var pixelMap = PixelMap;
        var bestPiece = pixelMap.BestPiece();
        colorImage = BestPieceInFullColor(colorImage, bestPiece);

        // Compute statistics
        ComputeStatisticBrightness(colorImage);
        ComputeStatisticContrast(colorImage);
        ComputeStatisticHue(colorImage);
        ComputeStatisticSaturation(colorImage);

        Image = bestPiece.Render() ?? new SKBitmap(1, 1);

        PieceWidth = Width;
        PieceHeight = Height;
        NormalizeResizeOnly();
        Normalized = true;
    }

    /// <summary>
    /// Extracts edge-based features from the character image for recognition.
    /// </summary>
    /// <returns>A list of feature values.</returns>
    public List<double> ExtractEdgeFeatures()
    {
        var width = Image.Width;
        var height = Image.Height;
        var array = BitmapToArrayWithBounds(Image, width, height);
        width += 2;
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
                        bias += features.Length;

                    if (my >= height / 2)
                        bias += features.Length * 2;

                    output[bias + f] += featureMatch < 0.05 ? 1 : 0;
                }

        return output.ToList();
    }

    /// <summary>
    /// Extracts pixel brightness map features from the character image for recognition.
    /// </summary>
    /// <returns>A list of brightness values for each pixel.</returns>
    public List<double> ExtractMapFeatures()
    {
        List<double> vectorInput = [];
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                vectorInput.Add(GetBrightness(x, y));

        return vectorInput;
    }

    /// <summary>
    /// Extracts features from the character image using the configured extraction method.
    /// </summary>
    /// <returns>A list of feature values for recognition.</returns>
    public List<double> ExtractFeatures()
    {
        var featureExtractionMethod = AnprConfig.Instance.Character.FeaturesExtractionMethod;
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

    private SKBitmap BestPieceInFullColor(SKBitmap bi, PixelMap.Piece piece)
    {
        if (piece.Width <= 0 || piece.Height <= 0)
            return bi;

        return bi.SubImage(piece.MostLeftPoint, piece.MostTopPoint, piece.Width, piece.Height);
    }

    private void NormalizeResizeOnly()
    {
        var x = AnprConfig.Instance.Character.NormalizedWidth;
        var y = AnprConfig.Instance.Character.NormalizedHeight;

        if (x == 0 || y == 0)
            return;

        if (AnprConfig.Instance.Character.ResizeMethod == 0)
            LinearResize(x, y);
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
