using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using dotNETANPR.ImageAnalysis.Util;

namespace dotNETANPR.ImageAnalysis
{
    public class PixelMap
    {
        private Piece bestPiece = null;
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
            private int mostLeftPoint;
            private int mostRightPoint;
            private int mostTopPoint;
            private int mostBottomPoint;
            private int width;
            private int height;
            private int centerX;
            private int centerY;
            private float magnitude;
            private int numberOfBlackPoints;
            private int numberOfAllPoints;

            public Bitmap Render()
            {
                if (numberOfAllPoints == 0)
                {
                    return null;
                }
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
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
                width = mostRightPoint - mostLeftPoint + 1;
                height = mostBottomPoint - mostTopPoint + 1;
                centerX = (mostLeftPoint + mostRightPoint) / 2;
                centerY = (mostTopPoint + mostBottomPoint) / 2;
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
                return width * height;
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
    }
}
