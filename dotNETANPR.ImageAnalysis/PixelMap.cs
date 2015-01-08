using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.ImageAnalysis
{
    public  class PixelMap
    {
        private class Point
        {
            public int x;
            public int y;
            bool[,] matrix = null;
            //     bool deleted;
            public Point(bool[,] outerMatrix)
            {
                this.matrix = outerMatrix;
            }
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
                //        this.deleted = false;
            }
            public bool equals(Point p2)
            {
                if (p2.x == this.x && p2.y == this.y) return true;
                return false;
            }
            public bool equals(int x, int y)
            {
                if (x == this.x && y == this.y) return true;
                return false;
            }
            public bool value()
            {
                if (matrix == null)
                {
                    throw new NullReferenceException("The refernced matrix is empty and hasnt been initialized");
                }
                else
                {
                    return matrix[x, y];
                }
            }
        }

        private class PointSet : List<Point>
        {
            static readonly long serialVersionUID = 0;
            public void removePoint(Point p)
            {
                Point toRemove = null;
                foreach (Point px in this)
                {
                    if (px.Equals(p)) toRemove = px;
                }
                this.Remove(toRemove);
            }

        }

        private  class PieceSet : List<Piece>
        {
            static readonly long serialVersionUID = 0;
        }
        private Piece bestPiece = null;


        public  class Piece : PointSet
        {
            static readonly long serialVersionUID = 0;
            public int mostLeftPoint;
            public int mostRightPoint;
            public int mostTopPoint;
            public int mostBottomPoint;
            public int width;
            public int height;
            public int centerX;
            public int centerY;
            public float magnitude;
            public int numberOfBlackPoints;
            public int numberOfAllPoints;
            public PixelMap outerClass = new PixelMap();
            public Bitmap render()
            {
                if (numberOfAllPoints == 0)
                    return null;
                Bitmap image = new Bitmap(this.width, this.height, PixelFormat.Format8bppIndexed);
                for (int x = this.mostLeftPoint; x <= this.mostRightPoint; x++)
                {
                    for (int y = this.mostTopPoint; y <= this.mostBottomPoint; y++)
                    {
                        if (outerClass.matrix[x, y])
                        {
                            image.SetPixel(x - this.mostLeftPoint, y - this.mostTopPoint, Color.Black);
                        }
                        else
                        {
                            image.SetPixel(x - this.mostLeftPoint, y - this.mostTopPoint, Color.White);
                        }
                    }
                }
                return image;
            }

            public void createStatistics()
            {
                this.mostLeftPoint = this.MostLeftPoint;
                this.mostRightPoint = this.MostRightPoint;
                this.mostTopPoint = this.MostTopPoint;
                this.mostBottomPoint = this.MostBottomPoint;
                this.width = this.mostRightPoint - this.mostLeftPoint + 1;
                this.height = this.mostBottomPoint - this.mostTopPoint + 1;
                this.centerX = (this.mostLeftPoint + this.mostRightPoint) / 2;
                this.centerY = (this.mostTopPoint + this.mostBottomPoint) / 2;
                this.numberOfBlackPoints = this.NumberOfBlackPoints;
                this.numberOfAllPoints = this.NumberOfAllPoints;
                this.magnitude = this.Magnitude;
            }
            public int cost()
            { // vypocita ako velmi sa piece podoba pismenku
                return this.numberOfAllPoints - NumberOfBlackPoints;
            }
            public void bleachPiece()
            {
                foreach (Point p in this)
                {
                    outerClass.matrix[p.x, p.y] = false;
                }
            }
            private float Magnitude
            {
                get
                {
                    return ((float)this.numberOfBlackPoints / this.numberOfAllPoints);
                }
            }
            private int NumberOfBlackPoints
            {
                get
                {
                    return this.Count;
                }
            }
            private int NumberOfAllPoints
            {
                get
                {
                    return this.width * this.height;
                }
            }

            private int MostLeftPoint
            {
                get
                {
                    int position = Int32.MaxValue;
                    foreach (Point p in this) position = Math.Min(position, p.x);
                    return position;
                }
            }
            private int MostRightPoint
            {
                get
                {
                    int position = 0;
                    foreach (Point p in this) position = Math.Max(position, p.x);
                    return position;
                }
            }
            private int MostTopPoint
            {
                get
                {
                    int position = Int32.MaxValue;
                    foreach (Point p in this) position = Math.Min(position, p.y);
                    return position;
                }
            }
            private int MostBottomPoint
            {
                get
                {
                    int position = 0;
                    foreach (Point p in this) position = Math.Max(position, p.y);
                    return position;
                }
            }
        }


        // row column
        public bool[,] matrix;
        private int width;
        private int height;

        public PixelMap(Photo bi)
        {
            this.matrixInit(bi);
        }

        public PixelMap()
        {

        }

        public void matrixInit(Photo bi)
        {
            this.width = bi.getWidth();
            this.height = bi.getHeight();

            this.matrix = new bool[this.width, this.height];

            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    this.matrix[x, y] = bi.getBrightness(x, y) < 0.5;
                }
            }
        }

        // renderuje celu maticu
        public Bitmap render()
        {
            Bitmap image = new Bitmap(this.width, this.height, PixelFormat.Format8bppIndexed);
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    if (this.matrix[x, y])
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

        public Piece getBestPiece()
        {
            this.reduceOtherPieces();
            if (this.bestPiece == null) return new Piece();
            return this.bestPiece;
        }

        private bool getPointValue(int x, int y)
        {
            // body mimo su automaticky biele
            if (x < 0 || y < 0 || x >= this.width || y >= this.height) return false;
            return matrix[x, y];
        }

        private bool isBoundaryPoint(int x, int y)
        {

            if (!getPointValue(x, y)) return false; // ak je bod biely, return false

            // konturovy bod ma aspon jeden bod v okoli biely
            if (!getPointValue(x - 1, y - 1) ||
                !getPointValue(x - 1, y + 1) ||
                !getPointValue(x + 1, y - 1) ||
                !getPointValue(x + 1, y + 1) ||
                !getPointValue(x, y + 1) ||
                !getPointValue(x, y - 1) ||
                !getPointValue(x + 1, y) ||
                !getPointValue(x - 1, y)) return true;

            return false;
        }




        private int n(int x, int y)
        { // pocet ciernych bodov v okoli
            int n = 0;
            if (getPointValue(x - 1, y - 1)) n++;
            if (getPointValue(x - 1, y + 1)) n++;
            if (getPointValue(x + 1, y - 1)) n++;
            if (getPointValue(x + 1, y + 1)) n++;
            if (getPointValue(x, y + 1)) n++;
            if (getPointValue(x, y - 1)) n++;
            if (getPointValue(x + 1, y)) n++;
            if (getPointValue(x - 1, y)) n++;
            return n;
        }

        // number of 0-1 transitions in ordered sequence 2,3,...,8,9,2
        private int t(int x, int y)
        {
            int n = 0; // number of 0-1 transitions
            // proceeding tranisions 2-3, 3-4, 4-5, 5-6, 6-7, 7-8, 8-9, 9-2
            for (int i = 2; i <= 8; i++)
            {
                if (!p(i, x, y) && p(i + 1, x, y)) n++;
            }
            if (!p(9, x, y) && p(2, x, y)) n++;
            return n;
        }

        /** okolie bodu p1
         *     p9  p2  p3
         *     p8  p1  p4
         *     p7  p6  p5
         */
        private bool p(int i, int x, int y)
        {
            if (i == 1) return getPointValue(x, y);
            if (i == 2) return getPointValue(x, y - 1);
            if (i == 3) return getPointValue(x + 1, y - 1);
            if (i == 4) return getPointValue(x + 1, y);
            if (i == 5) return getPointValue(x + 1, y + 1);
            if (i == 6) return getPointValue(x, y + 1);
            if (i == 7) return getPointValue(x - 1, y + 1);
            if (i == 8) return getPointValue(x - 1, y);
            if (i == 9) return getPointValue(x - 1, y - 1);
            return false;
        }

        private bool step1passed(int x, int y)
        {
            int m = n(x, y);
            return (
               (2 <= m && m <= 6) &&
               t(x, y) == 1 &&
               (!p(2, x, y) || !p(4, x, y) || !p(6, x, y)) &&
               (!p(4, x, y) || !p(6, x, y) || !p(8, x, y))
        );
        }
        private bool step2passed(int x, int y)
        {
            int m = n(x, y);
            return (
               (2 <= m && m <= 6) &&
               t(x, y) == 1 &&
               (!p(2, x, y) || !p(4, x, y) || !p(8, x, y)) &&
               (!p(2, x, y) || !p(6, x, y) || !p(8, x, y))
            );
        }
        private void findBoundaryPoints(PointSet set)
        {
            if (!set.Any()) set.Clear();
            for (int x = 0; x < this.width; x++)
            {
                for (int y = 0; y < this.height; y++)
                {
                    if (isBoundaryPoint(x, y)) set.Add(new Point(x, y));
                }
            }
        }

        public PixelMap skeletonize()
        { // vykona skeletonizaciu
        // thinning procedure
        PointSet flaggedPoints = new PointSet();
        PointSet boundaryPoints = new PointSet();
        bool cont;
        
        do 
        {
            cont = false;
            findBoundaryPoints(boundaryPoints);
            // apply step 1 to flag boundary points for deletion
            foreach (Point p in boundaryPoints) 
            {
                if (step1passed(p.x, p.y)) flaggedPoints.Add(p);
            }
            // delete flagged points
            if (!flaggedPoints.Any()) cont = true;
            foreach (Point p in flaggedPoints) 
            {
                this.matrix[p.x,p.y] = false;
                boundaryPoints.Remove(p);
            }
            flaggedPoints.Clear();
            // apply step 2 to flag remaining points
            foreach (Point p in boundaryPoints) 
            {
                if (step2passed(p.x, p.y)) flaggedPoints.Add(p);
            }            
            // delete flagged points
            if (!flaggedPoints.Any()) cont = true;
            foreach (Point p in flaggedPoints) 
            {
                this.matrix[p.x,p.y] = false;
            } 
            boundaryPoints.Clear();
            flaggedPoints.Clear();
        } while (cont);
        
        return (this);
    }

        // redukcia sumu /////////////////////////////

        public PixelMap reduceNoise()
        {
        PointSet pointsToReduce = new PointSet();
        for (int x=0; x<this.width; x++) 
        {
            for (int y=0; y<this.height; y++)
            {
                if (n(x,y) < 4) pointsToReduce.Add(new Point(x,y)); // doporucene 4
            }
        }
        // zmazemee oznacene body
        foreach (Point p in pointsToReduce) this.matrix[p.x,p.y] = false;
        return (this);
    }

        // reduce other pieces /////////////////////////////

        private bool isInPieces(PieceSet pieces, int x, int y)
        {
        foreach (Piece piece in pieces) // pre vsetky kusky
            foreach (Point point in piece) // pre vsetky body na kusku
                if (point.equals(x,y)) return true;
        
        return false;
    }

        private bool seedShouldBeAdded(Piece piece, Point p) 
        {
        // ak sa nevymyka okrajom 
        if (p.x<0 || p.y<0 || p.x>=this.width || p.y>=this.height) return false;
        // ak je cierny, 
        if (!this.matrix[p.x,p.y]) return false;
        // ak este nie je sucastou ziadneho kuska
        foreach (Point piecePoint in piece)
            if (piecePoint.Equals(p)) return false;
        return true;
    }

        // vytvori novy piece, najde okolie (piece) napcha donho vsetky body a vrati
        // vstupom je nejaka mnozina "ciernych" bodov, z ktorej algoritmus tie
        // body  vybera
        private Piece createPiece(PointSet unsorted)
        {

            Piece piece = new Piece();

            PointSet stack = new PointSet();
            stack.Insert(0,unsorted.Last());

            while (!stack.Any())
            {
                Point p = stack[0];
                stack.RemoveAt(0);
                if (seedShouldBeAdded(piece, p))
                {
                    piece.Add(p);
                    unsorted.removePoint(p);
                    stack.Insert(0, new Point(p.x + 1, p.y));
                    stack.Insert(0, new Point(p.x - 1, p.y));
                    stack.Insert(0, new Point(p.x, p.y + 1));
                    stack.Insert(0, new Point(p.x, p.y - 1));
                }
            }
            piece.createStatistics();
            return piece;
        }

        private PieceSet findPieces()
        {
            //bool continueFlag;
            PieceSet pieces = new PieceSet();

            // vsetky cierne body na kopu.
            PointSet unsorted = new PointSet();
            for (int x = 0; x < this.width; x++)
                for (int y = 0; y < this.height; y++)
                    if (this.matrix[x,y]) unsorted.Add(new Point(x, y));

            // pre kazdy cierny bod z kopy, 
            while (!unsorted.Any())
            {
                // createPiece vytvori novy piece s tym ze vsetky pouzite body vyhodi von z kopy

                pieces.Add(createPiece(unsorted));
            }
            /*
            do {
                continueFlag = false;
                bool loopBreak = false;
                for (int x = 0; x<this.width; x++) {
                    for (int y = 0; y<this.height; y++) { // for each pixel
                        // ak je pixel cierny, a nie je sucastou ziadneho kuska
                        if (this.matrix[x][y] && !isInPieces(pieces,x,y)) {
                            continueFlag = true;
                            pieces.add(createPiece(x,y));
                        } 
                    }// for y
                } // for x
            } while (continueFlag);
             */
            return pieces;
        }


        // redukuje ostatne pieces a vracia ten najlepsi
        public PixelMap reduceOtherPieces()
        {
            if (this.bestPiece != null) return this; // bestPiece uz je , netreba dalej nic

            PieceSet pieces = findPieces();
            int maxCost = 0;
            int maxIndex = 0;
            // najdeme najlepsi cost
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces.ElementAt(i).cost() > maxCost)
                {
                    maxCost = pieces.ElementAt(i).cost();
                    maxIndex = i;
                }
            }

            // a ostatne zmazeme
            for (int i = 0; i < pieces.Count; i++)
            {
                if (i != maxIndex) pieces.ElementAt(i).bleachPiece();
            }
            if (pieces.Count != 0) this.bestPiece = pieces.ElementAt(maxIndex);
            return this;
        }
    }
}
