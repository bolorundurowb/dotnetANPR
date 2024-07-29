using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
using PS = DotNetANPR.Intelligence.Parser;

namespace DotNetANPR.Intelligence;

public class Intelligence
{
    private static CharacterRecognizer _chrRecog;
    private static PS.Parser _parser;

    private static readonly Configurator _configurator = Configurator.Instance;
    public static long LastProcessDuration { get; private set; } = 0L;

    public Intelligence()
    {
        var classificationMethod = _configurator.Get<int>("intelligence_classification_method");
        if (classificationMethod == 0)
            _chrRecog = new KnnPatternClassifier();
        else
            _chrRecog = new NeuralPatternClassifier();

        _parser = new PS.Parser();
    }

    public string? Recognize(CarSnapshot carSnapshot, ReportGenerator? reportGenerator = null)
    {
        var generateReport = reportGenerator != null;
        var time = new TimeMeter();
        var syntaxAnalysisMode =
            (SyntaxAnalysisMode)_configurator.Get<int>("intelligence_syntaxanalysis");
        var skewDetectionMode = _configurator.Get<int>("intelligence_skewdetection");
        if (generateReport)
        {
            reportGenerator.InsertText("<h1>Automatic Number Plate Recognition Report</h1>");
            reportGenerator.InsertText("<span>Image width: " + carSnapshot.Width + " px</span>");
            reportGenerator.InsertText("<span>Image height: " + carSnapshot.Height + " px</span>");
            reportGenerator.InsertText("<h2>Vertical and Horizontal plate projection</h2>");
            reportGenerator.InsertImage(carSnapshot.RenderGraph(), "snapshotgraph", 0, 0);
            reportGenerator.InsertImage(carSnapshot.GetBitmapWithAxes(), "snapshot", 0, 0);
        }

        foreach (var b in carSnapshot.Bands())
        {
            if (generateReport)
            {
                reportGenerator.InsertText("<div class='bandtxt'><h4>Band<br></h4>");
                reportGenerator.InsertImage(b.Image, "bandsmall", 250, 30);
                reportGenerator.InsertText("<span>Band width : " + b.Width + " px</span>");
                reportGenerator.InsertText("<span>Band height : " + b.Height + " px</span>");
                reportGenerator.InsertText("</div>");
            }

            foreach (var plate in b.Plates())
            {
                var localPlate = plate;
                if (generateReport)
                {
                    reportGenerator.InsertText("<div class='platetxt'><h4>Plate<br></h4>");
                    reportGenerator.InsertImage(localPlate.Image, "platesmall", 120, 30);
                    reportGenerator.InsertText("<span>Plate width : " + localPlate.Width + " px</span>");
                    reportGenerator.InsertText("<span>Plate height : " + localPlate.Height + " px</span>");
                    reportGenerator.InsertText("</div>");
                }

                // Skew-related
                Plate notNormalizedCopy;
                Bitmap renderedHoughTransform;
                HoughTransformation hough;
                // detection is done either: 1. because of the report generator 2. because of skew detection
                if (generateReport || skewDetectionMode != 0)
                {
                    notNormalizedCopy = (Plate)plate.Clone();
                    notNormalizedCopy.HorizontalEdgeDetector(notNormalizedCopy.Image);
                    hough = notNormalizedCopy.GetHoughTransformation();
                    renderedHoughTransform = hough.Render(HoughTransformation.RenderType.RenderAll,
                        HoughTransformation.ColorType.BlackAndWhite);
                }

                if (skewDetectionMode != 0)
                {
                    // skew detection on
                    // Calculate the shear factor
                    var shearFactor = -(double)hough.Dy / hough.Dx;

                    // Create a shear transform
                    var shearTransform = new Matrix();
                    shearTransform.Shear(0, (float)shearFactor);

                    // Create a blank image with the same dimensions as the plate image
                    var core = new Bitmap(plate.Image.Width, plate.Image.Height);

                    using (var g = Graphics.FromImage(core))
                    {
                        // Apply the shear transform to the image
                        g.Transform = shearTransform;
                        g.DrawImage(plate.Image, new Point(0, 0));
                    }

                    // Update the plate with the new image
                    localPlate = new Plate(core);
                }

                localPlate.Normalize();

                var plateWHratio = localPlate.Width / (float)localPlate.Height;
                if ((plateWHratio < _configurator.Get<double>("intelligence_minPlateWidthHeightRatio")) || (
                        plateWHratio > _configurator.Get<double>("intelligence_maxPlateWidthHeightRatio")))
                    continue;

                var chars = localPlate.Characters();

                // heuristic analysis of the plate (uniformity and character count)
                if ((chars.Count < _configurator.Get<int>("intelligence_minimumChars")) || (chars.Count
                        > _configurator.Get<int>("intelligence_maximumChars")))
                    continue;

                if (plate.CharactersWidthDispersion(chars) > _configurator
                        .Get<double>("intelligence_maxCharWidthDispersion"))
                    continue;

                // Plate accepted; normalize and begin character heuristic
                if (generateReport)
                {
                    reportGenerator.InsertText("<h2>Detected band</h2>");
                    reportGenerator.InsertImage(b.GetBitmapWithAxes(), "band", 0, 0);
                    reportGenerator.InsertImage(b.RenderGraph(), "bandgraph", 0, 0);
                    reportGenerator.InsertText("<h2>Detected plate</h2>");
                    var plateCopy = (Plate)plate.Clone();
                    plateCopy.LinearResize(450, 90);
                    reportGenerator.InsertImage(plateCopy.GetBitmapWithAxes(), "plate", 0, 0);
                    reportGenerator.InsertImage(plateCopy.RenderGraph(), "plategraph", 0, 0);
                }

                // Skew-related
                if (generateReport)
                {
                    reportGenerator.InsertText("<h2>Skew detection</h2>");
                    reportGenerator.InsertImage(notNormalizedCopy.Image, "skewimage", 0, 0);
                    reportGenerator.InsertImage(renderedHoughTransform, "skewtransform", 0, 0);
                    reportGenerator.InsertText("Detected skew angle : <b>" + hough.Angle + "</b>");
                }

                var recognizedPlate = new RecognizedPlate();
                if (generateReport)
                {
                    reportGenerator.InsertText("<h2>Character segmentation</h2>");
                    reportGenerator.InsertText("<div class='charsegment'>");
                    foreach (var chr in chars) 
                        reportGenerator.InsertImage(Photo.LinearResizeImage(chr.Image, 70, 100), "", 0, 0);

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
                    // heuristic analysis of individual characters
                    var ok = true;
                    var errorFlags = "";
                    // when normalizing the chars, keep the width/height ratio in mind
                    float widthHeightRatio = (chr.PieceWidth);
                    widthHeightRatio /= (chr.PieceHeight);
                    if ((widthHeightRatio < _configurator.Get<double>("intelligence_minCharWidthHeightRatio")) || (
                            widthHeightRatio > _configurator
                                .Get<double>("intelligence_maxCharWidthHeightRatio")))
                    {
                        errorFlags += "WHR ";
                        ok = false;
                        if (!generateReport) 
                            continue;
                    }

                    if (((chr.PositionInPlate.LeftX < 2) || (chr.PositionInPlate.RightX > (plate.Width - 1))) && (
                            widthHeightRatio < 0.12))
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
                    if (brightnessCost > _configurator.Get<double>("intelligence_maxBrightnessCostDispersion"))
                    {
                        errorFlags += "BRI ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (contrastCost > _configurator.Get<double>("intelligence_maxContrastCostDispersion"))
                    {
                        errorFlags += "CON ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (hueCost > _configurator.Get<double>("intelligence_maxHueCostDispersion"))
                    {
                        errorFlags += "HUE ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (saturationCost > _configurator.Get<double>("intelligence_maxSaturationCostDispersion"))
                    {
                        errorFlags += "SAT ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    if (heightCost < -_configurator.Get<double>("intelligence_maxHeightCostDispersion"))
                    {
                        errorFlags += "HEI ";
                        ok = false;
                        if (!generateReport)
                            continue;
                    }

                    double similarityCost = 0;
                    RecognizedCharacter rc ;
                    if (ok)
                    {
                        rc = _chrRecog.Recognize(chr);
                        similarityCost = rc.Patterns[0].Cost;
                        if (similarityCost > _configurator.Get<double>("intelligence_maxSimilarityCostDispersion"))
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
                        reportGenerator.InsertText("<div class='heuristictable'>");
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
                        reportGenerator.InsertText("<span class='name'>HUE</span><span class='value'>" + hueCost +
                                                   "</span>");
                        reportGenerator.InsertText(
                            "<span class='name'>SAT</span><span class='value'>" + saturationCost + "</span>");
                        reportGenerator.InsertText("</table>");
                        
                        if (errorFlags.Length != 0) 
                            reportGenerator.InsertText("<span class='errflags'>" + errorFlags + "</span>");

                        reportGenerator.InsertText("</div>");
                    }
                }

                // if too few characters recognized, get next candidate
                if (recognizedPlate.Characters.Count < _configurator.Get<int>("intelligence_minimumChars")) 
                    continue;

                LastProcessDuration = time.GetTime();
                var parsedOutput = _parser.Parse(recognizedPlate, syntaxAnalysisMode);
                if (generateReport)
                {
                    reportGenerator.InsertText("<span class='recognized'>");
                    reportGenerator.InsertText("Recognized plate : " + parsedOutput);
                    reportGenerator.InsertText("</span>");
                    reportGenerator.Finish();
                }

                return parsedOutput;
            }
        }

        // TODO failed!
        LastProcessDuration = time.GetTime();
        
        if (generateReport) 
            reportGenerator!.Finish();

        return null;
    }
}
