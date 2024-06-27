using System;
using System.Collections.Generic;

namespace DotNetANPR.Recognizer;

public class RecognizedPattern(char chr, float cost) : IComparable<RecognizedPattern>
{
    public char Char { get; private set; } = chr;

    public float Cost { get; private set; } = cost;

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

public class RecognizedPatternComparer : IComparer<RecognizedPattern>
{
    public int Compare(RecognizedPattern x, RecognizedPattern y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (ReferenceEquals(null, y))
            return 1;

        if (x.Cost < y.Cost)
            return -1;

        if (x.Cost > y.Cost)
            return 1;

        return 0;
    }
}