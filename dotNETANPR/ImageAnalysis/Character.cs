using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Character
    {
        public Character(Bitmap clone, Bitmap bitmap, PositionInPlate positionInPlate)
        {
            throw new System.NotImplementedException();
        }

        public float FullWidth { get; set; }
        public float PieceWidth { get; set; }
        public float StatisticAverageHue { get; set; }
        public float StatisticContrast { get; set; }
        public float StatisticAverageBrightness { get; set; }
        public float StatisticMinimumBrightness { get; set; }
        public float StatisticMaximumBrightness { get; set; }
        public float StatisticAverageSaturation { get; set; }
        public float FullHeight { get; set; }
        public float PieceHeight { get; set; }
    }
}
