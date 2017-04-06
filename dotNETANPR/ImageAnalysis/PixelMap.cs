using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using dotNETANPR.ImageAnalysis.Util;

namespace dotNETANPR.ImageAnalysis
{
    public class PixelMap
    {
        private Piece bestPiece;
        private int height;
        private int width;
        public static bool[,] matrix;

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public bool Equals(Point point)
            {
                return point.X == X && point.Y == Y;
            }

            public bool Equals(int x, int y)
            {
                return x == X && y == Y;
            }

            public bool Value()
            {
                return matrix[X, Y];
            }
        }

        public class PointSet : CustomStack<Point>
        {
            public static long SerialVersionuid = 0;

            public void RemovePoint(Point point)
            {
                Point toRemove = null;
                foreach (Point px in this)
                {
                    if (px.Equals(point))
                    {
                        toRemove = px;
                    }
                }
                Remove(toRemove);
            }
        }

        public class PieceSet : List<Piece>
        {
            public static long SerialVersionuid = 0;
        }
        
        public class Piece : PointSet
        {
            public static long SerialVersionuid = 0;
            public int mostLeftPoint;
            public int mostRightPoint;
            public int mostTopPoint;
            public int mostBottomPoint;
            public int Width;
            public int Height;
            public int CenterX;
            public int CenterY;
            public float magnitude;
            public int numberOfBlackPoints;
            public int numberOfAllPoints;

            public Bitmap Render()
            {
                if (numberOfAllPoints == 0)
                {
                    return null;
                }
                Bitmap bitmap = new Bitmap(Width, Height);
                for (int x = mostLeftPoint; x < mostRightPoint; x++)
                {
                    for (int y = mostTopPoint; y < mostBottomPoint; y++)
                    {
                        if (matrix[x, y])
                        {
                            bitmap.SetPixel(x - mostLeftPoint,
                                y - mostTopPoint,
                                Color.Black);
                        }
                        else
                        {
                            bitmap.SetPixel(x - mostLeftPoint,
                                y - mostTopPoint,
                                Color.White);
                        }
                    }
                }
                return bitmap;
            }

            public void CreateStatistics()
            {
                mostLeftPoint = MostLeftPoint();
                mostRightPoint = MostRightPoint();
                mostTopPoint = MostTopPoint();
                mostBottomPoint = MostBottomPoint();
                Width = mostRightPoint - mostLeftPoint + 1;
                Height = mostBottomPoint - mostTopPoint + 1;
                CenterX = (mostLeftPoint + mostRightPoint) / 2;
                CenterY = (mostTopPoint + mostBottomPoint) / 2;
                numberOfBlackPoints = NumberOfBlackPoints();
                numberOfAllPoints = NumberOfAllPoints();
                magnitude = Magnitude();
            }

            public int Cost()
            {
                return numberOfAllPoints - NumberOfBlackPoints();
            }

            public void BleachPiece()
            {
                foreach (Point point in this)
                {
                    matrix[point.X, point.Y] = false;
                }
            }

            private float Magnitude()
            {
                return (float) numberOfBlackPoints / numberOfAllPoints;
            }

            private int NumberOfAllPoints()
            {
                return Width * Height;
            }

            private int NumberOfBlackPoints()
            {
                return Count;
            }

            private int MostRightPoint()
            {
                int position = 0;
                foreach (Point point in this)
                {
                    position = Math.Max(position, point.X);
                }
                return position;
            }

            private int MostLeftPoint()
            {
                int position = Int32.MaxValue;
                foreach (Point point in this)
                {
                    position = Math.Min(position, point.X);
                }
                return position;
            }

            private int MostBottomPoint()
            {
                int position = 0;
                foreach (Point point in this)
                {
                    position = Math.Max(position, point.Y);
                }
                return position;
            }

            private int MostTopPoint()
            {
                int position = Int32.MaxValue;
                foreach (Point point in this)
                {
                    position = Math.Min(position, point.Y);
                }
                return position;
            }
        }

        public PixelMap(Photo photo)
        {
            MatrixInit(photo);
        }

