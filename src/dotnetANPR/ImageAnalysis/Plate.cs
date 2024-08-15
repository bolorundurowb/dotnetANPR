using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using ImageMagick;

namespace DotNetANPR.ImageAnalysis;

public class Plate : Photo, ICloneable
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 0, 0);
    private static readonly int NumberOfCandidates = Configurator.Instance.Get<int>("intelligence_numberOfChars");

    private static readonly int HorizontalDetectionType =
        Configurator.Instance.Get<int>("platehorizontalgraph_detectionType");

    private Plate? _plateCopy; // TODO refactor: remove this variable completely
    private PlateGraph? _graphHandle;

    public Plate(MagickImage image, bool isCopy = false) : base(image)
    {
        if (!isCopy)
        {
            _plateCopy = new Plate(DuplicateMagickImage(Image), true);
            _plateCopy.AdaptiveThresholding();
        }
        else
        {
            _plateCopy = null;
        }
    }

    public new object Clone() => new Plate(DuplicateMagickImage(Image));

    public MagickImage RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderHorizontally(Width, 100);
    }

    public List<Character> Characters()
    {
        List<Character> characters = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
        {
            // Cut from the original image of the plate and save to a vector.
            // ATTENTION: Cutting from original,
            // we have to apply an inverse transformation to the coordinates calculated from imageCopy
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

    public PlateGraph Histogram(MagickImage bi)
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

    public MagickImage VerticalEdgeDetector(MagickImage source)
    {
        float[,] matrix = { { -1, 0, 1 } };
        return source.Convolve(matrix);
    }

    public MagickImage HorizontalEdgeDetector(MagickImage source)
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

    private MagickImage CutTopBottom(MagickImage origin, PlateVerticalGraph graph)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var p = graph.FindPeak(3)[0];
        return origin.SubImage(0, p.Left, Image.Width, p.Diff);
    }

    private MagickImage CutLeftRight(MagickImage origin, PlateHorizontalGraph graph)
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

    private PlateVerticalGraph HistogramYaxis(MagickImage bi)
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

    private PlateHorizontalGraph HistogramXAxis(MagickImage bi)
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
