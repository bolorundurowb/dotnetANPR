using System.Collections.Generic;
using System.Drawing;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.Utilities;

namespace dotnetANPR.ImageAnalysis;

public class CarSnapshot(Bitmap image) : Photo(image)
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

    public Bitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderVertically(100, Height);
    }

    public List<Band> Bands(StageWriter? writer = null)
    {
        List<Band> response = [];
        var peaks = ComputeGraph(writer);
        foreach (var peak in peaks)
        {
            // Cut from the original image of the plate and save to a vector.
            // ATTENTION: Cutting from original,
            // we have to apply an inverse transformation to the coordinates calculated from imageCopy
            response.Add(new Band(Image.SubImage(0, peak.Left, Image.Width, peak.Diff)));
        }

        return response;
    }

    public Bitmap VerticalEdge(Bitmap bitmap)
    {
        float[,] data = {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
        };
        return bitmap.Convolve(data);
    }

    public CarSnapshotGraph Histogram(Bitmap bitmap)
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
            var imageCopy = DuplicateBitmap(Image);
            imageCopy = VerticalEdge(imageCopy);
            Thresholding(imageCopy);
            writer?.Write("vertical-rank-filter", imageCopy);

            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(CarSnapshotGraphRankFilter);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates); // sort by height
        }

        return _graphHandle.Peaks;
    }
}
