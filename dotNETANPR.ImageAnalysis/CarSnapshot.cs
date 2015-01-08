using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
{
    public class CarSnapshot : Photo
    {
        static Configurator.Configurator configg = new Configurator.Configurator();
        private static int distributor_margins =
            configg.getIntProperty("carsnapshot_distributormargins");
        //    private static int carsnapshot_projectionresize_x =
        //            Main.configurator.getIntProperty("carsnapshot_projectionresize_x");
        //    private static int carsnapshot_projectionresize_y =
        //            Main.configurator.getIntProperty("carsnapshot_projectionresize_y");
        private static int carsnapshot_graphrankfilter =
                configg.getIntProperty("carsnapshot_graphrankfilter");

        static private int numberOfCandidates = configg.getIntProperty("intelligence_numberOfBands");
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
            return graphHandle.renderVertically(100, this.getHeight());
        }

        private List<Graph.Peak> computeGraph()
        {
            if (graphHandle != null) return graphHandle.peaks; // graf uz bol vypocitany

            Bitmap imageCopy = duplicateBitmap(this.image);
            verticalEdgeBi(imageCopy);
            thresholding(imageCopy); // strasne moc zere

            graphHandle = this.histogram(imageCopy);
            graphHandle.rankFilter(carsnapshot_graphrankfilter);
            graphHandle.applyProbabilityDistributor(distributor);

            graphHandle.findPeaks(numberOfCandidates); //sort by height
            return graphHandle.peaks;
        }

        public List<Band> getBands()
        {
            List<Band> outt = new List<Band>();

            List<Graph.Peak> peaks = computeGraph();

            for (int i = 0; i < peaks.Count; i++)
            {
                // vyseknut z povodneho! obrazka znacky, a ulozit do vektora. POZOR !!!!!! Vysekavame z povodneho, takze
                // na suradnice vypocitane z imageCopy musime uplatnit inverznu transformaciu
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
        //    public void verticalRankBi(Bitmap image) {
        //        Bitmap imageCopy = duplicateBi(image);
        //        
        //        float data[] = new float[9];
        //        for (int i=0; i<data.length; i++) data[i] = 1.0f/data.length;
        //        
        //        new ConvolveOp(new Kernel(1,data.length, data), ConvolveOp.EDGE_NO_OP, null).filter(imageCopy, image);
        //    }

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
