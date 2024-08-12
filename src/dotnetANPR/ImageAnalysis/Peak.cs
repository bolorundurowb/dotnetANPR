using System.Collections.Generic;

namespace DotNetANPR.ImageAnalysis;

public class Peak(int left, int center, int right)
{
    public int Left { get; set; } = left;

    public int Center { get; set; } = center;

    public int Right { get; set; } = right;

    public int Diff => Right - Left;

    public Peak(int left, int right) : this(left, (left + right) / 2, right) { }
}

public class PeakComparator(List<float> yValues) : IComparer<Peak>
{
    public int Compare(Peak x, Peak y) => yValues[y.Center].CompareTo(yValues[x.Center]);
}

public class SpaceComparator : IComparer<Peak>
{
    public int Compare(Peak peak1, Peak peak2)
    {
        double comparison = peak2.Center - peak1.Center;
        return comparison switch
        {
            < 0 => 1,
            > 0 => -1,
            _ => 0
        };
    }
}
