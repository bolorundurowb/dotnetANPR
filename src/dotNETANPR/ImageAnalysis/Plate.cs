using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using dotNETANPR.ImageAnalysis.Convolution;

namespace dotNETANPR.ImageAnalysis
{
    public class Plate : Photo
    {
        public static Graph.ProbabilityDistributor Distributor = new Graph.ProbabilityDistributor(0, 0, 0, 0);
        private static readonly int NumberOfCandidates =
            Intelligence.Intelligence.Configurator.GetIntProperty("intelligence_numberOfChars");
        private static readonly int HorizontalDetectionType =
            Intelligence.Intelligence.Configurator.GetIntProperty("platehorizontalgraph_detectionType");
        private PlateGraph _graphHandle;
        public Plate PlateCopy;

        public Plate()
        {
            Image = null;
        }

        public Plate(Bitmap bitmap) : base(bitmap)
        {
            PlateCopy = new Plate(DuplicateBitmap(Image), true);
            PlateCopy.AdaptiveThresholding();
        }

        public Plate(Bitmap bitmap, bool isCopy) : base(bitmap) {}

        public Bitmap RenderGraph()
        {
            ComputeGraph();
            return _graphHandle.RenderHorizontally(GetWidth(), 100);
        }

        private List<Graph.Peak> ComputeGraph()
        {
            if (_graphHandle != null) return _graphHandle.Peaks;

            _graphHandle = Histogram(PlateCopy.GetBitmap());
            _graphHandle.ApplyProbabilityDistributor(Distributor);
            _graphHandle.FindPeaks(NumberOfCandidates);

            return _graphHandle.Peaks;
        }
        
        public List<Character> GetChars()
        {
            var output = new List<Character>();
            var peaks = ComputeGraph();
            for (var i = 0; i < peaks.Count; i++)
            {
                var p = peaks[i];
                if (p.GetDiff() <= 0) continue;
                output.Add(new Character(
                        Image.Clone(new Rectangle(
                            p.Left,
                            0,
                            p.GetDiff(),
                            Image.Height
                        ), PixelFormat.Format8bppIndexed
                        )
                        ,
                        PlateCopy.Image.Clone(new Rectangle(
                            p.Left,
                            0,
                            p.GetDiff(),
                            Image.Height
                        ), PixelFormat.Format8bppIndexed
                        ),
                        new PositionInPlate(p.Left, p.Right
                        )
                    )
                );
            }
            return output;
        }

        public new Plate Clone()
        {
            return new Plate(DuplicateBitmap(Image));
        }

        public void HorizontalEdgeBi(Bitmap image)
        {
            int[,] matrix =
            {
                {-1, 0, 1}
            };
            var destination = DuplicateBitmap(image);
            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel
            {
                Size = 3,
                Matrix = matrix
            };
            destination = convolveOp.Convolve(image, kernel);
        }

        public void Normalize()
        {
            var clone1 = Clone();
            clone1.VerticalEdgeDetector(clone1.GetBitmap());
            var vertical = clone1.HistogramYaxis(clone1.GetBitmap());
            Image = CutTopBottom(Image, vertical);
            PlateCopy.Image = CutTopBottom(PlateCopy.Image, vertical);

            var clone2 = Clone();
            if (HorizontalDetectionType == 1) clone2.HorizontalEdgeDetector(clone2.GetBitmap());
            var horizontal = clone1.HistogramXAxis(clone2.GetBitmap());
            Image = CutLeftRight(Image, horizontal);
            PlateCopy.Image = CutLeftRight(PlateCopy.Image, horizontal);
        }

        private Bitmap CutTopBottom(Bitmap origin, PlateVerticalGraph graph)
        {
            graph.ApplyProbabilityDistributor(new Graph.ProbabilityDistributor(0f, 0f, 2, 2));
            var p = graph.FindPeak(3)[0];
            return origin.Clone(new Rectangle(0, p.Left, Image.Width, p.GetDiff()), PixelFormat.Format8bppIndexed
            );
        }
        private Bitmap CutLeftRight(Bitmap origin, PlateHorizontalGraph graph)
        {
            graph.ApplyProbabilityDistributor(new Graph.ProbabilityDistributor(0f, 0f, 2, 2));
            var peaks = graph.FindPeak(3);

            if (peaks.Count != 0)
            {
                var p = peaks[0];
                return origin.Clone(new Rectangle(p.Left, 0, p.GetDiff(), Image.Height), PixelFormat.Format8bppIndexed);
            }
            return origin;
        }

        public PlateGraph Histogram(Bitmap bitmap)
        {
            var graph = new PlateGraph(this);
            for (var x = 0; x < bitmap.Width; x++)
            {
                float counter = 0;
                for (var y = 0; y < bitmap.Height; y++)
                    counter += GetBrightness(bitmap, x, y);
                graph.AddPeak(counter);
            }
            return graph;
        }
        
