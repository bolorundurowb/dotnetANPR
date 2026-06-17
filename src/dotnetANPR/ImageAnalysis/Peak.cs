using System.Collections.Generic;

namespace DotNetANPR.ImageAnalysis;

/// <summary>
/// Represents a peak detected in a graph, defined by left boundary, center, and right boundary indices.
/// </summary>
/// <param name="left">The left boundary index of the peak.</param>
/// <param name="center">The center index of the peak.</param>
/// <param name="right">The right boundary index of the peak.</param>
public class Peak(int left, int center, int right)
{
    /// <summary>
    /// Gets or sets the left boundary index of the peak.
    /// </summary>
    public int Left { get; set; } = left;

    /// <summary>
    /// Gets or sets the center index of the peak.
    /// </summary>
    public int Center { get; set; } = center;

    /// <summary>
    /// Gets or sets the right boundary index of the peak.
    /// </summary>
    public int Right { get; set; } = right;

    /// <summary>
    /// Gets the width of the peak (difference between right and left boundaries).
    /// </summary>
    public int Diff => Right - Left;

    /// <summary>
    /// Initializes a new instance of the <see cref="Peak"/> class with left and right boundaries,
    /// automatically computing the center as the midpoint.
    /// </summary>
    /// <param name="left">The left boundary index.</param>
    /// <param name="right">The right boundary index.</param>
    public Peak(int left, int right) : this(left, (left + right) / 2, right) { }
}

/// <summary>
/// Compares two <see cref="Peak"/> instances by their center Y-value in descending order.
/// Peaks with higher Y-values at their center are sorted first.
/// </summary>
/// <param name="yValues">The list of Y-values from the parent graph.</param>
public class PeakComparator(List<float> yValues) : IComparer<Peak>
{
    /// <summary>
    /// Compares two peaks by their center Y-value in descending order.
    /// </summary>
    /// <param name="x">The first peak to compare.</param>
    /// <param name="y">The second peak to compare.</param>
    /// <returns>A negative value if x has a higher center value, positive if lower, zero if equal.</returns>
    public int Compare(Peak? x, Peak? y)
    {
        if (x is null && y is null) return 0;
        if (x is null) return 1;
        if (y is null) return -1;
        return yValues[y.Center].CompareTo(yValues[x.Center]);
    }
}

/// <summary>
/// Compares two <see cref="Peak"/> instances by their center position in ascending order.
/// Peaks further to the left (lower center index) are sorted first.
/// </summary>
public class SpaceComparator : IComparer<Peak>
{
    /// <summary>
    /// Compares two peaks by their center position in ascending order.
    /// </summary>
    /// <param name="peak1">The first peak to compare.</param>
    /// <param name="peak2">The second peak to compare.</param>
    /// <returns>A negative value if peak1 is to the left, positive if to the right, zero if equal.</returns>
    public int Compare(Peak? peak1, Peak? peak2)
    {
        if (peak1 is null && peak2 is null) return 0;
        if (peak1 is null) return 1;
        if (peak2 is null) return -1;

        double comparison = peak2.Center - peak1.Center;
        return comparison switch
        {
            < 0 => 1,
            > 0 => -1,
            _ => 0
        };
    }
}
