using System;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Recognizer;
using dotnetANPR.Utilities;
using PS = dotnetANPR.Intelligence.Parser;

namespace dotnetANPR.Intelligence;

public class Intelligence
{
    private readonly CharacterRecognizer _chrRecognizer;
    private readonly PS.Parser _parser;

    private static readonly Configurator Configurator = Configurator.Instance;

    /// <summary>
    /// Gets the duration (in milliseconds) of the last call to <see cref="Recognize"/>.
    /// </summary>
    public static long LastProcessDuration { get; private set; }

    /// <summary>
    /// Initialises the recognition engine with the configured classification method and syntax parser.
    /// </summary>
    public Intelligence()
    {
        var classificationMethod = Configurator.Get<int>("intelligence_classification_method");
        _chrRecognizer = classificationMethod == 0 ? new KnnPatternClassifier() : new NeuralPatternClassifier();
        _parser = new PS.Parser();
    }

    /// <summary>
    /// Analyses a car snapshot image and attempts to recognise a licence plate.
    /// </summary>
    /// <param name="carSnapshot">The car image to analyse.</param>
    /// <param name="stageWriter">Optional writer for dumping intermediate processing stages.</param>
    /// <returns>The recognised plate text, or <c>null</c> if no plate was found.</returns>
    public string? Recognize(CarSnapshot carSnapshot, StageWriter? stageWriter = null)
    {
        var timeMeter = new TimeMeter();
        var syntaxAnalysisMode =
            (SyntaxAnalysisMode)Configurator.Get<int>("intelligence_syntaxanalysis");
        var skewDetectionMode = Configurator.Get<int>("intelligence_skewdetection");

        foreach (var band in carSnapshot.Bands(stageWriter))
        {
            foreach (var plate in band.Plates(stageWriter))
            {
                var localPlate = plate;

                if (skewDetectionMode != 0 || stageWriter != null)
                {
                    using var edgeBitmap = plate.HorizontalEdgeDetector(plate.Image);

                    var hough = new HoughTransformation(edgeBitmap.Width, edgeBitmap.Height);
                    for (var hx = 0; hx < edgeBitmap.Width; hx++)
                        for (var hy = 0; hy < edgeBitmap.Height; hy++)
                            hough.AddLine(hx, hy, Photo.GetBrightness(edgeBitmap, hx, hy));

                    if (stageWriter != null)
                    {
                        stageWriter.Write("skew-horizontal-edge", edgeBitmap);
                        using var renderedHoughTransform = hough.Render(HoughTransformation.RenderType.RenderAll,
                            HoughTransformation.ColorType.BlackAndWhite);
                        stageWriter.Write("hough-transform", renderedHoughTransform);
                    }

                    if (skewDetectionMode != 0)
                    {
                        var shearFactor = -(float)hough.Dy / hough.Dx;

                        var shearTransform = SKMatrix.CreateSkew(0, shearFactor);

                        var core = new SKBitmap(plate.Image.Width, plate.Image.Height);
                        using var canvas = new SKCanvas(core);

                        using var paint = new SKPaint
                        {
                            FilterQuality = SKFilterQuality.High,
                            IsAntialias = true
                        };

                        canvas.SetMatrix(shearTransform);
                        canvas.DrawBitmap(plate.Image, 0, 0, paint);

                        localPlate = new Plate(core);
                        stageWriter?.Write("skew-corrected", localPlate.Image);
                    }
                }

                localPlate.Normalize(stageWriter);

                var plateWHratio = localPlate.Width / (float)localPlate.Height;
                if (plateWHratio < Configurator.Get<double>("intelligence_minPlateWidthHeightRatio") ||
                    plateWHratio > Configurator.Get<double>("intelligence_maxPlateWidthHeightRatio"))
                    continue;

                var chars = localPlate.Characters();

                if (chars.Count < Configurator.Get<int>("intelligence_minimumChars") ||
                    chars.Count > Configurator.Get<int>("intelligence_maximumChars"))
                    continue;

                if (plate.CharactersWidthDispersion(chars) >
                    Configurator.Get<double>("intelligence_maxCharWidthDispersion"))
                    continue;

                foreach (var chr in chars)
                    chr.Normalize();

                var averageHeight = plate.AveragePieceHeight(chars);
                var averageContrast = plate.AveragePieceContrast(chars);
                var averageBrightness = plate.AveragePieceBrightness(chars);
                var averageHue = plate.AveragePieceHue(chars);
                var averageSaturation = plate.AveragePieceSaturation(chars);

                var recognizedPlate = new RecognizedPlate();
                foreach (var chr in chars)
                {
                    float widthHeightRatio = chr.PieceWidth;
                    widthHeightRatio /= chr.PieceHeight;
                    if (widthHeightRatio < Configurator.Get<double>("intelligence_minCharWidthHeightRatio") ||
                        widthHeightRatio > Configurator.Get<double>("intelligence_maxCharWidthHeightRatio"))
                        continue;

                    if (chr.PositionInPlate is null)
                        throw new ArgumentNullException(nameof(chr.PositionInPlate),
                            "Character position in plate is null");

                    if ((chr.PositionInPlate.LeftX < 2 || chr.PositionInPlate.RightX > plate.Width - 1) &&
                        widthHeightRatio < 0.12)
                        continue;

                    if (Math.Abs(chr.StatisticAverageBrightness - averageBrightness) >
                        Configurator.Get<double>("intelligence_maxBrightnessCostDispersion"))
                        continue;

                    if (Math.Abs(chr.StatisticContrast - averageContrast) >
                        Configurator.Get<double>("intelligence_maxContrastCostDispersion"))
                        continue;

                    if (Math.Abs(chr.StatisticAverageHue - averageHue) >
                        Configurator.Get<double>("intelligence_maxHueCostDispersion"))
                        continue;

                    if (Math.Abs(chr.StatisticAverageSaturation - averageSaturation) >
                        Configurator.Get<double>("intelligence_maxSaturationCostDispersion"))
                        continue;

                    if ((chr.PieceHeight - averageHeight) / averageHeight <
                        -Configurator.Get<double>("intelligence_maxHeightCostDispersion"))
                        continue;

                    var rc = _chrRecognizer.Recognize(chr);

                    if (rc.Patterns is null || rc.Patterns.Count == 0)
                        throw new ArgumentNullException(nameof(rc),
                            "Recognized character does not have any patterns");

                    if (rc.Patterns[0].Cost > Configurator.Get<double>("intelligence_maxSimilarityCostDispersion"))
                        continue;

                    recognizedPlate.AddCharacter(rc);
                }

                if (recognizedPlate.Characters.Count < Configurator.Get<int>("intelligence_minimumChars"))
                    continue;

                LastProcessDuration = timeMeter.GetTime();
                return _parser.Parse(recognizedPlate, syntaxAnalysisMode);
            }
        }

        LastProcessDuration = timeMeter.GetTime();
        return null;
    }
}
