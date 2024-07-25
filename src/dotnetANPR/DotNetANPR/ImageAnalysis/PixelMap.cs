using System;
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
    
    private void MatrixInit(Photo bi) {
        width = bi.Width;
        height = bi.Height;
        matrix = new bool[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                matrix[x,y] = bi.GetBrightness(x, y) < 0.5;
            }
        }
    }
    
    public Bitmap Render() {
        var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x,y]) {
                    image.SetPixel(x, y, Color.Black);
                } else {
                    image.SetPixel(x, y, Color.White);
                }
            }
        }
        return image;
    }

    public Piece GetBestPiece() {
        ReduceOtherPieces();
        if (bestPiece == null) {
            return [];
        }
        return bestPiece;
    }
    
    public PixelMap skeletonize() {
        PointSet flaggedPoints = [];
        PointSet boundaryPoints = [];
        boolean cont;
        do {
            cont = false;
            findBoundaryPoints(boundaryPoints);
            // apply step 1 to flag boundary points for deletion
            for (Point p : boundaryPoints) {
                if (step1passed(p.x, p.y)) {
                    flaggedPoints.add(p);
                }
            }
            // delete flagged points
            if (!flaggedPoints.isEmpty()) {
                cont = true;
            }
            for (Point p : flaggedPoints) {
                matrix[p.x][p.y] = false;
                boundaryPoints.remove(p);
            }
            flaggedPoints.clear();
            // apply step 2 to flag remaining points
            for (Point p : boundaryPoints) {
                if (step2passed(p.x, p.y)) {
                    flaggedPoints.add(p);
                }
            }
            // delete flagged points
            if (!flaggedPoints.isEmpty()) {
                cont = true;
            }
            for (Point p : flaggedPoints) {
                matrix[p.x][p.y] = false;
            }
            boundaryPoints.clear();
            flaggedPoints.clear();
        } while (cont);
        return (this);
    }

    public PixelMap reduceNoise() {
        PointSet pointsToReduce = [];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (n(x, y) < 4) {
                    pointsToReduce.add(new Point(x, y)); // recommended 4
                }
            }
        }
        // remove marked points
        for (Point p : pointsToReduce) {
            matrix[p.x][p.y] = false;
        }
        return (this);
    }

    public Piece BestPiece() => throw new System.NotImplementedException();
    
    public PieceSet findPieces() {
        PieceSet pieces = [];
        // put all black points into a set
        PointSet unsorted = [];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (matrix[x][y]) {
                    unsorted.add(new Point(x, y));
                }
            }
        }
        while (!unsorted.isEmpty()) {
            pieces.add(createPiece(unsorted));
        }
        return pieces;
    }
    
    public void reduceOtherPieces() {
        if (bestPiece != null) {
            return; // we've got a best piece already
        }
        PieceSet pieces = findPieces();
        int maxCost = 0;
        int maxIndex = 0;
        // find the best cost
        for (int i = 0; i < pieces.size(); i++) {
            if (pieces.get(i).cost() > maxCost) {
                maxCost = pieces.get(i).cost();
                maxIndex = i;
            }
        }
        // delete the others
        for (int i = 0; i < pieces.size(); i++) {
            if (i != maxIndex) {
                pieces.get(i).bleachPiece();
            }
        }
        if (pieces.size() != 0) {
            bestPiece = pieces.get(maxIndex);
        }
    }
    
    #region Private Helpers
    
    private bool GetPointValue(int x, int y) {
        if ((x < 0) || (y < 0) || (x >= width) || (y >= height)) {
            return false;
        }
        return matrix[x,y];
    }

    private bool IsBoundaryPoint(int x, int y) {
        if (!GetPointValue(x, y)) { // if it's white (outside points are automatically white)
            return false;
        }
        // a boundary point must have at least one neighbor point that's white
        return !GetPointValue(x - 1, y - 1) || !GetPointValue(x - 1, y + 1) || !GetPointValue(x + 1, y - 1)
               || !GetPointValue(x + 1, y + 1) || !GetPointValue(x, y + 1) || !GetPointValue(x, y - 1)
               || !GetPointValue(x + 1, y) || !GetPointValue(x - 1, y);
    }

    private int N(int x, int y) { // number of black points in the neighborhood
        int n = 0;
        if (GetPointValue(x - 1, y - 1)) {
            n++;
        }
        if (GetPointValue(x - 1, y + 1)) {
            n++;
        }
        if (GetPointValue(x + 1, y - 1)) {
            n++;
        }
        if (GetPointValue(x + 1, y + 1)) {
            n++;
        }
        if (GetPointValue(x, y + 1)) {
            n++;
        }
        if (GetPointValue(x, y - 1)) {
            n++;
        }
        if (GetPointValue(x + 1, y)) {
            n++;
        }
        if (GetPointValue(x - 1, y)) {
            n++;
        }
        return n;
    }
    
    private int T(int x, int y) {
        int n = 0;
        for (int i = 2; i <= 8; i++) {
            if (!P(i, x, y) && P(i + 1, x, y)) {
                n++;
            }
        }
        if (!P(9, x, y) && P(2, x, y)) {
            n++;
        }
        return n;
    }
    
    private bool P(int i, int x, int y) {
        switch (i) {
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
    
    private bool Step1Passed(int x, int y) {
        int n = n(x, y);
        return (((2 <= n) && (n <= 6)) && (t(x, y) == 1) && (!p(2, x, y) || !p(4, x, y) || !p(6, x, y))
                && (!p(4, x, y) || !p(6, x, y) || !p(8, x, y)));
    }

    private bool Step2Passed(int x, int y) {
        int n = N(x, y);
        return (((2 <= n) && (n <= 6)) && (t(x, y) == 1) && (!p(2, x, y) || !p(4, x, y) || !p(8, x, y))
                && (!p(2, x, y) || !p(6, x, y) || !p(8, x, y)));
    }

    private void FindBoundaryPoints(PointSet set) {
        if (!set.Any()) {
            set.Clear();
        }
        
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (IsBoundaryPoint(x, y)) {
                    set.Add(new Point(x, y));
                }
            }
        }
    }
    
    private boolean seedShouldBeAdded(Piece piece, Point p) {
        // if it's not out of bounds
        if ((p.x < 0) || (p.y < 0) || (p.x >= width) || (p.y >= height)) {
            return false;
        }
        // if it's black
        if (!matrix[p.x][p.y]) {
            return false;
        }
        // if it's not part of the piece yet
        for (Point piecePoint : piece) {
            if (piecePoint.equals(p)) {
                return false;
            }
        }
        return true;
    }
    
    private Piece createPiece(PointSet unsorted) {
        Piece piece = [];
        PointSet stack = [];
        stack.push(unsorted.lastElement());
        while (!stack.isEmpty()) {
            Point p = stack.pop();
            if (seedShouldBeAdded(piece, p)) {
                piece.add(p);
                unsorted.removePoint(p);
                stack.push(new Point(p.x + 1, p.y));
                stack.push(new Point(p.x - 1, p.y));
                stack.push(new Point(p.x, p.y + 1));
                stack.push(new Point(p.x, p.y - 1));
            }
        }
        piece.createStatistics();
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
                image.SetPixel(x - MostLeftPoint, y - MostTopPoint, matrix[x,y] ? Color.Black : Color.White);

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
