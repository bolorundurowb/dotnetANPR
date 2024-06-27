using System;

namespace DotNetANPR.Recognizer
{
    public class RecognizedPattern : IComparable<RecognizedPattern>
    {
        public char Char { get; private set; }

        public float Cost { get; private set; }

        public RecognizedPattern(char chr, float cost)
        {
            Char = chr;
            Cost = cost;
        }

        public int CompareTo(RecognizedPattern other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (ReferenceEquals(null, other))
                return 1;

            if (Cost < other.Cost)
                return -1;

            if (Cost > other.Cost)
                return 1;

            return 0;
        }
    }
}
