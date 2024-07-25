﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace DotNetANPR.ImageAnalysis;

public class PixelMap(Photo photo)
{
    static bool[,] matrix;
    private Piece? bestPiece = null;
    private int width;
    private int height;

    private void MatrixInit(Photo bi)
    {
        width = bi.Width;
        height = bi.Height;
        matrix = new bool[width, height];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++) 
            matrix[x, y] = bi.GetBrightness(x, y) < 0.5;
    }

    public Bitmap Render()
    {
        var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (matrix[x, y])
                {
                    image.SetPixel(x, y, Color.Black);
                }
                else
                {
                    image.SetPixel(x, y, Color.White);
                }
            }
        }

        return image;
    }

    public Piece GetBestPiece()
    {
        ReduceOtherPieces();
        if (bestPiece == null)
        {
            return [];
        }

        return bestPiece;
    }

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
            foreach (var point in boundaryPoints)
            {
                if (Step1Passed(point.X, point.Y))
                {
                    flaggedPoints.Add(point);
                }
            }

            // delete flagged points
            if (!flaggedPoints.Any())
            {
                cont = true;
            }

            foreach (var point in flaggedPoints)
            {
                matrix[point.X, point.Y] = false;
                boundaryPoints.Remove(point);
            }

            flaggedPoints.Clear();
            // apply step 2 to flag remaining points
            foreach (var point in boundaryPoints)
            {
                if (Step2Passed(point.X, point.Y))
                {
                    flaggedPoints.Add(point);
                }
            }

            // delete flagged points
            if (!flaggedPoints.Any())
            {
                cont = true;
            }

            foreach (var point in flaggedPoints)
            {
                matrix[point.X, point.Y] = false;
            }

            boundaryPoints.Clear();
            flaggedPoints.Clear();
        } while (cont);

        return (this);
    }

    public PixelMap ReduceNoise()
    {
        PointSet pointsToReduce = [];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (N(x, y) < 4)
                {
                    pointsToReduce.Add(new Point(x, y)); // recommended 4
                }
            }
        }

        // remove marked points
        foreach (var point in pointsToReduce)
        {
            matrix[point.X, point.Y] = false;
        }

        return (this);
    }

    public Piece BestPiece() => throw new System.NotImplementedException();

    public PieceSet FindPieces()
    {
        PieceSet pieces = [];
        // put all black points into a set
        PointSet unsorted = [];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (matrix[x, y])
                {
                    unsorted.Add(new Point(x, y));
                }
            }
        }

        while (!unsorted.Any())
        {
            pieces.Add(CreatePiece(unsorted));
        }

        return pieces;
    }

    public void ReduceOtherPieces()
    {
        if (bestPiece != null)
        {
            return; // we've got a best piece already
        }

        PieceSet pieces = FindPieces();
        int maxCost = 0;
        int maxIndex = 0;
        // find the best cost
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].Cost() > maxCost)
            {
                maxCost = pieces[i].Cost();
                maxIndex = i;
            }
        }

        // delete the others
        for (int i = 0; i < pieces.Count; i++)
        {
            if (i != maxIndex)
            {
                pieces[i].BleachPiece();
            }
        }

        if (pieces.Count != 0)
        {
            bestPiece = pieces[maxIndex];
        }
    }

    #region Private Helpers

    private bool GetPointValue(int x, int y)
    {
        if ((x < 0) || (y < 0) || (x >= width) || (y >= height))
        {
            return false;
        }

        return matrix[x, y];
    }

    private bool IsBoundaryPoint(int x, int y)
    {
        if (!GetPointValue(x, y))
        {
            // if it's white (outside points are automatically white)
            return false;
        }

        // a boundary point must have at least one neighbor point that's white
        return !GetPointValue(x - 1, y - 1) || !GetPointValue(x - 1, y + 1) || !GetPointValue(x + 1, y - 1)
               || !GetPointValue(x + 1, y + 1) || !GetPointValue(x, y + 1) || !GetPointValue(x, y - 1)
               || !GetPointValue(x + 1, y) || !GetPointValue(x - 1, y);
    }

    private int N(int x, int y)
    {
        // number of black points in the neighborhood
        int n = 0;
        if (GetPointValue(x - 1, y - 1))
        {
            n++;
        }

        if (GetPointValue(x - 1, y + 1))
        {
            n++;
        }

        if (GetPointValue(x + 1, y - 1))
        {
            n++;
        }

        if (GetPointValue(x + 1, y + 1))
        {
            n++;
        }

        if (GetPointValue(x, y + 1))
        {
            n++;
        }

        if (GetPointValue(x, y - 1))
        {
            n++;
        }

        if (GetPointValue(x + 1, y))
        {
            n++;
        }

        if (GetPointValue(x - 1, y))
        {
            n++;
        }

        return n;
    }

    private int T(int x, int y)
    {
        int n = 0;
        for (int i = 2; i <= 8; i++)
        {
            if (!P(i, x, y) && P(i + 1, x, y))
            {
                n++;
            }
        }

        if (!P(9, x, y) && P(2, x, y))
        {
            n++;
        }

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
        int n = N(x, y);
        return (((2 <= n) && (n <= 6)) && (T(x, y) == 1) && (!P(2, x, y) || !P(4, x, y) || !P(6, x, y))
                && (!P(4, x, y) || !P(6, x, y) || !P(8, x, y)));
    }

    private bool Step2Passed(int x, int y)
    {
        int n = N(x, y);
        return (((2 <= n) && (n <= 6)) && (T(x, y) == 1) && (!P(2, x, y) || !P(4, x, y) || !P(8, x, y))
                && (!P(2, x, y) || !P(6, x, y) || !P(8, x, y)));
    }

    private void FindBoundaryPoints(PointSet set)
    {
        if (!set.Any())
        {
            set.Clear();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (IsBoundaryPoint(x, y))
                {
                    set.Add(new Point(x, y));
                }
            }
        }
    }

    private bool SeedShouldBeAdded(Piece piece, Point point)
    {
        // if it's not out of bounds
        if ((point.X < 0) || (point.Y < 0) || (point.X >= width) || (point.Y >= height))
        {
            return false;
        }

        // if it's black
        if (!matrix[point.X, point.Y])
        {
            return false;
        }

        // if it's not part of the piece yet
        foreach (Point piecePoint in piece)
        {
            if (piecePoint == point)
            {
                return false;
            }
        }

        return true;
    }

    private Piece CreatePiece(PointSet unsorted)
    {
        Piece piece = [];
        PointSet stack = [];
        stack.Push(unsorted.Last());
        while (!stack.Any())
        {
            Point point = stack.Pop();
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
                image.SetPixel(x - MostLeftPoint, y - MostTopPoint, matrix[x, y] ? Color.Black : Color.White);

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

        public int Cost() => NumberOfAllPoints - NumberOfBlackPoints;

        public void BleachPiece()
        {
            foreach (var point in this)
                matrix[point.X, point.Y] = false;
        }

        private int ComputeMostLeftPoint() => this.Select(point => point.X).Prepend(int.MaxValue).Min();

        private int ComputeMostRightPoint() => this.Select(point => point.X).Prepend(0).Max();

        private int ComputeMostTopPoint() => this.Select(point => point.Y).Prepend(int.MaxValue).Min();

        private int ComputeMostBottomPoint() => this.Select(point => point.Y).Prepend(0).Max();
    }
}
