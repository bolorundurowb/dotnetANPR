using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Plate : Photo
    {
        public static Graph.ProbabilityDistributor distributor = new Graph.ProbabilityDistributor(0, 0, 0, 0);

        private static int numberOfCandidates =
            Intelligence.Intelligence.Configurator.GetIntProperty("intelligence_numberOfImageAnalysis.Chars");

        private static int horizontalDetectionType =
            Intelligence.Intelligence.Configurator.GetIntProperty("platehorizontalgraph_detectionType");

        private PlateGraph graphHandle = null;
        public Plate plateCopy;

        public Plate()
        {
            this.Image = null;
        }

        public Plate(Bitmap bitmap) : base(bitmap)
        {
            plateCopy = new Plate(DuplicateBitmap(this.Image), true);
            plateCopy.AdaptiveThresholding();
        }

        public Plate(Bitmap bitmap, bool isCopy) : base(bitmap) {}
    }
}
