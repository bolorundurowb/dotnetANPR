using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace dotNETANPR.Recognizer
{
    public abstract class CharacterRecognizer
    {

        // rozpoznavane pismena :
        // 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 30 31 32 33 34 35
        // 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z
        public static char[] alphabet = 
        {
        '0','1','2','3','4','5','6','7','8','9','A','B','C',
        'D','E','F','G','H','I','J','K','L','M','N','O','P',
        'Q','R','S','T','U','V','W','X','Y','Z'
        };

        public static float[,] features =
        {
        {0,1,0,1}, //0
        {1,0,1,0}, //1
        {0,0,1,1}, //2
        {1,1,0,0}, //3
        {0,0,0,1}, //4        
        {1,0,0,0}, //5
        {1,1,1,0}, //6        
        {0,1,1,1}, //7
        {0,0,1,0}, //8        
        {0,1,0,0}, //9
        {1,0,1,1}, //10       
        {1,1,0,1}  //11    
        };

        public class RecognizedChar
        {
            public class RecognizedPattern
            {
                public char chr;
                public float cost;
                public RecognizedPattern(char chr, float value)
                {
                    this.chr = chr;
                    this.cost = value;
                }
                public char getChar
                {
                    get
                    {
                        return this.chr;
                    }
                }
                public float getCost
                {
                    get
                    {
                        return this.cost;
                    }
                }
            }
            public class PatternComparer : IComparer<RecognizedPattern>
            {
                int direction;
                public PatternComparer(int direction)
                {
                    this.direction = direction;
                }
                public int Compare(RecognizedPattern o1, RecognizedPattern o2)
                {
                    float cost1 = ((RecognizedPattern)o1).getCost;
                    float cost2 = ((RecognizedPattern)o2).getCost;

                    int ret = 0;

                    if (cost1 < cost2) ret = -1;
                    if (cost1 > cost2) ret = 1;
                    if (direction == 1) ret *= -1;
                    return ret;
                }
            }

            private List<RecognizedPattern> patterns;
            private bool isSorted;

            public RecognizedChar()
            {
                this.patterns = new List<RecognizedPattern>();
                this.isSorted = false;
            }
            public void addPattern(RecognizedPattern pattern)
            {
                this.patterns.Add(pattern);
            }
            public bool IsSorted
            {
                get
                {
                    return this.isSorted;
                }
            }
            public void sort(int direction)
            {
                if (this.isSorted) return;
                this.isSorted = true;
                patterns.Sort(new PatternComparer(direction));
            }

            public List<RecognizedPattern> getPatterns()
            {
                if (this.isSorted) return this.patterns;
                return null; // if not sorted
            }
            public RecognizedPattern getPattern(int i)
            {
                if (this.isSorted) return this.patterns.ElementAt(i);
                return null;
            }

            public Bitmap render()
            {
                int width = 500;
                int height = 200;
                Bitmap histogram = new Bitmap(width + 20, height + 20, PixelFormat.Format8bppIndexed);
                Graphics graphic = Graphics.FromImage(histogram);

                Pen graphicPen = new Pen(Color.LightGray);
                SolidBrush graphicBrush = new SolidBrush(Color.LightGray);
                Rectangle backRect = new Rectangle(0, 0, width + 20, height + 20);
                graphic.FillRectangle(graphicBrush, backRect);
                graphic.DrawRectangle(graphicPen, backRect);

                graphicPen.Color = Color.Black;
                graphicBrush.Color = Color.Black;
                Font graphicFont = new Font("Consolas", 20);

                int colWidth = width / this.patterns.Count;
                int left, top;


                for (int ay = 0; ay <= 100; ay += 10)
                {
                    int y = 15 + (int)((100 - ay) / 100.0f * (height - 20));
                    graphic.DrawString(ay.ToString(), graphicFont, graphicBrush, 3, y + 11);
                    graphic.DrawLine(graphicPen, 25, y + 5, 35, y + 5);
                }
                graphic.DrawLine(graphicPen, 35, 19, 35, height);

                graphicPen.Color = Color.Blue;
                graphicBrush.Color = Color.Blue;

                for (int i = 0; i < patterns.Count; i++)
                {
                    left = i * colWidth + 42;
                    top = height - (int)(patterns.ElementAt(i).cost * (height - 20));

                    graphic.DrawRectangle(graphicPen,
                            left,
                            top,
                            colWidth - 2,
                            height - top);
                    graphic.DrawString(patterns.ElementAt(i).chr + " ", graphicFont, graphicBrush, left + 2,
                            top - 8);
                }
                return histogram;
            }
        }


        /** Creates a new instance of CharacterRecognizer */
        public CharacterRecognizer()
        {

        }

        public abstract RecognizedChar recognize(Char chr);
    }
}
