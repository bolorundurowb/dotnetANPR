using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using dotNETANPR.Configurator;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
{
    public class Band : Photo
    {
        static public Graph.ProbabilityDistributor distributor = new Graph.ProbabilityDistributor(0, 0, 25, 25);
        static Configurator.Configurator configg = new Configurator.Configurator();
        static private int numberOfCandidates = configg.getIntProperty("intelligence_numberOfPlates");

        private BandGraph graphHandle = null;

        /** Creates a new instance of Band */
        public Band()
        {
            image = null;
        }

        public Band(Bitmap bi)
        {
            new Photo(bi);
        }

        public Bitmap renderGraph()
        {
            this.computeGraph();
            return graphHandle.renderHorizontally(this.getWidth(), 100);
        }

        private List<Graph.Peak> computeGraph()
        {
            if (graphHandle != null) return graphHandle.peaks; // graf uz bol vypocitany
            Bitmap imageCopy = duplicateBitmap(this.image);
            fullEdgeDetector(imageCopy);
            graphHandle = histogram(imageCopy);
            graphHandle.rankFilter(image.Height);
            graphHandle.applyProbabilityDistributor(distributor);
            graphHandle.findPeaks(numberOfCandidates);
            return graphHandle.peaks;
        }

        public List<Plate> getPlates()
        {
            List<Plate> outt = new List<Plate>();

            List<Graph.Peak> peaks = computeGraph();

            for (int i = 0; i < peaks.Count; i++)
            {
                // vyseknut z povodneho! obrazka znacky, a ulozit do vektora. POZOR !!!!!! Vysekavame z povodneho, takze
                // na suradnice vypocitane z imageCopy musime uplatnit inverznu transformaciu
                Graph.Peak p = peaks.ElementAt(i);
                outt.Add(new Plate(image.Clone(new Rectangle(p.getLeft(), 0, p.getDiff(), image.Height), PixelFormat.Format8bppIndexed)));
            }
            return outt;
        }

        //    public void horizontalRankBi(Bitmap image) {
        //        Bitmap imageCopy = duplicateBi(image);
        //        
        //        float data[] = new float[image.getHeight()];
        //        for (int i=0; i<data.length; i++) data[i] = 1.0f/data.length;
        //        
        //        new ConvolveOp(new Kernel(data.length,1, data), ConvolveOp.EDGE_NO_OP, null).filter(imageCopy, image);
        //    }

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
