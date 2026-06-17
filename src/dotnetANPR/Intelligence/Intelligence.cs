using System;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
using SkiaSharp;
using PS = DotNetANPR.Intelligence.Parser;

namespace DotNetANPR.Intelligence;

/// <summary>
/// Main recognition pipeline that orchestrates the entire ANPR process:
/// car snapshot analysis, band extraction, plate detection, skew correction,
/// character segmentation, heuristic filtering, character recognition, and syntax parsing.
/// </summary>
public class Intelligence
{
    private readonly CharacterRecognizer _chrRecognizer;
    private readonly PS.Parser _parser;

    /// <summary>
    /// Gets the duration of the last recognition process in milliseconds.
    /// </summary>
    public static long LastProcessDuration { get; private set; }

    /// <summary>
    /// Initializes a new <see cref="Intelligence"/> instance, creating the configured
    /// character recognizer (KNN or neural network) and the syntax parser.
    /// </summary>
    public Intelligence()
    {
        var config = AnprConfig.Instance;
        _chrRecognizer = config.Intelligence.ClassificationMethod == 0
            ? new KnnPatternClassifier()
            : new NeuralPatternClassifier();
        _parser = new PS.Parser();
    }

    /// <summary>
    /// Runs the full ANPR recognition pipeline on the given car snapshot.
    /// </summary>
    /// <param name="carSnapshot">The car image to analyze.</param>
    /// <returns>
    /// The recognized license plate text, or <c>null</c> if no plate could be recognized.
    /// </returns>
    public string? Recognize(CarSnapshot carSnapshot)
    {
        var config = AnprConfig.Instance;
        var timeMeter = new TimeMeter();
        var syntaxAnalysisMode = (SyntaxAnalysisMode)config.Intelligence.SyntaxAnalysis;
        var skewDetectionMode = config.Intelligence.SkewDetection;

        foreach (var band in carSnapshot.Bands())
        {
            foreach (var plate in band.Plates())
            {
                var localPlate = plate;

                // Skew detection and correction
                if (skewDetectionMode != 0)
                {
                    var notNormalizedCopy = (Plate)plate.Clone();
                    notNormalizedCopy.Image = notNormalizedCopy.HorizontalEdgeDetector(notNormalizedCopy.Image);
                    var hough = notNormalizedCopy.GetHoughTransformation();

                    var shearFactor = -(float)hough.Dy / hough.Dx;

                    var source = plate.Image;
                    var corrected = new SKBitmap(source.Width, source.Height);
                    using (var canvas = new SKCanvas(corrected))
                    {
                        canvas.Clear(SKColors.Black);
                        var matrix = SKMatrix.CreateSkew(0, shearFactor);
                        canvas.SetMatrix(matrix);
                        canvas.DrawBitmap(source, 0, 0);
                    }

                    localPlate = new Plate(corrected);
                }

                localPlate.Normalize();

                var plateWHratio = localPlate.Width / (float)localPlate.Height;
                if (plateWHratio < config.Intelligence.MinPlateWidthHeightRatio
                    || plateWHratio > config.Intelligence.MaxPlateWidthHeightRatio)
                    continue;

                var chars = localPlate.Characters();

                // Heuristic analysis: character count
                if (chars.Count < config.Intelligence.MinimumChars
                    || chars.Count > config.Intelligence.MaximumChars)
                    continue;

                if (plate.CharactersWidthDispersion(chars) > config.Intelligence.MaxCharWidthDispersion)
                    continue;

                var recognizedPlate = new RecognizedPlate();

                foreach (var chr in chars)
                    chr.Normalize();

                var averageHeight = plate.AveragePieceHeight(chars);
                var averageContrast = plate.AveragePieceContrast(chars);
                var averageBrightness = plate.AveragePieceBrightness(chars);
                var averageHue = plate.AveragePieceHue(chars);
                var averageSaturation = plate.AveragePieceSaturation(chars);

                foreach (var chr in chars)
                {
                    var ok = true;

                    float widthHeightRatio = chr.PieceWidth;
                    widthHeightRatio /= chr.PieceHeight;

                    if (widthHeightRatio < config.Intelligence.MinCharWidthHeightRatio
                        || widthHeightRatio > config.Intelligence.MaxCharWidthHeightRatio)
                    {
                        ok = false;
                        continue;
                    }

                    if (chr.PositionInPlate is null)
                        throw new ArgumentNullException(nameof(chr.PositionInPlate),
                            "Character position in plate is null");

                    if ((chr.PositionInPlate.LeftX < 2 || chr.PositionInPlate.RightX > plate.Width - 1)
                        && widthHeightRatio < 0.12)
                    {
                        ok = false;
                        continue;
                    }

                    var contrastCost = Math.Abs(chr.StatisticContrast - averageContrast);
                    var brightnessCost = Math.Abs(chr.StatisticAverageBrightness - averageBrightness);
                    var hueCost = Math.Abs(chr.StatisticAverageHue - averageHue);
                    var saturationCost = Math.Abs(chr.StatisticAverageSaturation - averageSaturation);
                    var heightCost = (chr.PieceHeight - averageHeight) / averageHeight;

                    if (brightnessCost > config.Intelligence.MaxBrightnessCostDispersion)
                        continue;

                    if (contrastCost > config.Intelligence.MaxContrastCostDispersion)
                        continue;

                    if (hueCost > config.Intelligence.MaxHueCostDispersion)
                        continue;

                    if (saturationCost > config.Intelligence.MaxSaturationCostDispersion)
                        continue;

                    if (heightCost < -config.Intelligence.MaxHeightCostDispersion)
                        continue;

                    var rc = _chrRecognizer.Recognize(chr);

                    if (rc.Patterns is null || rc.Patterns.Count == 0)
                        throw new InvalidOperationException(
                            "Recognized character does not have any patterns");

                    var similarityCost = rc.Patterns[0].Cost;
                    if (similarityCost > config.Intelligence.MaxSimilarityCostDispersion)
                        continue;

                    recognizedPlate.AddCharacter(rc);
                }

                // If too few characters recognized, try next candidate
                if (recognizedPlate.Characters.Count < config.Intelligence.MinimumChars)
                    continue;

                LastProcessDuration = timeMeter.GetTime();
                return _parser.Parse(recognizedPlate, syntaxAnalysisMode);
            }
        }

        LastProcessDuration = timeMeter.GetTime();
        return null;
    }
}
