using System;
using System.Collections.Generic;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a horizontal band extracted from a car snapshot image.
/// Contains methods for edge detection, histogram computation, and plate extraction.
/// </summary>
/// <param name="image">The bitmap representing this band.</param>
public class Band(SKBitmap image) : Photo(image)
{
    private static readonly ProbabilityDistributor Distributor = new(0, 0, 25, 25);

    private static readonly int NumberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfPlates");

    private BandGraph? _graphHandle;

    /// <summary>
    /// Renders the band's histogram graph as a horizontal bitmap.
    /// </summary>
    /// <returns>A bitmap of the rendered graph.</returns>
    public SKBitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderHorizontally(Width, 100);
    }

    /// <summary>
    /// Extracts candidate license plates from this band.
    /// </summary>
    /// <returns>A list of <see cref="Plate"/> objects extracted from this band.</returns>
    public List<Plate> Plates()
    {
        List<Plate> response = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
            response.Add(new Plate(Image.SubImage(peak.Left, 0, peak.Diff, Image.Height)));

        return response;
    }

    /// <summary>
    /// Computes a vertical brightness histogram for the given bitmap.
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <returns>A <see cref="BandGraph"/> containing the histogram data.</returns>
    public BandGraph Histogram(SKBitmap bitmap)
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

    /// <summary>
    /// Applies a full (vertical + horizontal) Sobel edge detector to the source bitmap in-place.
    /// The combined edge magnitude is written back into the source.
    /// </summary>
    /// <param name="source">The bitmap to apply edge detection to.</param>
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
    }

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle == null)
        {
            var imageCopy = DuplicateBitmap(Image);
            FullEdgeDetector(imageCopy);

            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(Image.Height);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);
        }

        return _graphHandle.Peaks;
    }
}
