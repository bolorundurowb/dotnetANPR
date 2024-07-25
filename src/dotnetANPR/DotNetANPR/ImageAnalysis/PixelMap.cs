using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DotNetANPR.ImageAnalysis;

public class PixelMap(Photo photo)
{
    static bool[][] matrix;
    private Piece bestPiece = null;
    private int width;
    private int height;

    public Piece BestPiece() => throw new System.NotImplementedException();

    public sealed class Point(int x, int y) : IEquatable<Point>
    {
        public int X { get; private set; } = x;

        public int Y { get; private set; } = y;

        public bool Equals(Point? other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return X == other.X && Y == other.Y;
        }

        public static bool operator ==(Point? left, Point? right) => Equals(left, right);

        public static bool operator !=(Point? left, Point? right) => !Equals(left, right);
    }

    public class PointSet : List<Point>
    {
        public void Push(Point point) { Add(point); }

        public Point Pop()
        {
            var value = this[Count - 1];
            RemoveAt(Count - 1);
            return value;
        }

        public void RemovePoint(Point point)
        {
            if (Contains(point))
            {
                Remove(point);
            }
        }
    }

    public class PieceSet : List<Piece> { }

    public class Piece : PointSet
    {
        public int MostLeftPoint { get; set; }

        public int MostRightPoint { get; set; }

        public int MostTopPoint { get; set; }

        public int MostBottomPoint { get; set; }

        public int CenterX { get; set; }

        public int CenterY { get; set; }

        public float Magnitude { get; set; }

        public int NumberOfBlackPoints { get; set; }

        public int NumberOfAllPoints { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }


        public Bitmap? Render()
        {
            if (NumberOfAllPoints == 0)
                return null;

            var image = new Bitmap(Width, Height);

            for (var x = MostLeftPoint; x <= MostRightPoint; x++)
            for (var y = MostTopPoint; y <= MostBottomPoint; y++)
                image.SetPixel(x - MostLeftPoint, y - MostTopPoint, matrix[x][y] ? Color.Black : Color.White);

            return image;
        }

        public void CreateStatistics()
        {
            MostLeftPoint = ComputeMostLeftPoint();
            MostRightPoint = ComputeMostRightPoint();
            MostTopPoint = ComputeMostTopPoint();
            MostBottomPoint = ComputeMostBottomPoint();
            Width = (MostRightPoint - MostLeftPoint) + 1;
            Height = (MostBottomPoint - MostTopPoint) + 1;
            CenterX = (MostLeftPoint + MostRightPoint) / 2;
            CenterY = (MostTopPoint + MostBottomPoint) / 2;
            NumberOfBlackPoints = Count;
            NumberOfAllPoints = Width * Height;
            Magnitude = (float)NumberOfBlackPoints / NumberOfAllPoints;
        }

        /**
         * Computes how much the piece is similar to a character.
         *
         * @return the cost
         */
        public int Cost() => NumberOfAllPoints - NumberOfBlackPoints();

        public void BleachPiece()
        {
            foreach (var point in this)
            {
                matrix[point.X][point.Y] = false;
            }
        }

        private int ComputeMostLeftPoint() => this.Select(point => point.X).Prepend(int.MaxValue).Min();

        private int ComputeMostRightPoint() => this.Select(point => point.X).Prepend(0).Max();

        private int ComputeMostTopPoint() => this.Select(point => point.Y).Prepend(int.MaxValue).Min();

        private int ComputeMostBottomPoint() => this.Select(point => point.Y).Prepend(0).Max();
    }
}
