using System;
using System.Collections.Generic;
using System.Drawing;
using DotNetANPR.Configuration;
using DotNetANPR.Extensions;

namespace DotNetANPR.ImageAnalysis;

public class Band : Photo
{
    private static readonly ProbabilityDistributor distributor = new(0, 0, 25, 25);

    private static readonly int numberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfPlates");

    private BandGraph? graphHandle = null;

    public Band(Bitmap image) : base(image) { }

    public Bitmap RenderGraph()
    {
        ComputeGraph();
        return graphHandle!.RenderHorizontally(Width, 100);
    }

    private List<Peak> ComputeGraph()
    {
        if (graphHandle != null)
        {
            return graphHandle.Peaks;
        }

        var imageCopy = Photo.DuplicateBitmap(Image);
        FullEdgeDetector(imageCopy);
        graphHandle = Histogram(imageCopy);
        graphHandle.RankFilter(Image.Height);
        graphHandle.ApplyProbabilityDistributor(Band.distributor);
        graphHandle.FindPeaks(Band.numberOfCandidates);
        return graphHandle.Peaks;
    }

    /**
     * Recommended: 3 plates.
     *
     * @return plates
     */
    public List<Plate> Plates()
    {
        List<Plate> response = new();
        List<Peak> peaks = ComputeGraph();
        foreach (Peak p in peaks)
        {
            // Cut from the original image of the plate and save to a vector.
            // ATTENTION: Cutting from original,
            // we have to apply an inverse transformation to the coordinates calculated from imageCopy
            response.Add(new Plate(Image.SubImage(p.Left, 0, p.Diff, Image.Height)));
        }

        return response;
    }

    public BandGraph Histogram(Bitmap bi)
    {
        BandGraph graph = new BandGraph(this);
        for (int x = 0; x < bi.Width; x++)
        {
            float counter = 0;
            for (int y = 0; y < bi.Height; y++)
            {
                counter += Photo.GetBrightness(bi, x, y);
            }

            graph.AddPeak(counter);
        }

        return graph;
    }

    public void FullEdgeDetector(Bitmap source)
    {
        float[] verticalMatrix = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        float[] horizontalMatrix = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
        var i1 = Photo.CreateBlankBitmap(source);
        var i2 = Photo.CreateBlankBitmap(source);

        // Apply vertical edge detection
        source.ConvolutionFilter(i1, verticalMatrix);

        // Apply horizontal edge detection
        source.ConvolutionFilter(i2, horizontalMatrix);

        // Combine edge detection results
        int width = source.Width;
        int height = source.Height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float sum = GetBrightness(source, x, y);
                sum += GetBrightness(i2, x, y);
                Photo.SetBrightness(source, x, y, Math.Min(1f, sum));
            }
        }
    }
}
