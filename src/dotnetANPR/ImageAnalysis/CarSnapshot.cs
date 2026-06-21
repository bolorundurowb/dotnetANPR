using System.Collections.Generic;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.Utilities;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a photograph of a car. Extracts horizontal bands that may contain a licence plate
/// using vertical edge detection and peak analysis on the image histogram.
/// </summary>
public class CarSnapshot(SKBitmap image) : Photo(image)
{
    private static readonly int DistributorMargins =
        Configurator.Instance.Get<int>("carsnapshot_distributormargins");

    private static readonly int CarSnapshotGraphRankFilter =
        Configurator.Instance.Get<int>("carsnapshot_graphrankfilter");

    private static readonly int NumberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfBands");

    private static readonly ProbabilityDistributor Distributor =
        new(0, 0, DistributorMargins, DistributorMargins);

    private CarSnapshotGraph? _graphHandle;

    /// <summary>
    /// Extracts candidate horizontal bands from the car image that may contain a licence plate.
    /// </summary>
    /// <param name="writer">Optional stage writer for diagnostic output.</param>
    /// <returns>List of image bands to analyse for plate content.</returns>
    public List<Band> Bands(StageWriter? writer = null)
    {
        List<Band> response = [];
        var peaks = ComputeGraph(writer);
        foreach (var peak in peaks)
            response.Add(new Band(Image.SubImage(0, peak.Left, Image.Width, peak.Diff)));

        return response;
    }

    /// <summary>
    /// Applies a vertical edge detection convolution kernel to the image.
    /// </summary>
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

    private List<Peak> ComputeGraph(StageWriter? writer = null)
    {
        if (_graphHandle == null)
        {
            var raw = DuplicateBitmap(Image);
            var imageCopy = VerticalEdge(raw); // Convolve returns a new Bitmap
            raw.Dispose();                     // release the DuplicateBitmap

            Thresholding(imageCopy);
            writer?.Write("vertical-rank-filter", imageCopy);

            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(CarSnapshotGraphRankFilter);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);
            imageCopy.Dispose(); // histogram is built; release the working bitmap
        }

        return _graphHandle.Peaks;
    }
}
