using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace dotnetANPR.ImageAnalysis;

/// <summary>
/// Represents a binary pixel matrix used for connected-component analysis, skeletonisation,
/// and noise reduction on thresholded character images.
/// </summary>
internal class PixelMap
{
    private bool[,] _matrix = null!;
    private Piece? _bestPiece;
    private int _width;
    private int _height;

    /// <summary>
    /// Initialises the pixel map from a thresholded photo.
    /// </summary>
    public PixelMap(Photo photo) => MatrixInit(photo);

    /// <summary>
    /// Renders the pixel map back to a binary bitmap.
    /// </summary>
    public SKBitmap Render()
    {
        var image = new SKBitmap(_width, _height);
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                image.SetPixel(x, y, _matrix[x, y] ? SKColors.Black : SKColors.White);

        return image;
    }

    /// <summary>
    /// Finds the best (largest) connected piece, removing all others.
    /// </summary>
    public Piece BestPiece()
    {
        ReduceOtherPieces();
        return _bestPiece ?? new Piece(_matrix);
    }

    /// <summary>
    /// Applies the Zhang-Suen thinning algorithm to skeletonise the pixel map in-place.
    /// </summary>
    public PixelMap Skeletonize()
    {
        PointSet flaggedPoints = [];
        PointSet boundaryPoints = [];
        bool cont;
        do
        {
            cont = false;
            FindBoundaryPoints(boundaryPoints);
            // apply step 1 to flag boundary points for deletion
            flaggedPoints.AddRange(boundaryPoints.Where(point => Step1Passed(point.X, point.Y)));

            // delete flagged points
            if (!flaggedPoints.Any())
                cont = true;

            foreach (var point in flaggedPoints)
            {
                _matrix[point.X, point.Y] = false;
                boundaryPoints.Remove(point);
            }

            flaggedPoints.Clear();
            // apply step 2 to flag remaining points
            flaggedPoints.AddRange(boundaryPoints.Where(point => Step2Passed(point.X, point.Y)));

            // delete flagged points
            if (!flaggedPoints.Any())
                cont = true;

            foreach (var point in flaggedPoints)
                _matrix[point.X, point.Y] = false;

            boundaryPoints.Clear();
            flaggedPoints.Clear();
        } while (cont);

        return this;
    }

    /// <summary>
    /// Removes isolated pixel noise where a point has fewer than 4 neighbours.
    /// </summary>
    public PixelMap ReduceNoise()
    {
        PointSet pointsToReduce = [];
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (N(x, y) < 4)
                    pointsToReduce.Add(new Point(x, y)); // recommended 4

        // remove marked points
        foreach (var point in pointsToReduce)
            _matrix[point.X, point.Y] = false;

        return this;
    }

    /// <summary>
    /// Finds all connected components (pieces) in the pixel map.
    /// </summary>
    public PieceSet FindPieces()
    {
        PieceSet pieces = [];
        // put all black points into a set
        PointSet unsorted = [];
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (_matrix[x, y])
                    unsorted.Add(new Point(x, y));

        while (unsorted.Any())
            pieces.Add(CreatePiece(unsorted));

        return pieces;
    }

