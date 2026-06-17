using System.Collections.Generic;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a snapshot image of a car. Entry point for the ANPR pipeline.
/// Provides vertical edge detection, thresholding, histogram analysis, and band extraction.
/// </summary>
public class CarSnapshot : Photo
{
    private static readonly int DistributorMargins =
        AnprConfig.Instance.CarSnapshot.DistributorMargins;

    private static readonly int CarSnapshotGraphRankFilter =
        AnprConfig.Instance.CarSnapshot.GraphRankFilter;

    private static readonly int NumberOfCandidates =
        AnprConfig.Instance.Intelligence.NumberOfBands;

    private static readonly ProbabilityDistributor Distributor =
        new(0, 0, DistributorMargins, DistributorMargins);

    private CarSnapshotGraph? _graphHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarSnapshot"/> class from a bitmap.
    /// </summary>
    /// <param name="image">The car snapshot bitmap.</param>
    public CarSnapshot(SKBitmap image) : base(image) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CarSnapshot"/> class by loading an image from a file.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    public CarSnapshot(string filePath) : base(SKBitmap.Decode(filePath)) { }

    /// <summary>
    /// Renders the car snapshot's histogram graph as a vertical bitmap.
    /// </summary>
    /// <returns>A bitmap of the rendered graph.</returns>
    public SKBitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderVertically(100, Height);
    }

    /// <summary>
    /// Extracts candidate horizontal bands from this car snapshot.
    /// </summary>
    /// <returns>A list of <see cref="Band"/> objects representing candidate plate regions.</returns>
    public List<Band> Bands()
    {
        List<Band> response = [];
        var peaks = ComputeGraph();
        foreach (var peak in peaks)
            response.Add(new Band(Image.SubImage(0, peak.Left, Image.Width, peak.Diff)));

        return response;
    }

    /// <summary>
    /// Applies a vertical edge detection filter to the given bitmap.
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <returns>A new bitmap with vertical edges detected.</returns>
    public SKBitmap VerticalEdge(SKBitmap bitmap)
    {
        float[,] data =
        {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
        };
        return bitmap.Convolve(data);
    }

    /// <summary>
    /// Computes a horizontal brightness histogram of the given bitmap.
    /// </summary>
    /// <param name="bitmap">The source bitmap.</param>
    /// <returns>A <see cref="CarSnapshotGraph"/> containing the histogram data.</returns>
    public CarSnapshotGraph Histogram(SKBitmap bitmap)
    {
        var graph = new CarSnapshotGraph();
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
            var imageCopy = DuplicateBitmap(Image);
            imageCopy = VerticalEdge(imageCopy);
            Thresholding(imageCopy);

            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(CarSnapshotGraphRankFilter);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);
        }

        return _graphHandle.Peaks;
    }
}
