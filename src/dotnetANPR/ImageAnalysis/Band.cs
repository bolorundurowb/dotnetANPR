using System;
using System.Collections.Generic;
using SkiaSharp;
using dotnetANPR.Extensions;
using dotnetANPR.Pipeline;
using dotnetANPR.Utilities;

namespace dotnetANPR.ImageAnalysis;

internal sealed class Band : Photo
{
    private readonly PipelineContext _context;
    private BandGraph? _graphHandle;

    public Band(SKBitmap image, PipelineContext context) : base(image) => _context = context;

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            var settings = _context.Settings;
            var distributor = new ProbabilityDistributor(0, 0, 25, 25);
            var image = DuplicateBitmap(Image);
            FullEdgeDetector(image);
            _context.StageWriter?.Write("horizontal-rank-filter", image);

            _graphHandle = Histogram(image);
            _graphHandle.RankFilter(Image.Height);
            _graphHandle.ApplyProbabilityDistributor(distributor);
            _graphHandle.FindPeaks(settings.IntelligenceNumberOfPlates);
            image.Dispose();
        }

        return _graphHandle.Peaks;
    }

    public List<Plate> Plates()
    {
        List<Plate> response = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
            response.Add(new Plate(Image.SubImage(peak.Left, 0, peak.Diff, Image.Height), _context));

        return response;
    }

    public BandGraph Histogram(SKBitmap bitmap)
    {
        var graph = new BandGraph(this, _context.Settings);
        for (var x = 0; x < bitmap.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bitmap.Height; y++)
                counter += GetBrightness(bitmap, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    public static void FullEdgeDetector(SKBitmap source)
    {
        float[,] verticalMatrix =
        {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        float[,] horizontalMatrix =
        {
            { -1, -2, -1 },
            { 0, 0, 0 },
            { 1, 2, 1 }
        };

        var i1 = source.Convolve(verticalMatrix);
        var i2 = source.Convolve(horizontalMatrix);
        var width = source.Width;
        var height = source.Height;

        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var sum = GetBrightness(i1, x, y);
                sum += GetBrightness(i2, x, y);
                SetBrightness(source, x, y, Math.Min(1f, sum));
            }

        i1.Dispose();
        i2.Dispose();
    }
}
