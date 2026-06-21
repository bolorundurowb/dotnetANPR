using System.Collections.Generic;

namespace dotnetANPR.Recognizer;

/// <summary>
/// Represents a single character match with an associated cost (lower is better).
/// </summary>
/// <param name="chr">The matched character.</param>
/// <param name="cost">The classification cost; lower values indicate a better match.</param>
public class RecognizedPattern(char chr, float cost)
{
    /// <summary>
    /// Gets the matched character.
    /// </summary>
    public char Char { get; private set; } = chr;

    /// <summary>
    /// Gets the classification cost. Lower values indicate a better match.
    /// </summary>
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
