using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace dotNETANPR.ImageAnalysis
{
    class CarSnapshot : Photo
    {
        static Configurator.Configurator config = new Configurator.Configurator();
        private static int distributor_margins =
            config.GetIntProperty("carsnapshot_distributormargins");
        private static int carsnapshot_graphrankfilter =
                config.GetIntProperty("carsnapshot_graphrankfilter");

        static private int numberOfCandidates = config.GetIntProperty("intelligence_numberOfBands");
        private CarSnapshotGraph graphHandle = null;


        public static Graph.ProbabilityDistributor distributor = new Graph.ProbabilityDistributor(0, 0, distributor_margins, distributor_margins);

        public CarSnapshot()
        {

        }

        public CarSnapshot(String filepath)
        {
            new Photo(filepath);
        }

        public CarSnapshot(Bitmap bi)
        {
            new Photo(bi);
        }

        public Bitmap renderGraph()
        {
            this.computeGraph();
            return graphHandle.renderVertically(100, this.GetHeight());
        }

        private List<Graph.Peak> computeGraph()
        {
            if (graphHandle != null) return graphHandle.peaks; 

            Bitmap imageCopy = duplicateBitmap(this.image);
            verticalEdgeBi(imageCopy);
            thresholding(imageCopy); 

            graphHandle = histogram(imageCopy);
            graphHandle.rankFilter(carsnapshot_graphrankfilter);
            graphHandle.applyProbabilityDistributor(distributor);

            graphHandle.FindPeaks(numberOfCandidates); 
            return graphHandle.peaks;
        }

        public List<Band> getBands()
        {
            List<Band> outt = new List<Band>();

            List<Graph.Peak> peaks = computeGraph();

            for (int i = 0; i < peaks.Count; i++)
            {
                Graph.Peak p = peaks.ElementAt(i);
                outt.Add(new Band(
                        image.Clone(new Rectangle(0,
                        (int)(p.getLeft()),
                        image.Width,
                        (int)(p.getDiff())
                        ), PixelFormat.Format8bppIndexed)
                        ));
            }
            return outt;
        }

        public void verticalEdgeBi(Bitmap image)
        {
            Bitmap imageCopy = duplicateBitmap(image);

            float[] data = {
            -1,0,1,
            -1,0,1,
            -1,0,1,
            -1,0,1
        };

            new ConvolveOp(new Kernel(3, 4, data), ConvolveOp.EDGE_NO_OP, null).filter(imageCopy, image);
        }

        public CarSnapshotGraph histogram(Bitmap bi)
        {
            CarSnapshotGraph graph = new CarSnapshotGraph(this);
            for (int y = 0; y < bi.Height; y++)
            {
                float counter = 0;
                for (int x = 0; x < bi.Width; x++)
                    counter += getBrightness(bi, x, y);
                graph.addPeak(counter);
            }
            return graph;
        }
    }
}
