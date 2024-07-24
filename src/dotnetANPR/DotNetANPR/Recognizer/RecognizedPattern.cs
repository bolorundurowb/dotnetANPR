using System.Collections.Generic;

namespace DotNetANPR.Recognizer;

public class RecognizedPattern(char chr, float cost)
{
    public char Char { get; private set; } = chr;

    public float Cost { get; private set; } = cost;
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