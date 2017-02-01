using System.Collections.Generic;

namespace dotNETANPR.ImageAnalysis
{
    public class PixelMap
    {
        private Piece bestPiece = null;
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
                //TODO: replace hardcoded value with evaluated value
//                return matrix[X, Y];
                return true;
            }
        }

        public class PointSet : Stack<Point>
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
                RemovePoint(toRemove);
            }
        }

        public class PieceSet : List<Piece>
        {
            public static long SerialVersionuid = 0;
        }
        
        public class Piece : PointSet
        {
            
        }
    }
}
