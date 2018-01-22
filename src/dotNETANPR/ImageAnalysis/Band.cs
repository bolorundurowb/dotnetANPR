using System;
using System.Collections.Generic;
using System.Drawing;
using dotnetANPR.ImageAnalysis.Convolution;

namespace dotnetANPR.ImageAnalysis
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

        public Band(Bitmap bitmap) : base(bitmap)
        {
        }

        public Bitmap RenderGraph()
        {
            ComputeGraph();
            return _graphHandle.RenderHorizontally(GetWidth(), 100);
        }

        private List<Graph.Peak> ComputeGraph()
        {
            if (_graphHandle != null) return _graphHandle.Peaks;
            var imageCopy = DuplicateBitmap(Image);
            FullEdgeDetector(imageCopy);
            _graphHandle = Histogram(imageCopy);
            _graphHandle.RankFilter(Image.Height);
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(numberOfCandidates);
            return _graphHandle.Peaks;
        }

        public List<Plate> GetPlates()
        {
            var output = new List<Plate>();
            var peaks = ComputeGraph();
            for (var i = 0; i < peaks.Count; i++)
            {
                var p = peaks[i];
                var rectangle = new Rectangle(
                    p.Left,
                    0,
                    p.GetDiff(),
                    Image.Height
                );
                var clone = Image.Clone(
                    rectangle,
                    Image.PixelFormat
                );
                var plate = new Plate(clone);
                output.Add(plate);
            }
            return output;
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

            var i1 = CreateBlankBitmap(source);
            var i2 = CreateBlankBitmap(source);

            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel
            {
                Size = 3,
                Matrix = verticalMatrix
            };
            i1 = convolveOp.Convolve(source, kernel);

            kernel = new ConvolutionKernel();
            kernel.Size = 3;
            kernel.Matrix = horizontalMatrix;
            i2 = convolveOp.Convolve(source, kernel);

            var w = source.Width;
            var h = source.Height;

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var sum = 0.0f;
                    sum += GetBrightness(i1, x, y);
                    sum += GetBrightness(i2, x, y);
                    SetBrightness(source, x, y, Math.Min(1.0f, sum));
                }
            }
        }
    }
}
