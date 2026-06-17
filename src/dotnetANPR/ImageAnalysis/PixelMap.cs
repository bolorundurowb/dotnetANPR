using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a binary pixel matrix derived from a <see cref="Photo"/>.
/// Provides connected component analysis, skeletonization, noise reduction,
/// and piece extraction for character recognition.
/// </summary>
public class PixelMap
{
    private bool[,] _matrix = null!;
    private Piece? _bestPiece;
    private int _width;
    private int _height;

    /// <summary>
    /// Initializes a new instance of the <see cref="PixelMap"/> class from a <see cref="Photo"/>.
    /// Pixels with brightness below 0.5 are treated as black (true).
    /// </summary>
    /// <param name="photo">The source photo.</param>
    public PixelMap(Photo photo) => MatrixInit(photo);

    /// <summary>
    /// Renders the entire binary matrix as a black-and-white bitmap.
    /// </summary>
    /// <returns>A new bitmap where true values are black and false values are white.</returns>
    public SKBitmap Render()
    {
        var image = new SKBitmap(_width, _height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                image.SetPixel(x, y, _matrix[x, y] ? SKColors.Black : SKColors.White);

        return image;
    }

    /// <summary>
    /// Returns the best (largest by cost) connected component piece, reducing all other pieces.
    /// </summary>
    /// <returns>The best <see cref="Piece"/>, or an empty piece if none found.</returns>
    public Piece BestPiece()
    {
        ReduceOtherPieces();
        return _bestPiece ?? [];
    }

    /// <summary>
    /// Applies Zhang-Suen thinning (skeletonization) to the binary matrix in-place.
    /// </summary>
    /// <returns>This <see cref="PixelMap"/> instance for chaining.</returns>
    public PixelMap Skeletonize()
    {
        PointSet flaggedPoints = [];
        PointSet boundaryPoints = [];
        bool cont;
        do
        {
            cont = false;
            FindBoundaryPoints(boundaryPoints);

            flaggedPoints.AddRange(boundaryPoints.Where(point => Step1Passed(point.X, point.Y)));

            if (flaggedPoints.Count != 0)
                cont = true;

            foreach (var point in flaggedPoints)
            {
                _matrix[point.X, point.Y] = false;
                boundaryPoints.Remove(point);
            }

            flaggedPoints.Clear();

            flaggedPoints.AddRange(boundaryPoints.Where(point => Step2Passed(point.X, point.Y)));

            if (flaggedPoints.Count != 0)
                cont = true;

            foreach (var point in flaggedPoints)
                _matrix[point.X, point.Y] = false;

            boundaryPoints.Clear();
            flaggedPoints.Clear();
        } while (cont);

        return this;
    }

    /// <summary>
    /// Removes isolated pixels (noise) that have fewer than 4 black neighbors.
    /// </summary>
    /// <returns>This <see cref="PixelMap"/> instance for chaining.</returns>
    public PixelMap ReduceNoise()
    {
        PointSet pointsToReduce = [];
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (N(x, y) < 4)
                    pointsToReduce.Add(new Point(x, y));

        foreach (var point in pointsToReduce)
            _matrix[point.X, point.Y] = false;

        return this;
    }

    /// <summary>
    /// Finds all connected components (pieces) in the binary matrix.
    /// </summary>
    /// <returns>A <see cref="PieceSet"/> containing all found pieces.</returns>
    public PieceSet FindPieces()
    {
        PieceSet pieces = [];
        PointSet unsorted = [];
        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (_matrix[x, y])
                    unsorted.Add(new Point(x, y));

        while (unsorted.Count > 0)
            pieces.Add(CreatePiece(unsorted));

        return pieces;
    }

    /// <summary>
    /// Keeps only the best piece (by cost) and bleaches all others from the matrix.
    /// </summary>
    public void ReduceOtherPieces()
    {
        if (_bestPiece != null)
            return;

        var pieces = FindPieces();
        var maxCost = 0;
        var maxIndex = 0;
        for (var i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].Cost() > maxCost)
            {
                maxCost = pieces[i].Cost();
                maxIndex = i;
            }
        }

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
            return false;

