using System.Collections.Generic;

namespace DotNetANPR.Recognizer;

/// <summary>
/// Represents a single character recognition result with an associated cost (confidence score).
/// Lower cost values typically indicate a closer match (for KNN), while for neural network
/// classification, higher values indicate greater confidence.
/// </summary>
/// <param name="chr">The recognized character.</param>
/// <param name="cost">The cost or confidence value associated with this recognition.</param>
public class RecognizedPattern(char chr, float cost)
{
    /// <summary>
    /// Gets the recognized character.
    /// </summary>
    public char Char { get; } = chr;

    /// <summary>
    /// Gets the cost (distance or confidence) of this recognition result.
    /// </summary>
    public float Cost { get; } = cost;
}

/// <summary>
/// Comparer for <see cref="RecognizedPattern"/> instances that sorts by <see cref="RecognizedPattern.Cost"/>.
/// </summary>
/// <param name="sortDesc">
/// When <c>true</c>, sorts in descending order (highest cost first);
/// when <c>false</c>, sorts in ascending order (lowest cost first).
/// </param>
public class RecognizedPatternComparer(bool sortDesc) : IComparer<RecognizedPattern>
{
    /// <summary>
    /// Compares two <see cref="RecognizedPattern"/> instances by their cost values.
    /// </summary>
    /// <param name="x">The first pattern.</param>
    /// <param name="y">The second pattern.</param>
    /// <returns>A negative, zero, or positive value indicating the relative order.</returns>
    public int Compare(RecognizedPattern? x, RecognizedPattern? y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (x is null)
            return -1;

        if (y is null)
            return 1;

        int result;
        if (x.Cost < y.Cost)
            result = -1;
        else if (x.Cost > y.Cost)
            result = 1;
        else
            result = 0;

        return sortDesc ? -result : result;
    }
}
