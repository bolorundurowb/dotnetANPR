using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a detected license plate region extracted from a band.
/// Provides normalization, character segmentation, edge detection, and statistical analysis.
/// </summary>
public class Plate : Photo, ICloneable
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 0, 0);
    private static readonly int NumberOfCandidates = AnprConfig.Instance.Intelligence.NumberOfChars;

    private static readonly int HorizontalDetectionType =
        AnprConfig.Instance.PlateHorizontalGraph.DetectionType;

    private Plate? _plateCopy;
    private PlateGraph? _graphHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plate"/> class from a bitmap.
    /// Creates a thresholded copy for segmentation unless this is itself a copy.
    /// </summary>
    /// <param name="image">The plate bitmap.</param>
    /// <param name="isCopy">If true, no thresholded copy is created.</param>
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

    /// <summary>
    /// Creates a deep copy of this plate.
    /// </summary>
    /// <returns>A new <see cref="Plate"/> instance.</returns>
    public new object Clone() => new Plate(DuplicateBitmap(Image));

    /// <summary>
    /// Renders the plate's histogram graph as a horizontal bitmap.
    /// </summary>
    /// <returns>A bitmap of the rendered graph.</returns>
    public SKBitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderHorizontally(Width, 100);
    }

    /// <summary>
    /// Segments the plate into individual character images.
    /// </summary>
    /// <returns>A list of <see cref="Character"/> objects extracted from this plate.</returns>
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
    /// Normalizes the plate by cropping top/bottom and left/right edges
    /// using vertical and horizontal edge projections.
    /// </summary>
    public void Normalize()
    {
        var clone1 = (Plate)Clone();
        clone1.Image = clone1.VerticalEdgeDetector(clone1.Image);
        var vertical = clone1.HistogramYaxis(clone1.Image);
        Image = CutTopBottom(Image, vertical);
        _plateCopy!.Image = CutTopBottom(_plateCopy.Image, vertical);
        var clone2 = (Plate)Clone();

        if (HorizontalDetectionType == 1)
            clone2.Image = clone2.HorizontalEdgeDetector(clone2.Image);

        var horizontal = clone1.HistogramXAxis(clone2.Image);
        Image = CutLeftRight(Image, horizontal);
        _plateCopy.Image = CutLeftRight(_plateCopy.Image, horizontal);
    }

    /// <summary>
    /// Computes a vertical brightness histogram (column sums) of the given bitmap.
    /// </summary>
    /// <param name="bi">The source bitmap.</param>
    /// <returns>A <see cref="PlateGraph"/> containing the histogram data.</returns>
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
    /// Applies vertical edge detection using a simple derivative kernel.
    /// </summary>
    /// <param name="source">The source bitmap.</param>
    /// <returns>A new bitmap with vertical edges detected.</returns>
    public SKBitmap VerticalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, 0, 1 } };
        return source.Convolve(matrix);
    }

    /// <summary>
    /// Applies horizontal Sobel edge detection.
    /// </summary>
    /// <param name="source">The source bitmap.</param>
    /// <returns>A new bitmap with horizontal edges detected.</returns>
    public SKBitmap HorizontalEdgeDetector(SKBitmap source)
    {
        float[,] matrix = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        return source.Convolve(matrix);
    }

    /// <summary>
    /// Computes the width dispersion of characters relative to average character width.
    /// </summary>
    /// <param name="characters">The list of characters.</param>
    /// <returns>The normalized dispersion value.</returns>
    public float CharactersWidthDispersion(List<Character> characters)
    {
        float averageDispersion = 0;
        var averageWidth = AverageCharacterWidth(characters);
        foreach (var chr in characters)
            averageDispersion += Math.Abs(averageWidth - chr.FullWidth);

        averageDispersion /= characters.Count;
        return averageDispersion / averageWidth;
    }

    /// <summary>
    /// Computes the width dispersion of character pieces relative to average piece width.
    /// </summary>
    /// <param name="characters">The list of characters.</param>
    /// <returns>The normalized dispersion value.</returns>
    public float PiecesWidthDispersion(List<Character> characters)
    {
        float averageDispersion = 0;
        var averageWidth = AveragePieceWidth(characters);
        foreach (var chr in characters)
            averageDispersion += Math.Abs(averageWidth - chr.PieceWidth);

        averageDispersion /= characters.Count;
        return averageDispersion / averageWidth;
    }

    /// <summary>
    /// Gets the average full width of the characters.
    /// </summary>
    public float AverageCharacterWidth(List<Character> characters) => (float)characters.Average(x => x.FullWidth);

    /// <summary>
    /// Gets the average piece width of the characters.
    /// </summary>
    public float AveragePieceWidth(List<Character> characters) => (float)characters.Average(x => x.PieceWidth);

    /// <summary>
    /// Gets the average hue of the character pieces.
    /// </summary>
    public float AveragePieceHue(List<Character> characters) => characters.Average(x => x.StatisticAverageHue);

    /// <summary>
    /// Gets the average contrast of the character pieces.
    /// </summary>
    public float AveragePieceContrast(List<Character> characters) => characters.Average(x => x.StatisticContrast);

    /// <summary>
    /// Gets the average brightness of the character pieces.
    /// </summary>
    public float AveragePieceBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageBrightness);

    /// <summary>
    /// Gets the average minimum brightness of the character pieces.
    /// </summary>
    public float AveragePieceMinBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticMinimumBrightness);

    /// <summary>
    /// Gets the average maximum brightness of the character pieces.
    /// </summary>
    public float AveragePieceMaxBrightness(List<Character> characters) =>
        characters.Average(x => x.StatisticMaximumBrightness);

    /// <summary>
    /// Gets the average saturation of the character pieces.
    /// </summary>
    public float AveragePieceSaturation(List<Character> characters) =>
        characters.Average(x => x.StatisticAverageSaturation);

    /// <summary>
    /// Gets the average full height of the characters.
    /// </summary>
    public float AverageCharacterHeight(List<Character> characters) => (float)characters.Average(x => x.FullHeight);

    /// <summary>
    /// Gets the average piece height of the characters.
    /// </summary>
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
