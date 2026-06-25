using dotnetANPR.ImageAnalysis;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class PeakTests
{
    [TestMethod]
    public void Peak_ThreeArgConstructor_StoresAllValues()
    {
        var peak = new Peak(2, 5, 9);
        peak.Left.Verify().ToBe(2);
        peak.Center.Verify().ToBe(5);
        peak.Right.Verify().ToBe(9);
    }

    [TestMethod]
    public void Peak_TwoArgConstructor_ComputesCenterAsAverage()
    {
        var peak = new Peak(4, 10);
        peak.Left.Verify().ToBe(4);
        peak.Right.Verify().ToBe(10);
        peak.Center.Verify().ToBe((4 + 10) / 2);
    }

    [TestMethod]
    public void Peak_Diff_IsRightMinusLeft()
    {
        var peak = new Peak(3, 11);
        peak.Diff.Verify().ToBe(8);
    }

    [TestMethod]
    public void Peak_Diff_IsZeroWhenEqualBounds()
    {
        var peak = new Peak(5, 5);
        peak.Diff.Verify().ToBe(0);
    }

    [TestMethod]
    public void Peak_MutablySetLeft_UpdatesDiff()
    {
        var peak = new Peak(2, 10);
        peak.Left = 5;
        peak.Diff.Verify().ToBe(5);
    }

    [TestMethod]
    public void PeakComparator_SortsDescendingByYValue()
    {
        var yValues = new List<float> { 0f, 3f, 1f, 5f, 2f };
        var comparer = new PeakComparator(yValues);

        var peaks = new List<Peak>
        {
            new Peak(0, 1, 2),
            new Peak(2, 3, 4),
        };

        peaks.Sort(comparer);

        peaks[0].Center.Verify().ToBe(3);
        peaks[1].Center.Verify().ToBe(1);
    }

    [TestMethod]
    public void PeakComparator_EqualYValues_ReturnZero()
    {
        var yValues = new List<float> { 1f, 2f, 2f };
        var comparer = new PeakComparator(yValues);
        var a = new Peak(0, 1, 2);
        var b = new Peak(0, 2, 2);
        comparer.Compare(a, b).Verify().ToBe(0);
    }

    [TestMethod]
    public void SpaceComparator_OrdersByCenter_Ascending()
    {
        var comparer = new SpaceComparator();

        var peaks = new List<Peak>
        {
            new Peak(10, 15, 20),
            new Peak(0, 3, 6),
            new Peak(5, 8, 11),
        };

        peaks.Sort(comparer);

        peaks[0].Center.Verify().ToBe(3);
        peaks[1].Center.Verify().ToBe(8);
        peaks[2].Center.Verify().ToBe(15);
    }

    [TestMethod]
    public void SpaceComparator_EqualCenters_ReturnsZero()
    {
        var comparer = new SpaceComparator();
        var a = new Peak(0, 5, 10);
        var b = new Peak(1, 5, 9);
        comparer.Compare(a, b).Verify().ToBe(0);
    }
}
