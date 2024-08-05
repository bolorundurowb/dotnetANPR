using System.Collections.Generic;

namespace DotNetANPR.Recognizer;

public class RecognizedPattern(char chr, float cost)
{
    public char Char { get; private set; } = chr;

    public float Cost { get; private set; } = cost;
}

public class RecognizedPatternComparer(bool sortDesc) : IComparer<RecognizedPattern>
{
    public int Compare(RecognizedPattern x, RecognizedPattern y)
    {
        return sortDesc ? -1 * Compute() : Compute();

        int Compute()
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
}
