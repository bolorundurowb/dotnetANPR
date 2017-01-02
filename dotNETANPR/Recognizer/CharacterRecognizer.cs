using System;

namespace dotNETANPR.Recognizer
{
    public class CharacterRecognizer
    {
        public static char[] alphabet =
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

            public class PatternComparer : IComparable
            {
                public int Direction { get; set; }

                public PatternComparer(int direction)
                {
                    Direction = direction;
                }

                public int CompareTo(object obj)
                {
//                    float cost1 = ((RecognizedPattern) this).Cost;
                    float cost2 = ((RecognizedPattern) obj).Cost;
                    return -1;
                }
            }
        }
    }
}
