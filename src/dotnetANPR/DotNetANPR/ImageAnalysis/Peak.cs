using System.Collections.Generic;

namespace DotNetANPR.ImageAnalysis;

public class Peak
{
    public int Left { get; set; }

    public int Center { get; set; }

    public int Right { get; set; }

    public int Diff => Right - Left;

    public Peak(int left, int right)
    {
        Left = left;
        Center = (left + right) / 2;
        Right = right;
    }

    public Peak(int left, int center, int right)
    {
        Left = left;
        Center = center;
        Right = right;
    }
}

public class PeakComparator(List<float> yValues) : IComparer<Peak>
{
    public int Compare(Peak x, Peak y) => yValues[y.Center].CompareTo(yValues[x.Center]);
}
