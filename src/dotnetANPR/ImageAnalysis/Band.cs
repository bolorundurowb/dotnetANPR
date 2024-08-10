using System;
using System.Collections.Generic;
using System.Drawing;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;

namespace DotNetANPR.ImageAnalysis;

public class Band(Bitmap image) : Photo(image)
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 25, 25);

    private static readonly int NumberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfPlates");

    private BandGraph? _graphHandle;

    public Bitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderHorizontally(Width, 100);
    }

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            var image = DuplicateBitmap(Image);
            FullEdgeDetector(image);

            _graphHandle = Histogram(image);
            _graphHandle.RankFilter(Image.Height);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);
        }

        return _graphHandle.Peaks;
    }

    public List<Plate> Plates()
    {
        List<Plate> response = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
            // Cut from the original image of the plate and save to a vector.
            // ATTENTION: Cutting from original,
            // we have to apply an inverse transformation to the coordinates calculated from imageCopy
            response.Add(new Plate(Image.SubImage(peak.Left, 0, peak.Diff, Image.Height)));

        return response;
    }

    public BandGraph Histogram(Bitmap bitmap)
    {
        var graph = new BandGraph(this);
        for (var x = 0; x < bitmap.Width; x++)
        {
            float counter = 0;
            for (var y = 0; y < bitmap.Height; y++)
                counter += GetBrightness(bitmap, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }

    public static void FullEdgeDetector(Bitmap source)
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

        // Apply vertical edge detection
        var i1 = source.Convolve(verticalMatrix);

        // Apply horizontal edge detection
        var i2 = source.Convolve(horizontalMatrix);

        // Combine edge detection results
        var width = source.Width;
        var height = source.Height;

        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var sum = GetBrightness(i1, x, y);
                sum += GetBrightness(i2, x, y);
                SetBrightness(source, x, y, Math.Min(1f, sum));
            }
    }
}