using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;

namespace DotNetANPR.ImageAnalysis;

public class Plate : Photo, ICloneable
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 0, 0);
    private static readonly int NumberOfCandidates = Configurator.Instance.Get<int>("intelligence_numberOfCharacters");

    private static readonly int HorizontalDetectionType =
        Configurator.Instance.Get<int>("platehorizontalgraph_detectionType");

    private Plate? _plateCopy; // TODO refactor: remove this variable completely
    private PlateGraph? _graphHandle;

    public Plate(Bitmap image, bool isCopy = false) : base(image)
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

    object ICloneable.Clone() => new Plate(DuplicateBitmap(Image));

    public Bitmap RenderGraph()
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

            characters.Add(new Character(Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                _plateCopy!.Image.SubImage(peak.Left, 0, peak.Diff, Image.Height),
                new PositionInPlate(peak.Left, peak.Right)));
        }

        return characters;
    }

    public void HorizontalEdgeBi(Bitmap image)
    {
        var imageCopy = DuplicateBitmap(image);
        float[] data = [-1, 0, 1];
        imageCopy.ConvolutionFilter(image, data);
    }

    /**
     * Create a clone, normalize it, threshold it with coefficient 0.999.
     *
     * Function {@link Plate#cutTopBottom(Bitmap, PlateVerticalGraph)} and
     * {@link Plate#cutLeftRight(Bitmap, PlateHorizontalGraph)} crop the original image using horizontal and
     * vertical projections of the cloned image (which is thresholded).
     */
    public void Normalize()
    {
        var clone1 = (Plate)Clone();
        clone1.VerticalEdgeDetector(clone1.Image);
        var vertical = clone1.HistogramYaxis(clone1.Image);
        Image = CutTopBottom(Image, vertical);
        _plateCopy!.Image = CutTopBottom(_plateCopy.Image, vertical);
        var clone2 = (Plate)Clone();
        if (HorizontalDetectionType == 1) clone2.HorizontalEdgeDetector(clone2.Image);

        var horizontal = clone1.HistogramXaxis(clone2.Image);
        Image = CutLeftRight(Image, horizontal);
        _plateCopy.Image = CutLeftRight(_plateCopy.Image, horizontal);
    }

    public PlateGraph Histogram(Bitmap bi)
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

    public override void VerticalEdgeDetector(Bitmap source)
    {
        float[] matrix = [-1, 0, 1];
        var destination = DuplicateBitmap(source);
        destination.ConvolutionFilter(source, matrix);
    }

    public void HorizontalEdgeDetector(Bitmap source)
    {
        var destination = DuplicateBitmap(source);
        float[] matrix = [-1, -2, -1, 0, 0, 0, 1, 2, 1];
        destination.ConvolutionFilter(source, matrix);
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

    private Bitmap CutTopBottom(Bitmap origin, PlateVerticalGraph graph)
    {
        graph.ApplyProbabilityDistributor(new ProbabilityDistributor(0f, 0f, 2, 2));
        var p = graph.FindPeak(3)[0];
        return origin.SubImage(0, p.Left, Image.Width, p.Diff);
    }

    private Bitmap CutLeftRight(Bitmap origin, PlateHorizontalGraph graph)
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

    private PlateVerticalGraph HistogramYaxis(Bitmap bi)
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

    private PlateHorizontalGraph HistogramXaxis(Bitmap bi)
    {
        var graph = new PlateHorizontalGraph();
        for (var x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bi.Height; y++) counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    #endregion
}
