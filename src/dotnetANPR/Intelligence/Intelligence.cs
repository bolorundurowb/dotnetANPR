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

    private static readonly Configurator Configurator = Configurator.Instance;

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
        var classificationMethod = Configurator.Get<int>("intelligence_classification_method");
        _chrRecognizer = classificationMethod == 0
            ? new KnnPatternClassifier()
            : new NeuralPatternClassifier();
        _parser = new PS.Parser();
    }

    /// <summary>
    /// Runs the full ANPR recognition pipeline on the given car snapshot.
    /// </summary>
    /// <param name="carSnapshot">The car image to analyze.</param>
    /// <param name="reportGenerator">
    /// An optional <see cref="ReportGenerator"/> for producing an HTML diagnostic report.
    /// Pass <c>null</c> to skip report generation.
    /// </param>
    /// <returns>
    /// The recognized license plate text, or <c>null</c> if no plate could be recognized.
    /// </returns>
    public string? Recognize(CarSnapshot carSnapshot, ReportGenerator? reportGenerator = null)
    {
        var generateReport = reportGenerator is not null;
        var timeMeter = new TimeMeter();
        var syntaxAnalysisMode =
            (SyntaxAnalysisMode)Configurator.Get<int>("intelligence_syntaxanalysis");
        var skewDetectionMode = Configurator.Get<int>("intelligence_skewdetection");

        if (generateReport)
        {
            reportGenerator!.InsertText("<h1>Automatic Number Plate Recognition Report</h1>");
            reportGenerator.InsertText("<span>Image width: " + carSnapshot.Width + " px</span>");
            reportGenerator.InsertText("<span>Image height: " + carSnapshot.Height + " px</span>");
            reportGenerator.InsertText("<h2>Vertical and Horizontal plate projection</h2>");
            reportGenerator.InsertImage(carSnapshot.RenderGraph(), "snapshotgraph", 0, 0);
            reportGenerator.InsertImage(carSnapshot.GetBitmapWithAxes(), "snapshot", 0, 0);
        }

        foreach (var band in carSnapshot.Bands())
        {
            if (generateReport)
            {
                reportGenerator!.InsertText("<div class='bandtxt'><h4>Band<br></h4>");
                reportGenerator.InsertImage(band.Image, "bandsmall", 250, 30);
                reportGenerator.InsertText("<span>Band width : " + band.Width + " px</span>");
                reportGenerator.InsertText("<span>Band height : " + band.Height + " px</span>");
                reportGenerator.InsertText("</div>");
            }

            foreach (var plate in band.Plates())
            {
                var localPlate = plate;

                if (generateReport)
                {
                    reportGenerator!.InsertText("<div class='platetxt'><h4>Plate<br></h4>");
                    reportGenerator.InsertImage(localPlate.Image, "platesmall", 120, 30);
                    reportGenerator.InsertText("<span>Plate width : " + localPlate.Width + " px</span>");
                    reportGenerator.InsertText("<span>Plate height : " + localPlate.Height + " px</span>");
                    reportGenerator.InsertText("</div>");
                }

                // Skew detection and correction
                Plate? notNormalizedCopy = null;
                SKBitmap? renderedHoughTransform = null;
                HoughTransformation? hough = null;

                if (generateReport || skewDetectionMode != 0)
                {
                    notNormalizedCopy = (Plate)plate.Clone();
                    notNormalizedCopy.Image = notNormalizedCopy.HorizontalEdgeDetector(notNormalizedCopy.Image);
                    hough = notNormalizedCopy.GetHoughTransformation();
                    renderedHoughTransform = hough.Render(HoughTransformation.RenderType.RenderAll,
                        HoughTransformation.ColorType.BlackAndWhite);

                    if (skewDetectionMode != 0)
                    {
                        // Apply skew correction using SkiaSharp shear transform
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
                }

                localPlate.Normalize();

                var plateWHratio = localPlate.Width / (float)localPlate.Height;
                if (plateWHratio < Configurator.Get<double>("intelligence_minPlateWidthHeightRatio")
                    || plateWHratio > Configurator.Get<double>("intelligence_maxPlateWidthHeightRatio"))
                    continue;

                var chars = localPlate.Characters();

                // Heuristic analysis: character count
                if (chars.Count < Configurator.Get<int>("intelligence_minimumChars")
                    || chars.Count > Configurator.Get<int>("intelligence_maximumChars"))
                    continue;

                if (plate.CharactersWidthDispersion(chars) >
                    Configurator.Get<double>("intelligence_maxCharWidthDispersion"))
                    continue;

                // Plate accepted; normalize and begin character heuristic
                if (generateReport)
                {
                    reportGenerator!.InsertText("<h2>Detected band</h2>");
                    reportGenerator.InsertImage(band.GetBitmapWithAxes(), "band", 0, 0);
                    reportGenerator.InsertImage(band.RenderGraph(), "bandgraph", 0, 0);
                    reportGenerator.InsertText("<h2>Detected plate</h2>");
                    var plateCopy = (Plate)plate.Clone();
                    plateCopy.LinearResize(450, 90);
                    reportGenerator.InsertImage(plateCopy.GetBitmapWithAxes(), "plate", 0, 0);
                    reportGenerator.InsertImage(plateCopy.RenderGraph(), "plategraph", 0, 0);
                }

                // Skew report
                if (generateReport)
                {
                    reportGenerator!.InsertText("<h2>Skew detection</h2>");
                    reportGenerator.InsertImage(notNormalizedCopy?.Image, "skewimage", 0, 0);
                    reportGenerator.InsertImage(renderedHoughTransform, "skewtransform", 0, 0);
                    reportGenerator.InsertText("Detected skew angle : <b>" + hough?.Angle + "</b>");
                }

                var recognizedPlate = new RecognizedPlate();

                if (generateReport)
                {
                    reportGenerator!.InsertText("<h2>Character segmentation</h2>");
                    reportGenerator.InsertText("<div class='charsegment'>");
                    foreach (var chr in chars)
                        reportGenerator.InsertImage(
                            Photo.LinearResizeImage(chr.Image, 70, 100), "", 0, 0);

                    reportGenerator.InsertText("</div>");
                }

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
                    var errorFlags = "";

                    float widthHeightRatio = chr.PieceWidth;
                    widthHeightRatio /= chr.PieceHeight;

                    if (widthHeightRatio < Configurator.Get<double>("intelligence_minCharWidthHeightRatio")
                        || widthHeightRatio > Configurator.Get<double>("intelligence_maxCharWidthHeightRatio"))
                    {
                        errorFlags += "WHR ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (chr.PositionInPlate is null)
                        throw new ArgumentNullException(nameof(chr.PositionInPlate),
                            "Character position in plate is null");

                    if ((chr.PositionInPlate.LeftX < 2 || chr.PositionInPlate.RightX > plate.Width - 1)
                        && widthHeightRatio < 0.12)
                    {
                        errorFlags += "POS ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    var contrastCost = Math.Abs(chr.StatisticContrast - averageContrast);
                    var brightnessCost = Math.Abs(chr.StatisticAverageBrightness - averageBrightness);
                    var hueCost = Math.Abs(chr.StatisticAverageHue - averageHue);
                    var saturationCost = Math.Abs(chr.StatisticAverageSaturation - averageSaturation);
                    var heightCost = (chr.PieceHeight - averageHeight) / averageHeight;

                    if (brightnessCost > Configurator.Get<double>("intelligence_maxBrightnessCostDispersion"))
                    {
                        errorFlags += "BRI ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (contrastCost > Configurator.Get<double>("intelligence_maxContrastCostDispersion"))
                    {
                        errorFlags += "CON ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (hueCost > Configurator.Get<double>("intelligence_maxHueCostDispersion"))
                    {
                        errorFlags += "HUE ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (saturationCost > Configurator.Get<double>("intelligence_maxSaturationCostDispersion"))
                    {
                        errorFlags += "SAT ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (heightCost < -Configurator.Get<double>("intelligence_maxHeightCostDispersion"))
                    {
                        errorFlags += "HEI ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    double similarityCost = 0;
                    RecognizedCharacter? rc = null;

                    if (ok)
                    {
                        rc = _chrRecognizer.Recognize(chr);

                        if (rc.Patterns is null || rc.Patterns.Count == 0)
                            throw new InvalidOperationException(
                                "Recognized character does not have any patterns");

                        similarityCost = rc.Patterns[0].Cost;
                        if (similarityCost > Configurator.Get<double>("intelligence_maxSimilarityCostDispersion"))
                        {
                            errorFlags += "NEU ";
                            ok = false;
                            if (!generateReport)
                                continue;
                        }

                        if (ok)
                            recognizedPlate.AddCharacter(rc);
                    }

                    if (generateReport)
                    {
                        reportGenerator!.InsertText("<div class='heuristictable'>");
                        reportGenerator.InsertImage(
                            Photo.LinearResizeImage(chr.Image, chr.Width * 2, chr.Height * 2),
                            "skeleton", 0, 0);
                        reportGenerator.InsertText(
                            "<span class='name'>WHR</span><span class='value'>" + widthHeightRatio + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>HEI</span><span class='value'>" + heightCost + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>NEU</span><span class='value'>" + similarityCost + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>CON</span><span class='value'>" + contrastCost + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>BRI</span><span class='value'>" + brightnessCost + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>HUE</span><span class='value'>" + hueCost + "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>SAT</span><span class='value'>" + saturationCost + "</span>");
                        reportGenerator.InsertText("</table>");

                        if (errorFlags.Length != 0)
                            reportGenerator.InsertText("<span class='errflags'>" + errorFlags + "</span>");

                        reportGenerator.InsertText("</div>");
                    }
                }

                // If too few characters recognized, try next candidate
                if (recognizedPlate.Characters.Count < Configurator.Get<int>("intelligence_minimumChars"))
                    continue;

                LastProcessDuration = timeMeter.GetTime();
                var parsedOutput = _parser.Parse(recognizedPlate, syntaxAnalysisMode);

                if (generateReport)
                {
                    reportGenerator!.InsertText("<span class='recognized'>");
                    reportGenerator.InsertText("Recognized plate : " + parsedOutput);
                    reportGenerator.InsertText("</span>");
                    reportGenerator.Finish();
                }

                return parsedOutput;
            }
        }

        LastProcessDuration = timeMeter.GetTime();

        if (generateReport)
            reportGenerator!.Finish();

        return null;
    }
}
