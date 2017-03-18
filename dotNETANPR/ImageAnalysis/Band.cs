using System;
using System.Collections.Generic;
using System.Drawing;
using dotNETANPR.ImageAnalysis.Convolution;

namespace dotNETANPR.ImageAnalysis
{
    public class Band : Photo
    {
        public static Graph.ProbabilityDistributor Distributor =
            new Graph.ProbabilityDistributor(0, 0, 25, 25);

        private static readonly int numberOfCandidates =
            Intelligence.Intelligence.Configurator.GetIntProperty("intelligence_numberOfPlates");

        private BandGraph _graphHandle;

        public Band()
        {
            Image = null;
        }

        public Band(Bitmap bitmap) : base(bitmap) {}

        public Bitmap RenderGraph() {
            ComputeGraph();
            return _graphHandle.RenderHorizontally(GetWidth(), 100);
        }

        private List<Graph.Peak> ComputeGraph() {
            if (_graphHandle != null) return _graphHandle.Peaks;
            Bitmap imageCopy = DuplicateBitmap(Image);
            FullEdgeDetector(imageCopy);
            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(Image.Height);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(numberOfCandidates);
            return _graphHandle.Peaks;
        }

        public List<Plate> GetPlates()
        {
            List<Plate> output = new List<Plate>();
            List<Graph.Peak> peaks = ComputeGraph();
            for (int i = 0; i < peaks.Count; i++)
            {
                Graph.Peak p = peaks[i];
                output.Add(new Plate(
                    Image.Clone(new Rectangle(
                            p.Left,
                            0,
                            p.GetDiff(),
                            Image.Height
                        ),
                        Image.PixelFormat
                    )));
            }
            return output;
        }

        public BandGraph Histogram(Bitmap bitmap)
        {
            BandGraph graph = new BandGraph(this);
            for (int x = 0; x < bitmap.Width; x++)
            {
                float counter = 0;
                for (int y = 0; y < bitmap.Height; y++)
                    counter += GetBrightness(bitmap, x, y);
                graph.AddPeak(counter);
            }
            return graph;
        }

        public void FullEdgeDetector(Bitmap source)
        {
            int[,] verticalMatrix =
            {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            };
            int[,] horizontalMatrix =
            {
                {-1, -2, -1},
                {0, 0, 0},
                {1, 2, 1}
            };

            Bitmap i1 = CreateBlankBitmap(source);
            Bitmap i2 = CreateBlankBitmap(source);

            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel();
            kernel.Size = 3;
            kernel.Matrix = verticalMatrix;
            i1 = convolveOp.Convolve(source, kernel);

            kernel = new ConvolutionKernel();
            kernel.Size = 3;
            kernel.Matrix = horizontalMatrix;
            i2 = convolveOp.Convolve(source, kernel);

            int w = source.Width;
            int h = source.Height;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float sum = 0.0f;
                    sum += GetBrightness(i1, x, y);
                    sum += GetBrightness(i2, x, y);
                    SetBrightness(source, x, y, Math.Min(1.0f, sum));
                }
            }
        }
    }
}