    /// <summary>
    /// Retains only the best (highest cost) connected piece and removes all others.
    /// </summary>
    public void ReduceOtherPieces()
    {
        if (_bestPiece != null)
            return; // we've got a best piece already

        var pieces = FindPieces();
        var maxCost = 0;
        var maxIndex = 0;
        // find the best cost
        for (var i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].Cost() > maxCost)
            {
                maxCost = pieces[i].Cost();
                maxIndex = i;
            }
        }

        // delete the others
        for (var i = 0; i < pieces.Count; i++)
            if (i != maxIndex)
                pieces[i].BleachPiece();

        if (pieces.Count != 0)
            _bestPiece = pieces[maxIndex];
    }

    #region Private Helpers

    private void MatrixInit(Photo bi)
    {
        _width = bi.Width;
        _height = bi.Height;
        _matrix = new bool[_width, _height];
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                _matrix[x, y] = bi.GetBrightness(x, y) < 0.5;
    }

    private bool GetPointValue(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _width || y >= _height)
            return false;

        return _matrix[x, y];
    }

    private bool IsBoundaryPoint(int x, int y)
    {
        if (!GetPointValue(x, y))
            // if it's white (outside points are automatically white)
            return false;

        // a boundary point must have at least one neighbor point that's white
        return !GetPointValue(x - 1, y - 1) || !GetPointValue(x - 1, y + 1) || !GetPointValue(x + 1, y - 1)
               || !GetPointValue(x + 1, y + 1) || !GetPointValue(x, y + 1) || !GetPointValue(x, y - 1)
               || !GetPointValue(x + 1, y) || !GetPointValue(x - 1, y);
    }

    private int N(int x, int y)
    {
        // number of black points in the neighborhood
        var n = 0;
        if (GetPointValue(x - 1, y - 1))
            n++;

        if (GetPointValue(x - 1, y + 1))
            n++;

        if (GetPointValue(x + 1, y - 1))
            n++;

        if (GetPointValue(x + 1, y + 1))
            n++;

        if (GetPointValue(x, y + 1))
            n++;

        if (GetPointValue(x, y - 1))
            n++;

        if (GetPointValue(x + 1, y))
            n++;

        if (GetPointValue(x - 1, y))
            n++;

        return n;
    }

    private int T(int x, int y)
    {
        var n = 0;
        for (var i = 2; i <= 8; i++)
            if (!P(i, x, y) && P(i + 1, x, y))
                n++;

        if (!P(9, x, y) && P(2, x, y))
            n++;

        return n;
    }

    private bool P(int i, int x, int y)
    {
        switch (i)
        {
            case 1:
                return GetPointValue(x, y);
            case 2:
                return GetPointValue(x, y - 1);
            case 3:
                return GetPointValue(x + 1, y - 1);
            case 4:
                return GetPointValue(x + 1, y);
            case 5:
                return GetPointValue(x + 1, y + 1);
            case 6:
                return GetPointValue(x, y + 1);
            case 7:
                return GetPointValue(x - 1, y + 1);
            case 8:
                return GetPointValue(x - 1, y);
            case 9:
                return GetPointValue(x - 1, y - 1);
            default:
                return false;
        }
    }

    private bool Step1Passed(int x, int y)
    {
        var n = N(x, y);
        return 2 <= n && n <= 6 && T(x, y) == 1 && (!P(2, x, y) || !P(4, x, y) || !P(6, x, y))
               && (!P(4, x, y) || !P(6, x, y) || !P(8, x, y));
    }

    private bool Step2Passed(int x, int y)
    {
        var n = N(x, y);
        return 2 <= n && n <= 6 && T(x, y) == 1 && (!P(2, x, y) || !P(4, x, y) || !P(8, x, y))
               && (!P(2, x, y) || !P(6, x, y) || !P(8, x, y));
    }

    private void FindBoundaryPoints(PointSet set)
    {
        if (!set.Any())
            set.Clear();

        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (IsBoundaryPoint(x, y))
                    set.Add(new Point(x, y));
    }

    private bool SeedShouldBeAdded(Piece piece, Point point)
    {
        // if it's not out of bounds
        if (point.X < 0 || point.Y < 0 || point.X >= _width || point.Y >= _height)
            return false;

        // if it's black
        if (!_matrix[point.X, point.Y])
            return false;

        // if it's not part of the piece yet
        foreach (var piecePoint in piece)
            if (piecePoint == point)
                return false;

        return true;
    }

    private Piece CreatePiece(PointSet unsorted)
    {
        var piece = new Piece(_matrix);
        PointSet stack = [];
        stack.Push(unsorted.Last());
        while (stack.Any())
        {
            var point = stack.Pop();
            if (SeedShouldBeAdded(piece, point))
            {
                piece.Add(point);
                unsorted.RemovePoint(point);
                stack.Push(new Point(point.X + 1, point.Y));
                stack.Push(new Point(point.X - 1, point.Y));
                stack.Push(new Point(point.X, point.Y + 1));
                stack.Push(new Point(point.X, point.Y - 1));
            }
        }

        piece.CreateStatistics();
        return piece;
    }

    #endregion

    /// <summary>Represents a 2D point in the pixel map.</summary>
    public sealed class Point(int x, int y) : IEquatable<Point>
    {
        public int X { get; } = x;

        public int Y { get; } = y;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            var objAsPoint = obj as Point;

            return !ReferenceEquals(null, objAsPoint) && Equals(objAsPoint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

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

    /// <summary>A stack-like collection of points.</summary>
    internal class PointSet : List<Point>
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
                Remove(point);
        }
    }

    /// <summary>A collection of connected pieces.</summary>
    internal class PieceSet : List<Piece> { }

    /// <summary>
    /// Represents a connected component (piece) in the pixel map with bounding box, centre, and magnitude.
    /// </summary>
    internal class Piece : PointSet
    {
        private readonly bool[,] _matrix;

        public Piece(bool[,] matrix)
        {
            _matrix = matrix;
        }

        /// <summary>The leftmost x-coordinate of the piece.</summary>
        public int MostLeftPoint { get; set; }

        /// <summary>The rightmost x-coordinate of the piece.</summary>
        public int MostRightPoint { get; set; }

        /// <summary>The topmost y-coordinate of the piece.</summary>
        public int MostTopPoint { get; set; }

        /// <summary>The bottommost y-coordinate of the piece.</summary>
        public int MostBottomPoint { get; set; }

        /// <summary>Horizontal centre of the bounding box.</summary>
        public int CenterX { get; set; }

        /// <summary>Vertical centre of the bounding box.</summary>
        public int CenterY { get; set; }

        /// <summary>Ratio of black to total pixels within the bounding box.</summary>
        public float Magnitude { get; set; }

        /// <summary>Number of black pixels in the piece.</summary>
        public int NumberOfBlackPoints { get; set; }

        /// <summary>Total number of pixels within the bounding box.</summary>
        public int NumberOfAllPoints { get; set; }

        /// <summary>Width of the bounding box.</summary>
        public int Width { get; set; }

        /// <summary>Height of the bounding box.</summary>
        public int Height { get; set; }


        /// <summary>
        /// Renders the piece as a binary bitmap within its bounding box.
        /// </summary>
        public SKBitmap? Render()
        {
            if (NumberOfAllPoints == 0)
                return null;

            var image = new SKBitmap(Width, Height);

            for (var x = MostLeftPoint; x <= MostRightPoint; x++)
                for (var y = MostTopPoint; y <= MostBottomPoint; y++)
                    image.SetPixel(x - MostLeftPoint, y - MostTopPoint, _matrix[x, y] ? SKColors.Black : SKColors.White);

            return image;
        }

        /// <summary>Computes bounding box, centre, and magnitude statistics.</summary>
        public void CreateStatistics()
        {
            MostLeftPoint = ComputeMostLeftPoint();
            MostRightPoint = ComputeMostRightPoint();
            MostTopPoint = ComputeMostTopPoint();
            MostBottomPoint = ComputeMostBottomPoint();
            Width = MostRightPoint - MostLeftPoint + 1;
            Height = MostBottomPoint - MostTopPoint + 1;
            CenterX = (MostLeftPoint + MostRightPoint) / 2;
            CenterY = (MostTopPoint + MostBottomPoint) / 2;
            NumberOfBlackPoints = Count;
            NumberOfAllPoints = Width * Height;
            Magnitude = (float)NumberOfBlackPoints / NumberOfAllPoints;
        }

        /// <summary>Cost is the number of white pixels in the bounding box (used for ranking pieces).</summary>
        public int Cost() => NumberOfAllPoints - NumberOfBlackPoints;

        /// <summary>Sets all pixels belonging to this piece to white (false).</summary>
        public void BleachPiece()
        {
            foreach (var point in this)
                _matrix[point.X, point.Y] = false;
        }

        private int ComputeMostLeftPoint() => this.Select(point => point.X).Prepend(int.MaxValue).Min();

        private int ComputeMostRightPoint() => this.Select(point => point.X).Prepend(0).Max();

        private int ComputeMostTopPoint() => this.Select(point => point.Y).Prepend(int.MaxValue).Min();

        private int ComputeMostBottomPoint() => this.Select(point => point.Y).Prepend(0).Max();
    }
}
