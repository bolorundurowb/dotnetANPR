using System;
using System.Collections.Generic;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Intelligence;
using dotnetANPR.Recognizer;
using dotnetANPR.Utilities;
using SkiaSharp;

namespace dotnetANPR.Pipeline;

internal sealed class PlateRecognitionPipeline
{
    private readonly ICharacterRecognizer _recognizer;
    private readonly ISyntaxCorrector _syntaxCorrector;
    private readonly PlateValidator _plateValidator = new();
    private readonly CharacterValidator _characterValidator = new();
    private readonly PlateScorer _plateScorer = new();

    public PlateRecognitionPipeline(ICharacterRecognizer recognizer, ISyntaxCorrector syntaxCorrector)
    {
        _recognizer = recognizer;
        _syntaxCorrector = syntaxCorrector;
    }

    public (PlateCandidate? Best, List<PlateCandidate> All) Process(
        CarSnapshot snapshot,
        PipelineContext context)
    {
        var settings = context.Settings;
        var syntaxMode = (SyntaxAnalysisMode)settings.IntelligenceSyntaxAnalysis;
        var candidates = new List<PlateCandidate>();
        var bandIndex = 0;

        foreach (var band in snapshot.Bands())
        {
            try
            {
                var plateIndex = 0;
                foreach (var plate in band.Plates())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    ProcessPlate(plate, bandIndex, plateIndex, settings, context, candidates);
                    plateIndex++;
                }
            }
            finally
            {
                band.Dispose();
            }

            bandIndex++;
        }

        PlateCandidate? best = null;
        foreach (var candidate in candidates)
        {
            candidate.Score = _plateScorer.Score(candidate, settings);
            if (best is null || candidate.Score > best.Score)
                best = candidate;
        }

        if (best is not null)
        {
            best.Score = _plateScorer.Score(best, settings);
            var corrected = _syntaxCorrector.Correct(best.RecognizedPlate, syntaxMode);
            // Store corrected text via a wrapper - we'll use RecognitionResult builder in AnprEngine
        }

        return (best, candidates);
    }

    private void ProcessPlate(
        Plate plate,
        int bandIndex,
        int plateIndex,
        AnprSettings settings,
        PipelineContext context,
        List<PlateCandidate> candidates)
    {
        var localPlate = plate;
        Plate? skewPlate = null;

        try
        {
            var dumpSkew = context.StageWriter is not null && context.Settings.IntelligenceSkewDetection != 0;
            var enableSkew = context.Settings.IntelligenceSkewDetection != 0;
            // Skew gating handled via RecognitionOptions on context - store flags on PipelineContext

            if (context.EnableSkewDiagnostics || context.EnableSkewCorrection)
            {
                using var edgeBitmap = plate.HorizontalEdgeDetector(plate.Image);
                var hough = new HoughTransformation(edgeBitmap.Width, edgeBitmap.Height);
                for (var hx = 0; hx < edgeBitmap.Width; hx++)
                    for (var hy = 0; hy < edgeBitmap.Height; hy++)
                        hough.AddLine(hx, hy, Photo.GetBrightness(edgeBitmap, hx, hy));

                if (context.StageWriter is not null && context.EnableSkewDiagnostics)
                {
                    context.StageWriter.Write("skew-horizontal-edge", edgeBitmap);
                    using var rendered = hough.Render(
                        HoughTransformation.RenderType.RenderAll,
                        HoughTransformation.ColorType.BlackAndWhite);
                    context.StageWriter.Write("hough-transform", rendered);
                }

                if (context.EnableSkewCorrection)
                {
                    var shearFactor = -(float)hough.Dy / hough.Dx;
                    var shearTransform = SKMatrix.CreateSkew(0, shearFactor);
                    var core = new SKBitmap(plate.Image.Width, plate.Image.Height);
                    using var canvas = new SKCanvas(core);
                    using var paint = new SKPaint { FilterQuality = SKFilterQuality.High, IsAntialias = true };
                    canvas.SetMatrix(shearTransform);
                    canvas.DrawBitmap(plate.Image, 0, 0, paint);
                    skewPlate = new Plate(core, context);
                    localPlate = skewPlate;
                    context.StageWriter?.Write("skew-corrected", localPlate.Image);
                }
            }

            localPlate.Normalize();
            if (!_plateValidator.IsPlateShapeValid(localPlate, settings))
                return;

            var chars = localPlate.Characters();
            if (!_plateValidator.IsCharacterCountValid(chars.Count, settings))
            {
                DisposeCharacters(chars);
                return;
            }

            if (!_plateValidator.IsWidthDispersionValid(plate, chars, settings))
            {
                DisposeCharacters(chars);
                return;
            }

            foreach (var chr in chars)
                chr.Normalize(settings);

            var averageHeight = plate.AveragePieceHeight(chars);
            var averageContrast = plate.AveragePieceContrast(chars);
            var averageBrightness = plate.AveragePieceBrightness(chars);
            var averageHue = plate.AveragePieceHue(chars);
            var averageSaturation = plate.AveragePieceSaturation(chars);

            var recognizedPlate = new RecognizedPlate();
            foreach (var chr in chars)
            {
                if (!_characterValidator.IsValid(
                        chr, plate, averageHeight, averageContrast, averageBrightness,
                        averageHue, averageSaturation, settings))
                    continue;

                var rc = _recognizer.Recognize(chr, settings);
                if (rc.Patterns is null || rc.Patterns.Count == 0)
                    continue;

                if (!_characterValidator.IsClassificationCostValid(rc.Patterns[0].Cost, settings))
                    continue;

                recognizedPlate.AddCharacter(rc);
            }

            DisposeCharacters(chars);

            if (!_plateValidator.HasMinimumRecognizedChars(recognizedPlate, settings))
                return;

            candidates.Add(new PlateCandidate
            {
                RecognizedPlate = recognizedPlate,
                BandIndex = bandIndex,
                PlateIndex = plateIndex,
                PlateWidthHeightRatio = localPlate.Width / (float)localPlate.Height,
            });
        }
        finally
        {
            if (skewPlate is not null && !ReferenceEquals(skewPlate, plate))
                skewPlate.Dispose();
            plate.Dispose();
        }
    }

    private static void DisposeCharacters(List<Character> chars)
    {
        foreach (var chr in chars)
            chr.Dispose();
    }
}
