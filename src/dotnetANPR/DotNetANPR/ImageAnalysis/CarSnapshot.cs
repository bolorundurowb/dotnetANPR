using System.Collections.Generic;
using System.Drawing;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;

namespace DotNetANPR.ImageAnalysis;

public class CarSnapshot(Bitmap image) : Photo(image)
{
    private static int _distributorMargins =
        Configurator.Instance.Get<int>("carsnapshot_distributormargins");

    private static int _carsnapshotGraphrankfilter =
        Configurator.Instance.Get<int>("carsnapshot_graphrankfilter");

    private static int _numberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfBands");

    public static ProbabilityDistributor Distributor =
        new(0, 0, _distributorMargins, _distributorMargins);

    private CarSnapshotGraph? _graphHandle = null;

    public Bitmap RenderGraph()
    {
        ComputeGraph();
        return _graphHandle!.RenderVertically(100, Height);
    }

    private List<Peak> ComputeGraph()
    {
        if (_graphHandle != null)
            return _graphHandle.Peaks;

        var imageCopy = DuplicateBitmap(Image);
        VerticalEdge(imageCopy);
        Thresholding(imageCopy);

        _graphHandle = Histogram(imageCopy);
        _graphHandle.RankFilter(_carsnapshotGraphrankfilter);
        _graphHandle.ApplyProbabilityDistributor(Distributor);
        _graphHandle.FindPeaks(_numberOfCandidates); // sort by height
        return _graphHandle.Peaks;
    }

    /**
     * Recommended: 3 bands.
     *
     * @return bands
     */
    public List<Band> Bands()
    {
        List<Band> response = new();
        var peaks = ComputeGraph();
        foreach (var p in peaks)
        {
            // Cut from the original image of the plate and save to a vector.
            // ATTENTION: Cutting from original,
            // we have to apply an inverse transformation to the coordinates calculated from imageCopy
            response.Add(new Band(Image.SubImage(0, (p.Left), Image.Width, (p.Diff))));
        }

        return response;
    }

    public void VerticalEdge(Bitmap image)
    {
        var imageCopy = DuplicateBitmap(image);
        float[] data = { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        imageCopy.ConvolutionFilter(image, data);
    }

    public CarSnapshotGraph Histogram(Bitmap bi)
    {
        var graph = new CarSnapshotGraph();
        for (var y = 0; y < bi.Height; y++)
        {
            float counter = 0;
            for (var x = 0; x < bi.Width; x++)
                counter += GetBrightness(bi, x, y);

            graph.AddPeak(counter);
        }

        return graph;
    }
}
