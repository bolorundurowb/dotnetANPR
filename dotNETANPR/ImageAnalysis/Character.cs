using System.Drawing;

namespace dotNETANPR.ImageAnalysis
{
    public class Character : Photo
    {
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

        public bool normalized = false;
        public PositionInPlate positionInPlate = null;
        public Bitmap thresholdedImage;
        private PixelMap.Piece bestPiece = null;

        public Character()
        {
            Image = null;
            Init();
        }

        public Character(Bitmap bitmap) : this(bitmap, bitmap, null)
        {
            Init();
        }

        public Character(string filePath) : base(filePath)
        {
            Bitmap origin = DuplicateBitmap(Image);
            AdaptiveThresholding();
            thresholdedImage = Image;
            Image = origin;
            Init();
        }

        public Character(Bitmap clone, Bitmap bitmap, PositionInPlate positionInPlate) : base(clone)
        {
            thresholdedImage = bitmap;
            this.positionInPlate = positionInPlate;
            Init();
        }

        public new Character Clone()
        {
            return new Character((Bitmap)Image.Clone(), (Bitmap)thresholdedImage.Clone(), positionInPlate);
        }

        private void Init()
        {
            FullHeight = base.GetHeight();
            FullWidth = base.GetWidth();
        }
    }
}