        private PlateVerticalGraph HistogramYaxis(Bitmap bitmap)
        {
            var graph = new PlateVerticalGraph(this);
            var w = bitmap.Width;
            var h = bitmap.Height;
            for (var y = 0; y < h; y++)
            {
                float counter = 0;
                for (var x = 0; x < w; x++)
                    counter += GetBrightness(bitmap, x, y);
                graph.AddPeak(counter);
            }
            return graph;
        }
        
        private PlateHorizontalGraph HistogramXAxis(Bitmap bitmap)
        {
            var graph = new PlateHorizontalGraph(this);
            var w = bitmap.Width;
            var h = bitmap.Height;
            for (var x = 0; x < w; x++)
            {
                float counter = 0;
                for (var y = 0; y < h; y++)
                    counter += GetBrightness(bitmap, x, y);
                graph.AddPeak(counter);
            }
            return graph;
        }

        public new void VerticalEdgeDetector(Bitmap source)
        {
            int[,] matrix =
            {
                {-1, 0, 1},
                {-1, 0, 1},
                {-1, 0, 1}
            };
            var destination = DuplicateBitmap(source);
            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel
            {
                Size = 3,
                Matrix = matrix
            };
            destination = convolveOp.Convolve(source, kernel);
        }

        public void HorizontalEdgeDetector(Bitmap source)
        {
            var destination = DuplicateBitmap(source);
            int[,] matrix =
            {
                {-1, -2, -1},
                {0, 0, 0},
                {1, 2, 1}
            };
            var convolveOp = new ConvolveOp();
            var kernel = new ConvolutionKernel
            {
                Size = 3,
                Matrix = matrix
            };
            destination = convolveOp.Convolve(source, kernel);
        }

        public float GetCharsWidthDispersion(List<Character> chars)
        {
            float averageDispersion = 0;
            var averageWidth = GetAverageCharWidth(chars);
            foreach (var chr in chars)
                averageDispersion += Math.Abs(averageWidth - chr.FullWidth);
            averageDispersion /= chars.Count;
            return averageDispersion / averageWidth;
        }

        public float GetPiecesWidthDispersion(List<Character> chars)
        {
            float averageDispersion = 0;
            var averageWidth = GetAveragePieceWidth(chars);
            foreach (var chr in chars)
                averageDispersion += Math.Abs(averageWidth - chr.PieceWidth);
            averageDispersion /= chars.Count;
            return averageDispersion / averageWidth;
        }
        
        public float GetAverageCharWidth(List<Character> chars)
        {
            float averageWidth = 0;
            foreach (var chr in chars)
                averageWidth += chr.FullWidth;
            averageWidth /= chars.Count;
            return averageWidth;
        }
        public float GetAveragePieceWidth(List<Character> chars)
        {
            float averageWidth = 0;
            foreach (var chr in chars)
                averageWidth += chr.PieceWidth;
            averageWidth /= chars.Count;
            return averageWidth;
        }

        public float GetAveragePieceHue(List<Character> chars)
        {
            float averageHue = 0;
            foreach (var chr in chars)
                averageHue += chr.StatisticAverageHue;
            averageHue /= chars.Count;
            return averageHue;
        }
        public float GetAveragePieceContrast(List<Character> chars)
        {
            float averageContrast = 0;
            foreach (var chr in chars)
                averageContrast += chr.StatisticContrast;
            averageContrast /= chars.Count;
            return averageContrast;
        }
        public float GetAveragePieceBrightness(List<Character> chars)
        {
            float averageBrightness = 0;
            foreach (var chr in chars)
                averageBrightness += chr.StatisticAverageBrightness;
            averageBrightness /= chars.Count;
            return averageBrightness;
        }
        public float GetAveragePieceMinBrightness(List<Character> chars)
        {
            float averageMinBrightness = 0;
            foreach (var chr in chars)
                averageMinBrightness += chr.StatisticMinimumBrightness;
            averageMinBrightness /= chars.Count;
            return averageMinBrightness;
        }
        public float GetAveragePieceMaxBrightness(List<Character> chars)
        {
            float averageMaxBrightness = 0;
            foreach (var chr in chars)
                averageMaxBrightness += chr.StatisticMaximumBrightness;
            averageMaxBrightness /= chars.Count;
            return averageMaxBrightness;
        }

        public float GetAveragePieceSaturation(List<Character> chars)
        {
            float averageSaturation = 0;
            foreach (var chr in chars)
                averageSaturation += chr.StatisticAverageSaturation;
            averageSaturation /= chars.Count;
            return averageSaturation;
        }

        public float GetCharHeight(List<Character> chars)
        {
            float averageHeight = 0;
            foreach (var chr in chars)
                averageHeight += chr.FullHeight;
            averageHeight /= chars.Count;
            return averageHeight;
        }
        public float GetAveragePieceHeight(List<Character> chars)
        {
            float averageHeight = 0;
            foreach (var chr in chars)
                averageHeight += chr.PieceHeight;
            averageHeight /= chars.Count;
            return averageHeight;
        }
    }
}
