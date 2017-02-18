using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Band : Photo
    {
        public static Graph.ProbabilityDistributor Distributor =
            new Graph.ProbabilityDistributor(0, 0, 25, 25);

        private static int numberOfCandidates =
            Intelligence.Configurator.GetIntProperty("intelligence_numberOfPlates");

        private BandGraph graphHandle = null;

        public Band()
        {
            Image = null;
        }

        public Band(Bitmap bitmap) : base(bitmap) {}


    }
}
