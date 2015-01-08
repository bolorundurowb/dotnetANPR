using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Plate : Photo
    {
        static public Graph.ProbabilityDistributor distributor = new Graph.ProbabilityDistributor(0, 0, 0, 0);
        static Configurator.Configurator configg = new Configurator.Configurator();
        static private int numberOfCandidates = configg.getIntProperty("intelligence_numberOfImageAnalysis.Chars");
        private static int horizontalDetectionType = configg.getIntProperty("platehorizontalgraph_detectionType");

        private PlateGraph graphHandle = null;
        public Plate plateCopy;

        /** Creates a new instance of ImageAnalysis.Character */
        public Plate()
        {
            image = null;
        }

        public Plate(Bitmap bi)
        {
            new Photo(bi);
            Bitmap temp = duplicateBitmap(image);
            this.plateCopy = new Plate(temp, true);
            this.plateCopy.adaptiveThresholding();
        }

        public Plate(Bitmap bi, bool isCopy)
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

            graphHandle = histogram(plateCopy.getBi()); //PlateGraph graph = histogram(imageCopy); 
            graphHandle.applyProbabilityDistributor(distributor);
            graphHandle.findPeaks(numberOfCandidates);

            return graphHandle.peaks;
        }

        public List<ImageAnalysis.Char> getChars()
        {
            List<ImageAnalysis.Char> outt = new List<ImageAnalysis.Char>();

            List<Graph.Peak> peaks = computeGraph();

            for (int i = 0; i < peaks.Count; i++)
            {
                // vyseknut z povodneho! obrazka znacky, a ulozit do vektora. POZOR !!!!!! Vysekavame z povodneho, takze
                // na suradnice vypocitane z imageCopy musime uplatnit inverznu transformaciu
                Graph.Peak p = peaks.ElementAt(i);
                if (p.getDiff() <= 0) continue;
                outt.Add(new ImageAnalysis.Char(image.Clone(new Rectangle(
                                p.getLeft(),
                                0,
                                p.getDiff(),
                                image.Height), PixelFormat.Format8bppIndexed)
                              ,
                        this.plateCopy.image.Clone(new Rectangle(
                                p.getLeft(),
                                0,
                                p.getDiff(),
                                image.Height
                            ), PixelFormat.Format8bppIndexed),
                            new PositionInPlate(p.getLeft(), p.getRight())
                        )
                        );
            }

            return outt;
        }

        public Plate clone()
        {
            return new Plate(duplicateBitmap(image));
        }

        public void horizontalEdgeBi(Bitmap image)
        {
            Bitmap imageCopy = duplicateBitmap(image);
            float[] data = {
          -1,0,1
        };
            new ConvolveOp(new Kernel(1, 3, data), ConvolveOp.EDGE_NO_OP, null).filter(imageCopy, image);
        }

        public void normalize()
        {
            // pre ucely orezania obrazka sa vytvori klon ktory sa normalizuje a prahuje s
            // koeficientom 0.999. funkcie cutTopBottom a cutLeftRight orezu originalny
            // obrazok na zaklade horizontalnej a vertikalnej projekcie naklonovaneho
            // obrazka, ktory je prahovany

            Plate clone1 = this.clone();
            clone1.verticalEdgeDetector(clone1.getBi());
            PlateVerticalGraph vertical = clone1.histogramYaxis(clone1.getBi());
            this.image = cutTopBottom(this.image, vertical);
            this.plateCopy.image = cutTopBottom(this.plateCopy.image, vertical);

            Plate clone2 = this.clone();
            if (horizontalDetectionType == 1) clone2.horizontalEdgeDetector(clone2.getBi());
            PlateHorizontalGraph horizontal = clone1.histogramXaxis(clone2.getBi());
            this.image = cutLeftRight(this.image, horizontal);
            this.plateCopy.image = cutLeftRight(this.plateCopy.image, horizontal);

        }
        private Bitmap cutTopBottom(Bitmap origin, PlateVerticalGraph graph)
        {
            graph.applyProbabilityDistributor(new Graph.ProbabilityDistributor(0f, 0f, 2, 2));
            Graph.Peak p = graph.findPeak(3).ElementAt(0);
            return origin.Clone(new Rectangle(0, p.getLeft(), this.image.Width, p.getDiff()), PixelFormat.Format8bppIndexed
                );
        }
        private Bitmap cutLeftRight(Bitmap origin, PlateHorizontalGraph graph)
        {
            graph.applyProbabilityDistributor(new Graph.ProbabilityDistributor(0f, 0f, 2, 2));
            List<Graph.Peak> peaks = graph.findPeak(3);

            if (peaks.Count != 0)
            {
                Graph.Peak p = peaks.ElementAt(0);
                return origin.Clone(new Rectangle(p.getLeft(), 0, p.getDiff(), image.Height), PixelFormat.Format8bppIndexed);
            }
            return origin;
        }


        public PlateGraph histogram(Bitmap bi)
        {
            PlateGraph graph = new PlateGraph(this);
            for (int x = 0; x < bi.Width; x++)
            {
                float counter = 0;
                for (int y = 0; y < bi.Height; y++)
                    counter += getBrightness(bi, x, y);
                graph.addPeak(counter);
            }
            return graph;
        }

        private PlateVerticalGraph histogramYaxis(Bitmap bi)
        {
            PlateVerticalGraph graph = new PlateVerticalGraph(this);
            int w = bi.Width;
            int h = bi.Height;
            for (int y = 0; y < h; y++)
            {
                float counter = 0;
                for (int x = 0; x < w; x++)
                    counter += getBrightness(bi, x, y);
                graph.addPeak(counter);
            }
            return graph;
        }
        private PlateHorizontalGraph histogramXaxis(Bitmap bi)
        {
            PlateHorizontalGraph graph = new PlateHorizontalGraph(this);
            int w = bi.Width;
            int h = bi.Height;
            for (int x = 0; x < w; x++)
            {
                float counter = 0;
                for (int y = 0; y < h; y++)
                    counter += getBrightness(bi, x, y);
                graph.addPeak(counter);
            }
            return graph;
        }

        public void verticalEdgeDetector(Bitmap source)
        {

            float[] matrix = {
            -1,0,1
        };

            Bitmap destination = duplicateBitmap(source);

            new ConvolveOp(new Kernel(3, 1, matrix), ConvolveOp.EDGE_NO_OP, null).filter(destination, source);

        }

        public void horizontalEdgeDetector(Bitmap source)
        {
            Bitmap destination = duplicateBitmap(source);

            float[] matrix = {
            -1,-2,-1,
            0,0,0,
            1,2,1
        };

            new ConvolveOp(new Kernel(3, 3, matrix), ConvolveOp.EDGE_NO_OP, null).filter(destination, source);
        }

        public float getCharsWidthDispersion(List<ImageAnalysis.Char> chars)
        {
            float averageDispersion = 0;
            float averageWidth = this.getAverageCharWidth(chars);

            foreach (ImageAnalysis.Char chr in chars)
                averageDispersion += (Math.Abs(averageWidth - chr.fullWidth));
            averageDispersion /= chars.Count;

            return averageDispersion / averageWidth;
        }
        public float getPiecesWidthDispersion(List<ImageAnalysis.Char> chars)
        {
            float averageDispersion = 0;
            float averageWidth = this.getAveragePieceWidth(chars);

            foreach (ImageAnalysis.Char chr in chars)
                averageDispersion += (Math.Abs(averageWidth - chr.pieceWidth));
            averageDispersion /= chars.Count;

            return averageDispersion / averageWidth;
        }

        public float getAverageCharWidth(List<ImageAnalysis.Char> chars)
        {
            float averageWidth = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageWidth += chr.fullWidth;
            averageWidth /= chars.Count;
            return averageWidth;
        }
        public float getAveragePieceWidth(List<ImageAnalysis.Char> chars)
        {
            float averageWidth = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageWidth += chr.pieceWidth;
            averageWidth /= chars.Count;
            return averageWidth;
        }

        public float getAveragePieceHue(List<ImageAnalysis.Char> chars)
        {
            float averageHue = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageHue += chr.statisticAverageHue;
            averageHue /= chars.Count;
            return averageHue;
        }
        public float getAveragePieceContrast(List<ImageAnalysis.Char> chars)
        {
            float averageContrast = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageContrast += chr.statisticContrast;
            averageContrast /= chars.Count;
            return averageContrast;
        }
        public float getAveragePieceBrightness(List<ImageAnalysis.Char> chars)
        {
            float averageBrightness = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageBrightness += chr.statisticAverageBrightness;
            averageBrightness /= chars.Count;
            return averageBrightness;
        }
        public float getAveragePieceMinBrightness(List<ImageAnalysis.Char> chars)
        {
            float averageMinBrightness = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageMinBrightness += chr.statisticMinimumBrightness;
            averageMinBrightness /= chars.Count;
            return averageMinBrightness;
        }
        public float getAveragePieceMaxBrightness(List<ImageAnalysis.Char> chars)
        {
            float averageMaxBrightness = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageMaxBrightness += chr.statisticMaximumBrightness;
            averageMaxBrightness /= chars.Count;
            return averageMaxBrightness;
        }

        public float getAveragePieceSaturation(List<ImageAnalysis.Char> chars)
        {
            float averageSaturation = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageSaturation += chr.statisticAverageSaturation;
            averageSaturation /= chars.Count;
            return averageSaturation;
        }

        public float getCharHeight(List<ImageAnalysis.Char> chars)
        {
            float averageHeight = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageHeight += chr.fullHeight;
            averageHeight /= chars.Count;
            return averageHeight;
        }
        public float getAveragePieceHeight(List<ImageAnalysis.Char> chars)
        {
            float averageHeight = 0;
            foreach (ImageAnalysis.Char chr in chars)
                averageHeight += chr.pieceHeight;
            averageHeight /= chars.Count;
            return averageHeight;
        }

        //    public float getAverageCharSquare(List<ImageAnalysis.Char> chars)
        //   {
        //        float average = 0;
        //        for (ImageAnalysis.Char chr : chars)
        //            average += chr.getWidth() * chr.getHeight();
        //        average /= chars.Count;
        //        return average;
        //    }

    }
}
