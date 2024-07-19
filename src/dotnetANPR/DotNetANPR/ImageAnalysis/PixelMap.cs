using System.Drawing;

namespace DotNetANPR.ImageAnalysis;

public class PixelMap(Character character)
{
    public class Piece
    {
        public Bitmap Render() { throw new System.NotImplementedException(); }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MostLeftPoint() { throw new System.NotImplementedException(); }

        public int MostTopPoint() { throw new System.NotImplementedException(); }
    }

    public Piece BestPiece() { throw new System.NotImplementedException(); }
}
