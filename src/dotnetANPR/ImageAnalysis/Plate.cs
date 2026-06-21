using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.Pipeline;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a candidate licence plate region extracted from a car image.
/// </summary>
internal sealed class Plate : Photo, ICloneable
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 0, 0);

    private readonly PipelineContext _context;
    private Plate? _plateCopy;
    private PlateGraph? _graphHandle;

    public Plate(SKBitmap image, PipelineContext context, bool isCopy = false) : base(image)
    {
        _context = context;
        if (!isCopy)
        {
            _plateCopy = new Plate(DuplicateBitmap(Image), context, true);
            _plateCopy.AdaptiveThresholding(context.Settings.PhotoAdaptiveThresholdingRadius);
        }
    }

    public new object Clone() => new Plate(DuplicateBitmap(Image), _context);

    public List<Character> Characters()
    {
        List<Character> characters = [];
        var peaks = ComputeGraph();
        var settings = _context.Settings;
        foreach (var peak in peaks)
        {
            if (peak.Diff <= 0)
                continue;
            var positionInPlate = new PositionInPlate(peak.Left, peak.Right);
            var character = new Character(
                Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                _plateCopy!.Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                positionInPlate,
                settings);
            characters.Add(character);
        }

        return characters;
    }

    public void Normalize()
    {
        var writer = _context.StageWriter;
        var settings = _context.Settings;

        var verticalEdgeBitmap = VerticalEdgeDetector(Image);
        writer?.Write("plate-vertical-edge", verticalEdgeBitmap);

        var vertical = HistogramYaxis(verticalEdgeBitmap, settings);
        verticalEdgeBitmap.Dispose();

        var oldImage = Image;
        Image = CutTopBottom(Image, vertical, settings);
        oldImage.Dispose();

        var oldPlateCopyImage = _plateCopy!.Image;
        _plateCopy.Image = CutTopBottom(_plateCopy.Image, vertical, settings);
        oldPlateCopyImage.Dispose();
        writer?.Write("plate-cut-top-bottom", Image);

        SKBitmap horizontalEdgeBitmap;
        var disposeHorizontalEdge = false;
        if (settings.PlateHorizontalGraphDetectionType == 1)
        {
            horizontalEdgeBitmap = HorizontalEdgeDetector(Image);
            disposeHorizontalEdge = true;
        }
        else
        {
            horizontalEdgeBitmap = Image;
        }

        writer?.Write("plate-horizontal-edge", horizontalEdgeBitmap);

        var horizontal = HistogramXAxis(horizontalEdgeBitmap, settings);
        if (disposeHorizontalEdge)
            horizontalEdgeBitmap.Dispose();

        oldImage = Image;
        Image = CutLeftRight(Image, horizontal, settings);
        oldImage.Dispose();

        oldPlateCopyImage = _plateCopy.Image;
        _plateCopy.Image = CutLeftRight(_plateCopy.Image, horizontal, settings);
        oldPlateCopyImage.Dispose();
        writer?.Write("plate-normalized", Image);
    }

    public PlateGraph Histogram(SKBitmap bi) =>
        new(this, _context.Settings);

    public SKBitmap VerticalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, 0, 1 } };
        return source.Convolve(matrix);
    }

    public SKBitmap HorizontalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        return source.Convolve(matrix);
    }

    public float CharactersWidthDispersion(List<Character> characters)
    {
        float averageDispersion = 0;
        var averageWidth = AverageCharacterWidth(characters);
        foreach (var chr in characters)
            averageDispersion += Math.Abs(averageWidth - chr.FullWidth);

        averageDispersion /= characters.Count;
        return averageDispersion / averageWidth;
    }

    public float AverageCharacterWidth(List<Character> characters) =>
        (float)characters.Average(x => x.FullWidth);

    public float AveragePieceWidth(List<Character> characters) =>
        (float)characters.Average(x => x.PieceWidth);

    public float AveragePieceHue(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageHue);

    public float AveragePieceContrast(List<Character> characters) =>
        characters.Average(x => x.StatisticContrast);

    public float AveragePieceBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageBrightness);

    public float AveragePieceSaturation(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageSaturation);

    public float AveragePieceHeight(List<Character> characters) =>
        (float)characters.Average(x => x.PieceHeight);

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            if (_plateCopy is null)
                throw new InvalidOperationException("Plate copy cannot be null");

            _graphHandle = BuildPlateGraph(_plateCopy.Image);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(_context.Settings.IntelligenceNumberOfChars);
        }

        return _graphHandle.Peaks;
    }

    private PlateGraph BuildPlateGraph(SKBitmap bi)
    {
        var graph = new PlateGraph(this, _context.Settings);
        for (var x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bi.Height; y++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    private SKBitmap CutTopBottom(SKBitmap origin, PlateVerticalGraph graph, AnprSettings settings)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var p = graph.FindPeak(3, settings)[0];
        return origin.SubImage(0, p.Left, Image.Width, p.Diff);
    }

    private SKBitmap CutLeftRight(SKBitmap origin, PlateHorizontalGraph graph, AnprSettings settings)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var peaks = graph.FindPeak(settings);

        if (peaks.Count != 0)
        {
            var peak = peaks[0];
            return origin.SubImage(peak.Left, 0, peak.Diff, Image.Height);
        }

        return origin;
    }

    private PlateVerticalGraph HistogramYaxis(SKBitmap bi, AnprSettings settings)
    {
        var graph = new PlateVerticalGraph(settings);
        for (var y = 0; y < bi.Height; y++)
        {
            float counter = 0;
            for (var x = 0; x < bi.Width; x++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    private PlateHorizontalGraph HistogramXAxis(SKBitmap bi, AnprSettings settings)
    {
        var graph = new PlateHorizontalGraph(settings);
        for (var x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bi.Height; y++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }
}
