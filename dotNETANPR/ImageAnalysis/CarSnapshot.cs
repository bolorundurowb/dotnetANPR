using System.Collections.Generic;
using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class CarSnapshot : Photo
    {
        private static int distributorMargins =
            Intelligence.Intelligence.Configurator.GetIntProperty("carsnapshot_distributormargins");

        private static int carSnapshotGraphRankFilter =
            Intelligence.Intelligence.Configurator.GetIntProperty("carsnapshot_graphrankfilter");

        private static int numberOfCandidates =
            Intelligence.Intelligence.Configurator.GetIntProperty("intelligence_numberOfBands");

        public static Graph.ProbabilityDistributor Distributor = new Graph.ProbabilityDistributor(0, 0, distributorMargins, distributorMargins);

        private CarSnapshotGraph graphHandle = null;

        public CarSnapshot()
        {

        }

        public CarSnapshot(string filePath) : base(filePath)
        {

        }

        public CarSnapshot(Bitmap bitmap) : base(bitmap)
        {

        }

        public Bitmap RenderGraph()
        {
            ComputeGraph();
            return graphHandle.RenderVertically(100, GetHeight());
        }

        private List<Graph.Peak> ComputeGraph()
        {
            if (graphHandle != null)
            {
                return graphHandle.Peaks;
            }
            Bitmap bitmap = DuplicateBitmap(Image);
            VerticalEdgeBitmap(bitmap);
            Thresholding(bitmap);

            graphHandle = Histogram(bitmap);
            graphHandle.RankFilter(carSnapshotGraphRankFilter);
            graphHandle.ApplyProbabilityDistributor(Distributor);
            graphHandle.FindPeaks(numberOfCandidates);
            return graphHandle.Peaks;
        }

        public List<Band> GetBands()
        {

        }
    }
}
