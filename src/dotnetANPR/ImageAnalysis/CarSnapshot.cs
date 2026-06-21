using System.Collections.Generic;
using SkiaSharp;
using dotnetANPR.Extensions;
using dotnetANPR.Pipeline;
using dotnetANPR.Utilities;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a photograph of a car. Extracts horizontal bands that may contain a licence plate.
/// </summary>
internal sealed class CarSnapshot : Photo
{
    private readonly PipelineContext _context;
    private CarSnapshotGraph? _graphHandle;

    public CarSnapshot(SKBitmap image, PipelineContext context) : base(image)
    {
        _context = context;
    }

    public List<Band> Bands()
    {
        List<Band> response = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
            response.Add(new Band(Image.SubImage(0, peak.Left, Image.Width, peak.Diff), _context));

        return response;
    }

    public SKBitmap VerticalEdge(SKBitmap bitmap)
    {
        float[,] data = {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
        };
        return bitmap.Convolve(data);
    }

    public CarSnapshotGraph Histogram(SKBitmap bitmap)
    {
        var graph = new CarSnapshotGraph(_context.Settings);
        for (var y = 0; y < bitmap.Height; y++)
        {
            float counter = 0;
            for (var x = 0; x < bitmap.Width; x++)
                counter += GetBrightness(bitmap, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            var settings = _context.Settings;
            var distributor = new ProbabilityDistributor(
                0, 0,
                settings.CarSnapshotDistributorMargins,
                settings.CarSnapshotDistributorMargins);

            var raw = DuplicateBitmap(Image);
            var imageCopy = VerticalEdge(raw);
            raw.Dispose();

            Thresholding(imageCopy);
            _context.StageWriter?.Write("vertical-rank-filter", imageCopy);

            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(settings.CarSnapshotGraphRankFilter);
            _graphHandle.ApplyProbabilityDistributor(distributor);
            _graphHandle.FindPeaks(settings.IntelligenceNumberOfBands);
            imageCopy.Dispose();
        }

        return _graphHandle.Peaks;
    }
}
