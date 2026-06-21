using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.Utilities;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a candidate licence plate region extracted from a car image.
/// Handles normalisation (cropping to bounds), character segmentation via histogram peak analysis,
/// and computes statistical properties of the contained characters.
/// </summary>
public class Plate : Photo, ICloneable
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 0, 0);
    private static readonly int NumberOfCandidates = Configurator.Instance.Get<int>("intelligence_numberOfChars");

    private static readonly int HorizontalDetectionType =
        Configurator.Instance.Get<int>("platehorizontalgraph_detectionType");

    private Plate? _plateCopy;
    private PlateGraph? _graphHandle;

    public Plate(SKBitmap image, bool isCopy = false) : base(image)
    {
        if (!isCopy)
        {
            _plateCopy = new Plate(DuplicateBitmap(Image), true);
            _plateCopy.AdaptiveThresholding();
        }
        else
        {
            _plateCopy = null;
        }
    }

    public new object Clone() => new Plate(DuplicateBitmap(Image));

    /// <summary>
    /// Segments the plate image into individual character regions using histogram peak analysis.
    /// </summary>
    /// <returns>A list of character images with their positions within the plate.</returns>
    public List<Character> Characters()
    {
        List<Character> characters = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
        {
            if (peak.Diff <= 0)
                continue;
            var positionInPlate = new PositionInPlate(peak.Left, peak.Right);
            var character = new Character(Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                _plateCopy!.Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                positionInPlate);
            characters.Add(character);
        }

        return characters;
    }

    /// <summary>
    /// Crops the plate to its bounds using vertical and horizontal edge detection and histogram analysis.
    /// </summary>
    public void Normalize(StageWriter? writer = null)
    {
        // Vertical edge — create a temporary Bitmap for edge detection only (no Plate clone needed)
        var verticalEdgeBitmap = VerticalEdgeDetector(Image);
        writer?.Write("plate-vertical-edge", verticalEdgeBitmap);

        var vertical = HistogramYaxis(verticalEdgeBitmap);
        verticalEdgeBitmap.Dispose();

        var oldImage = Image;
        Image = CutTopBottom(Image, vertical);
        oldImage.Dispose();

        var oldPlateCopyImage = _plateCopy!.Image;
        _plateCopy.Image = CutTopBottom(_plateCopy.Image, vertical);
        oldPlateCopyImage.Dispose();
        writer?.Write("plate-cut-top-bottom", Image);

        // Horizontal edge — similarly avoid cloning the Plate
        SKBitmap horizontalEdgeBitmap;
        bool disposeHorizontalEdge;
        if (HorizontalDetectionType == 1)
        {
            horizontalEdgeBitmap = HorizontalEdgeDetector(Image);
            disposeHorizontalEdge = true;
        }
        else
        {
            horizontalEdgeBitmap = Image;
            disposeHorizontalEdge = false;
        }
        writer?.Write("plate-horizontal-edge", horizontalEdgeBitmap);

        var horizontal = HistogramXAxis(horizontalEdgeBitmap);
        if (disposeHorizontalEdge)
            horizontalEdgeBitmap.Dispose();

        oldImage = Image;
        Image = CutLeftRight(Image, horizontal);
        oldImage.Dispose();

        oldPlateCopyImage = _plateCopy.Image;
        _plateCopy.Image = CutLeftRight(_plateCopy.Image, horizontal);
        oldPlateCopyImage.Dispose();
        writer?.Write("plate-normalized", Image);
    }

    /// <summary>
    /// Computes the horizontal histogram for character segmentation.
    /// </summary>
    public PlateGraph Histogram(SKBitmap bi)
    {
        var graph = new PlateGraph(this);
        for (var x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bi.Height; y++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    /// <summary>
    /// Applies vertical edge detection for finding the top and bottom bounds of the plate.
    /// </summary>
    public SKBitmap VerticalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, 0, 1 } };
        return source.Convolve(matrix);
    }

    /// <summary>
    /// Applies horizontal edge detection for finding the left and right bounds of the plate.
    /// </summary>
    public SKBitmap HorizontalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        return source.Convolve(matrix);
    }

    /// <summary>
    /// Measures the width dispersion of character regions relative to the average character width.
    /// A lower value indicates more uniform character widths.
    /// </summary>
    public float CharactersWidthDispersion(List<Character> characters)
    {
        float averageDispersion = 0;
        var averageWidth = AverageCharacterWidth(characters);
        foreach (var chr in characters)
            averageDispersion += Math.Abs(averageWidth - chr.FullWidth);

        averageDispersion /= characters.Count;
        return averageDispersion / averageWidth;
    }

    public float PiecesWidthDispersion(List<Character> characters)
    {
        float averageDispersion = 0;
        var averageWidth = AveragePieceWidth(characters);
        foreach (var chr in characters)
            averageDispersion += Math.Abs(averageWidth - chr.PieceWidth);

        averageDispersion /= characters.Count;
        return averageDispersion / averageWidth;
    }

    public float AverageCharacterWidth(List<Character> characters) => (float)characters.Average(x => x.FullWidth);

    public float AveragePieceWidth(List<Character> characters) => (float)characters.Average(x => x.PieceWidth);

    public float AveragePieceHue(List<Character> characters) => characters.Average(x => x.StatisticAverageHue);

    public float AveragePieceContrast(List<Character> characters) => characters.Average(x => x.StatisticContrast);

    public float AveragePieceBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageBrightness);

    public float AveragePieceMinBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticMinimumBrightness);

    public float AveragePieceMaxBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticMaximumBrightness);

    public float AveragePieceSaturation(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageSaturation);

    public float AverageCharacterHeight(List<Character> characters) => (float)characters.Average(x => x.FullHeight);

    public float AveragePieceHeight(List<Character> characters) => (float)characters.Average(x => x.PieceHeight);

    #region Private Helpers

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            if (_plateCopy is null)
                throw new ArgumentNullException(nameof(_plateCopy), "PlateCopy cannot be null");

            _graphHandle = Histogram(_plateCopy.Image);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);
        }

        return _graphHandle.Peaks;
    }

    private SKBitmap CutTopBottom(SKBitmap origin, PlateVerticalGraph graph)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var p = graph.FindPeak(3)[0];
        return origin.SubImage(0, p.Left, Image.Width, p.Diff);
    }

    private SKBitmap CutLeftRight(SKBitmap origin, PlateHorizontalGraph graph)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var peaks = graph.FindPeak();

        if (peaks.Count != 0)
        {
            var peak = peaks[0];
            return origin.SubImage(peak.Left, 0, peak.Diff, Image.Height);
        }

        return origin;
    }

    private PlateVerticalGraph HistogramYaxis(SKBitmap bi)
    {
        var graph = new PlateVerticalGraph();
        for (var y = 0; y < bi.Height; y++)
        {
            float counter = 0;
            for (var x = 0; x < bi.Width; x++) counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    private PlateHorizontalGraph HistogramXAxis(SKBitmap bi)
    {
        var graph = new PlateHorizontalGraph();
        for (var x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bi.Height; y++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    #endregion
}