        return !GetPointValue(x - 1, y - 1) || !GetPointValue(x - 1, y + 1) || !GetPointValue(x + 1, y - 1)
               || !GetPointValue(x + 1, y + 1) || !GetPointValue(x, y + 1) || !GetPointValue(x, y - 1)
               || !GetPointValue(x + 1, y) || !GetPointValue(x - 1, y);
    }

    private int N(int x, int y)
    {
        var n = 0;
        if (GetPointValue(x - 1, y - 1)) n++;
        if (GetPointValue(x - 1, y + 1)) n++;
        if (GetPointValue(x + 1, y - 1)) n++;
        if (GetPointValue(x + 1, y + 1)) n++;
        if (GetPointValue(x, y + 1)) n++;
        if (GetPointValue(x, y - 1)) n++;
        if (GetPointValue(x + 1, y)) n++;
        if (GetPointValue(x - 1, y)) n++;
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
        return i switch
        {
            1 => GetPointValue(x, y),
            2 => GetPointValue(x, y - 1),
            3 => GetPointValue(x + 1, y - 1),
            4 => GetPointValue(x + 1, y),
            5 => GetPointValue(x + 1, y + 1),
            6 => GetPointValue(x, y + 1),
            7 => GetPointValue(x - 1, y + 1),
            8 => GetPointValue(x - 1, y),
            9 => GetPointValue(x - 1, y - 1),
            _ => false
        };
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
        if (set.Count > 0)
            set.Clear();

        for (var x = 0; x < _width; x++)
            for (var y = 0; y < _height; y++)
                if (IsBoundaryPoint(x, y))
                    set.Add(new Point(x, y));
    }

    private bool SeedShouldBeAdded(Piece piece, Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= _width || point.Y >= _height)
            return false;

        if (!_matrix[point.X, point.Y])
            return false;

        foreach (var piecePoint in piece)
            if (piecePoint == point)
                return false;

        return true;
    }

    private Piece CreatePiece(PointSet unsorted)
    {
        Piece piece = [];
        piece.SetParent(this);
        PointSet stack = [];
        stack.Push(unsorted.Last());
        while (stack.Count > 0)
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

    /// <summary>
    /// Represents a 2D point with integer coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public sealed class Point(int x, int y) : IEquatable<Point>
    {
        /// <summary>
        /// Gets the x-coordinate of the point.
        /// </summary>
        public int X { get; } = x;

        /// <summary>
        /// Gets the y-coordinate of the point.
        /// </summary>
        public int Y { get; } = y;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            var objAsPoint = obj as Point;
            return !ReferenceEquals(null, objAsPoint) && Equals(objAsPoint);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <inheritdoc />
        public bool Equals(Point? other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Determines whether two points are equal.
        /// </summary>
        public static bool operator ==(Point? left, Point? right) => Equals(left, right);

        /// <summary>
        /// Determines whether two points are not equal.
        /// </summary>
        public static bool operator !=(Point? left, Point? right) => !Equals(left, right);
    }

    /// <summary>
    /// A list of <see cref="Point"/> objects with stack-like push/pop operations.
    /// </summary>
    public class PointSet : List<Point>
    {
        /// <summary>
        /// Pushes a point onto the end of the list.
        /// </summary>
        /// <param name="point">The point to add.</param>
        public void Push(Point point) { Add(point); }

        /// <summary>
        /// Removes and returns the last point in the list.
        /// </summary>
        /// <returns>The removed point.</returns>
        public Point Pop()
        {
            var value = this[Count - 1];
            RemoveAt(Count - 1);
            return value;
        }

        /// <summary>
        /// Removes the specified point from the list, if present.
        /// </summary>
        /// <param name="point">The point to remove.</param>
        public void RemovePoint(Point point)
        {
            if (Contains(point))
                Remove(point);
        }
    }

    /// <summary>
    /// A list of <see cref="Piece"/> objects representing connected components.
    /// </summary>
    public class PieceSet : List<Piece> { }

    /// <summary>
    /// Represents a connected component (piece) in the binary pixel matrix.
    /// Contains bounding box statistics and rendering capabilities.
    /// </summary>
    public class Piece : PointSet
    {
        private PixelMap? _parentMap;

        /// <summary>
        /// Gets or sets the x-coordinate of the leftmost point in the piece.
        /// </summary>
        public int MostLeftPoint { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the rightmost point in the piece.
        /// </summary>
        public int MostRightPoint { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the topmost point in the piece.
        /// </summary>
        public int MostTopPoint { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the bottommost point in the piece.
        /// </summary>
        public int MostBottomPoint { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the center of the bounding box.
        /// </summary>
        public int CenterX { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the center of the bounding box.
        /// </summary>
        public int CenterY { get; set; }

        /// <summary>
        /// Gets or sets the ratio of black points to total bounding box area.
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the number of black points in the piece.
        /// </summary>
        public int NumberOfBlackPoints { get; set; }

        /// <summary>
        /// Gets or sets the total number of pixels in the bounding box.
        /// </summary>
        public int NumberOfAllPoints { get; set; }

        /// <summary>
        /// Gets or sets the width of the piece bounding box.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the piece bounding box.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Renders this piece as a black-and-white bitmap.
        /// </summary>
        /// <returns>A bitmap of the piece, or null if the piece has no points.</returns>
        public SKBitmap? Render()
        {
            if (NumberOfAllPoints == 0)
                return null;

            var parentMatrix = GetParentMatrix();
            var image = new SKBitmap(Width, Height, SKColorType.Bgra8888, SKAlphaType.Opaque);

            for (var x = MostLeftPoint; x <= MostRightPoint; x++)
                for (var y = MostTopPoint; y <= MostBottomPoint; y++)
                    image.SetPixel(x - MostLeftPoint, y - MostTopPoint,
                        parentMatrix[x, y] ? SKColors.Black : SKColors.White);

            return image;
        }

        /// <summary>
        /// Computes bounding box statistics for this piece.
        /// </summary>
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

        /// <summary>
        /// Computes the cost of this piece (total area minus black points).
        /// Higher cost generally means a better character candidate.
        /// </summary>
        /// <returns>The cost value.</returns>
        public int Cost() => NumberOfAllPoints - NumberOfBlackPoints;

        /// <summary>
        /// Sets all points in this piece to white (false) in the parent matrix.
        /// </summary>
        public void BleachPiece()
        {
            var parentMatrix = GetParentMatrix();
            foreach (var point in this)
                parentMatrix[point.X, point.Y] = false;
        }

        /// <summary>
        /// Sets the parent <see cref="PixelMap"/> reference for matrix access.
        /// </summary>
        /// <param name="parent">The parent pixel map.</param>
        internal void SetParent(PixelMap parent) => _parentMap = parent;

        private bool[,] GetParentMatrix() =>
            _parentMap?._matrix ?? throw new InvalidOperationException("Piece has no parent PixelMap");

        private int ComputeMostLeftPoint() => this.Select(point => point.X).Prepend(int.MaxValue).Min();

        private int ComputeMostRightPoint() => this.Select(point => point.X).Prepend(0).Max();

        private int ComputeMostTopPoint() => this.Select(point => point.Y).Prepend(int.MaxValue).Min();

        private int ComputeMostBottomPoint() => this.Select(point => point.Y).Prepend(0).Max();
    }
}
