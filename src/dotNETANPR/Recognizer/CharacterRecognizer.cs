using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using dotNETANPR.ImageAnalysis;

namespace dotNETANPR.Recognizer
{
    public class CharacterRecognizer : ICharacterRecognizer
    {
        public static char[] Alphabet =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C',
            'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static float[,] features =
        {
            {0, 1, 0, 1},
            {1, 0, 1, 0},
            {0, 0, 1, 1},
            {1, 1, 0, 0},
            {0, 0, 0, 1},
            {1, 0, 0, 0},
            {1, 1, 1, 0},
            {0, 1, 1, 1},
            {0, 0, 1, 0},
            {0, 1, 0, 0},
            {1, 0, 1, 1},
            {1, 1, 0, 1}
        };

        public class RecognizedChar
        {
            public class RecognizedPattern
            {
                public char Character { get; set; }
                public float Cost { get; set; }

                public RecognizedPattern(char character, float cost)
                {
                    Character = character;
                    Cost = cost;
                }
            }

            public class PatternComparer : IComparer<RecognizedPattern>
            {
                public int Direction { get; set; }

                public PatternComparer(int direction)
                {
                    Direction = direction;
                }

                public int Compare(RecognizedPattern pattern1, RecognizedPattern pattern2)
                {
                    var cost1 = pattern1.Cost;
                    var cost2 = pattern2.Cost;
                    var ret = 0;
                    if (cost1 < cost2)
                    {
                        ret = -1;
                    }
                    if (cost1 > cost2)
                    {
                        ret = 1;
                    }
                    if (Direction == 1)
                    {
                        ret *= -1;
                    }
                    return ret;
                }
            }

            private List<RecognizedPattern> _patterns;
            public List<RecognizedPattern> Patterns
            {
                get
                {
                    if (IsSorted)
                    {
                        return _patterns;
                    }
                    return null;
                }
                set { _patterns = value; }
            }
            public bool IsSorted { get; set; }

            public RecognizedChar()
            {
                Patterns = new List<RecognizedPattern>();
                IsSorted = false;
            }

            public void AddPattern(RecognizedPattern pattern)
            {
                Patterns.Add(pattern);
            }

            public void Sort(int direction)
            {
                if (IsSorted)
                {
                    return;
                }
                IsSorted = true;
                _patterns.Sort(new PatternComparer(direction));
            }

            public List<RecognizedPattern> GetPatterns()
            {
                if (IsSorted)
                {
                    return _patterns;
                }
                return null;
            }

            public RecognizedPattern GetPattern(int index)
            {
                return _patterns[index];
            }

            public Bitmap Render()
            {
                var width = 500;
                var height = 200;
                var histogram = new Bitmap(width + 20, height + 20, PixelFormat.Format8bppIndexed);
                var graphic = Graphics.FromImage(histogram);

                var graphicPen = new Pen(Color.LightGray);
                var graphicBrush = new SolidBrush(Color.LightGray);
                var backRect = new Rectangle(0, 0, width + 20, height + 20);
                graphic.FillRectangle(graphicBrush, backRect);
                graphic.DrawRectangle(graphicPen, backRect);

                graphicPen.Color = Color.Black;
                graphicBrush.Color = Color.Black;
                var graphicFont = new Font("Consolas", 20);

                var colWidth = width / _patterns.Count;
                int left, top;


                for (var i = 0; i <= 100; i += 10)
                {
                    var y = 15 + (int) ((100 - i) / 100.0f * (height - 20));
                    graphic.DrawString(i.ToString(), graphicFont, graphicBrush, 3, y + 11);
                    graphic.DrawLine(graphicPen, 25, y + 5, 35, y + 5);
                }
                graphic.DrawLine(graphicPen, 35, 19, 35, height);

                graphicPen.Color = Color.Blue;
                graphicBrush.Color = Color.Blue;

                for (var i = 0; i < _patterns.Count; i++)
                {
                    left = i * colWidth + 42;
                    top = height - (int) (_patterns[i].Cost * (height - 20));
                    graphic.DrawRectangle(graphicPen, left, top, colWidth - 2, height - top);
                    graphic.DrawString(_patterns[i].Character + " ", graphicFont, graphicBrush, left + 2, top - 8);
                }
                return histogram;
            }
        }

        public virtual RecognizedChar Recognize(Character character)
        {
            throw new System.NotImplementedException();
        }
    }
}