        public void MatrixInit(Photo photo)
        {
            width = photo.GetWidth();
            height = photo.GetHeight();
            matrix = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    matrix[x, y] = photo.GetBrightness(x, y) < 0.5;
                }
            }
        }

        public Bitmap Render()
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (matrix[x, y])
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.White);
                    }
                }
            }
            return bitmap;
        }

        public Piece GetBestPiece()
        {
            ReduceOtherPieces();
            if (bestPiece == null)
            {
                return new Piece();
            }
            return bestPiece;
        }

        private bool GetPointValue(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return false;
            }
            return matrix[x, y];
        }

        private bool IsBoundaryPoint(int x, int y)
        {
            if (!GetPointValue(x, y))
            {
                return false;
            }

            if (!GetPointValue(x - 1, y - 1) ||
                !GetPointValue(x - 1, y + 1) ||
                !GetPointValue(x + 1, y - 1) ||
                !GetPointValue(x + 1, y + 1) ||
                !GetPointValue(x, y + 1) ||
                !GetPointValue(x, y - 1) ||
                !GetPointValue(x + 1, y) ||
                !GetPointValue(x - 1, y))
            {
                return true;
            }
            return false;
        }

        private int N(int x, int y)
        {
            int n = 0;
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
            int n = 0;
            for (int i = 2; i <= 8; i++)
            {
                if (!P(i, x, y) && P(i + 1, x, y)) n++;
            }
            if (!P(9, x, y) && P(2, x, y)) n++;
            return n;
        }

        private bool P(int i, int x, int y)
        {
            if (i == 1) return GetPointValue(x, y);
            if (i == 2) return GetPointValue(x, y - 1);
            if (i == 3) return GetPointValue(x + 1, y - 1);
            if (i == 4) return GetPointValue(x + 1, y);
            if (i == 5) return GetPointValue(x + 1, y + 1);
            if (i == 6) return GetPointValue(x, y + 1);
            if (i == 7) return GetPointValue(x - 1, y + 1);
            if (i == 8) return GetPointValue(x - 1, y);
            if (i == 9) return GetPointValue(x - 1, y - 1);
            return false;
        }

        private bool Step1Passed(int x, int y)
        {
            int n = N(x, y);
            return 2 <= n && n <= 6 &&
                   T(x, y) == 1 &&
                   (!P(2, x, y) || !P(4, x, y) || !P(6, x, y)) &&
                   (!P(4, x, y) || !P(6, x, y) || !P(8, x, y));
        }

        private bool Step2Passed(int x, int y)
        {
            int n = N(x, y);
            return 2 <= n && n <= 6 &&
                   T(x, y) == 1 &&
                   (!P(2, x, y) || !P(4, x, y) || !P(8, x, y)) &&
                   (!P(2, x, y) || !P(6, x, y) || !P(8, x, y));
        }

        private void FindBoundaryPoints(PointSet set)
        {
            if (set.Count != 0) set.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsBoundaryPoint(x, y)) set.Add(new Point(x, y));
                }
            }
        }

        public PixelMap Skeletonize()
        {
            PointSet flaggedPoints = new PointSet();
            PointSet boundaryPoints = new PointSet();
            bool cont;

            do
            {
                cont = false;
                FindBoundaryPoints(boundaryPoints);

                foreach (Point p in boundaryPoints)
                {
                    if (Step1Passed(p.X, p.Y))
                        flaggedPoints.Add(p);
                }

                if (flaggedPoints.Count != 0)
                    cont = true;
                foreach (Point p in flaggedPoints)
                {
                    matrix[p.X, p.Y] = false;
                    boundaryPoints.Remove(p);
                }
                flaggedPoints.Clear();

                foreach (Point p in boundaryPoints)
                {
                    if (Step2Passed(p.X, p.Y))
                        flaggedPoints.Add(p);
                }

                if (flaggedPoints.Count != 0) cont = true;
                foreach (Point p in flaggedPoints)
                {
                    matrix[p.X, p.Y] = false;
                }
                boundaryPoints.Clear();
                flaggedPoints.Clear();
            } while (cont);
            return this;
        }

        public PixelMap ReduceNoise()
        {
            PointSet pointsToReduce = new PointSet();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (N(x, y) < 4)
                    {
                        pointsToReduce.Add(new Point(x, y));
                    }
                }
            }

            foreach (Point point in pointsToReduce)
            {
                matrix[point.X, point.Y] = false;
            }
            return this;
        }

        public bool IsInPieces(PieceSet pieces, int x, int y)
        {
            foreach (Piece piece in pieces)
            {
                foreach (Point point in piece)
                {
                    if (point.Equals(x, y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SeedShouldBeAdded(Piece piece, Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= width || point.Y >= height)
            {
                return false;
            }
            if (!matrix[point.X, point.Y])
            {
                return false;
            }
            foreach (Point piecePoint in piece)
            {
                if (piecePoint.Equals(point))
                {
                    return false;
                }
            }
            return true;
        }

        private Piece CreatePiece(PointSet unsorted)
        {
            Piece piece = new Piece();

            PointSet stack = new PointSet();
            stack.Push(unsorted.FindLast(x => true));

            while (stack.Count != 0)
            {
                Point p = stack.Pop();
                if (SeedShouldBeAdded(piece, p))
                {
                    piece.Add(p);
                    unsorted.RemovePoint(p);
                    stack.Push(new Point(p.X + 1, p.Y));
                    stack.Push(new Point(p.X - 1, p.Y));
                    stack.Push(new Point(p.X, p.Y + 1));
                    stack.Push(new Point(p.X, p.Y - 1));
                }
            }
            piece.CreateStatistics();
            return piece;
        }

        public PieceSet FindPieces()
        {
            PieceSet pieces = new PieceSet();
            PointSet unsorted = new PointSet();
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

            while (unsorted.Count != 0)
            {
                pieces.Add(CreatePiece(unsorted));
            }
            return pieces;
        }

        public PixelMap ReduceOtherPieces()
        {
            if (bestPiece != null)
            {
                return this;
            }
            PieceSet pieces = FindPieces();
            int maxCost = 0;
            int maxIndex = 0;
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i].Cost() > maxCost)
                {
                    maxCost = pieces[i].Cost();
                    maxIndex = i;
                }
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                if (i != maxIndex) pieces[i].BleachPiece();
            }
            if (pieces.Count != 0) bestPiece = pieces[maxIndex];
            return this;
        }
    }
}
