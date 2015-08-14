using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace dotNETANPR.ImageAnalysis
{
    class Band : Photo
    {
        static public Graph.ProbabilityDistributor distributor = new Graph.ProbabilityDistributor(0, 0, 25, 25);
        static Configurator.Configurator config = new Configurator.Configurator();
        static private int numberOfCandidates = config.GetIntProperty("intelligence_numberOfPlates");

        private BandGraph graphHandle = null;
        
        public Band()
        {
            image = null;
        }

        public Band(Bitmap bi)
        {
            new Photo(bi);
        }

        public Bitmap RenderGraph()
        {
            computeGraph();
            return graphHandle.renderHorizontally(this.GetWidth(), 100);
        }

        private List<Graph.Peak> computeGraph()
        {
            if (graphHandle != null) return graphHandle.peaks; 
            Bitmap imageCopy = duplicateBitmap(this.image);
            fullEdgeDetector(imageCopy);
            graphHandle = histogram(imageCopy);
            graphHandle.rankFilter(image.Height);
            graphHandle.applyProbabilityDistributor(distributor);
            graphHandle.findPeaks(numberOfCandidates);
            return graphHandle.peaks;
        }

        public List<Plate> GetPlates()
        {
            List<Plate> outt = new List<Plate>();

            List<Graph.Peak> peaks = computeGraph();

            for (int i = 0; i < peaks.Count; i++)
            {
                Graph.Peak p = peaks.ElementAt(i);
                outt.Add(new Plate(image.Clone(new Rectangle(p.GetLeft(), 0, p.GetDiff(), image.Height), PixelFormat.Format8bppIndexed)));
            }
            return outt;
        }

        public BandGraph histogram(Bitmap bi)
        {
            BandGraph graph = new BandGraph(this);
            for (int x = 0; x < bi.Width; x++)
            {
                float counter = 0;
                for (int y = 0; y < bi.Height; y++)
                    counter += getBrightness(bi, x, y);
                graph.addPeak(counter);
            }
            return graph;
        }

        public void fullEdgeDetector(Bitmap source)
        {
            float[] verticalMatrix = {
            -1,0,1,
            -2,0,2,
            -1,0,1,
        };
            float[] horizontalMatrix = {
            -1,-2,-1,
            0, 0, 0,
            1, 2, 1
        };

            Bitmap i1 = createBlankBi(source);
            Bitmap i2 = createBlankBi(source);

            new ConvolveOp(new Kernel(3, 3, verticalMatrix), ConvolveOp.EDGE_NO_OP, null).filter(source, i1);
            new ConvolveOp(new Kernel(3, 3, horizontalMatrix), ConvolveOp.EDGE_NO_OP, null).filter(source, i2);

            int w = source.Width;
            int h = source.Height;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    float sum = 0.0f;
                    sum += getBrightness(i1, x, y);
                    sum += getBrightness(i2, x, y);
                    setBrightness(source, x, y, Math.Min(1.0f, sum));
                }

        }

    }
}
