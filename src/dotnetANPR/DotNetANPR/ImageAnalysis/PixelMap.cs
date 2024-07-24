using System;
using System.Drawing;

namespace DotNetANPR.ImageAnalysis;

public class PixelMap(Photo photo)
{
    public Piece BestPiece() { throw new System.NotImplementedException(); }

    private sealed class Point(int x, int y) : IEquatable<Point>
    {
        public int X { get; private set; } = x;
        
        public int Y { get; private set; } = y;

        public bool Equals(Point? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y;
        }

        public static bool operator ==(Point? left, Point? right) => Equals(left, right);
        public static bool operator !=(Point? left, Point? right) => !Equals(left, right);
    }
    
    public class Piece
    {
        public Bitmap Render() { throw new System.NotImplementedException(); }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MostLeftPoint() { throw new System.NotImplementedException(); }

        public int MostTopPoint() { throw new System.NotImplementedException(); }
    }
}
