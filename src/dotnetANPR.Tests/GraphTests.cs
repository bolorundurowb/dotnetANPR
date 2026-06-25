using dotnetANPR.ImageAnalysis;
using OmniAssert;

namespace dotnetANPR.Tests;

/// <summary>
/// Exposes protected members of Graph for unit testing.
/// </summary>
internal sealed class TestGraph : Graph
{
    public void SetValues(IEnumerable<float> values)
    {
        YValues.Clear();
        foreach (var v in values)
            YValues.Add(v);
        DeActualizeFlags();
    }

    public void SetPeaks(List<Peak> peaks) => Peaks = peaks;
}

[TestClass]
public class GraphTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static TestGraph Build(params float[] values)
    {
        var g = new TestGraph();
        g.SetValues(values);
        return g;
    }

    // ── AverageValue ─────────────────────────────────────────────────────────

    [TestMethod]
    public void AverageValue_ReturnsCorrectMean()
    {
        var g = Build(1f, 2f, 3f, 4f);
        // sum = 10, count = 4  →  mean = 10/4 = 2.5
        g.AverageValue().Verify().ToBeApproximately(2.5f, 0.0001f);
    }

    [TestMethod]
    public void AverageValue_Range_SumsOnlySlice()
    {
        var g = Build(1f, 2f, 3f, 4f);
        // AverageValue(a,b) sums [a..b) but divides by total count (4)
        // slice [1,3) = 2+3 = 5  →  5/4 = 1.25
        g.AverageValue(1, 3).Verify().ToBeApproximately(1.25f, 0.0001f);
    }

    [TestMethod]
    public void AverageValue_IsCached_AfterFirstCall()
    {
        var g = Build(10f, 20f);
        var first = g.AverageValue();
        var second = g.AverageValue();
        second.Verify().ToBe(first);
    }

    [TestMethod]
    public void DeActualizeFlags_ForcesRecompute()
    {
        var g = Build(10f, 20f);
        _ = g.AverageValue(); // cache it
        g.SetValues(new[] { 5f, 5f }); // SetValues calls DeActualizeFlags
        g.AverageValue().Verify().ToBeApproximately(5f, 0.0001f);
    }

    // ── MaxValue ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void MaxValue_ReturnsLargest()
    {
        var g = Build(3f, 7f, 1f, 5f);
        g.MaxValue().Verify().ToBeApproximately(7f, 0.0001f);
    }

    [TestMethod]
    public void MaxValue_Range_IntOverload()
    {
        var g = Build(9f, 2f, 8f, 1f);
        g.MaxValue(1, 2).Verify().ToBeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void MaxValue_Range_FloatOverload()
    {
        var g = Build(1f, 2f, 3f, 4f);
        // 0.5..1.0 maps to indices [2,4)
        g.MaxValue(0.5f, 1.0f).Verify().ToBeApproximately(4f, 0.0001f);
    }

    [TestMethod]
    public void MaxValueIndex_ReturnsIndexOfLargest()
    {
        var g = Build(1f, 5f, 3f);
        g.MaxValueIndex(0, 3).Verify().ToBe(1);
    }

    // ── MinValue ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void MinValue_ReturnsSmallest()
    {
        var g = Build(4f, 1f, 9f, 2f);
        g.MinValue().Verify().ToBeApproximately(1f, 0.0001f);
    }

    [TestMethod]
    public void MinValue_Range_IntOverload()
    {
        var g = Build(9f, 2f, 8f, 1f);
        g.MinValue(1, 2).Verify().ToBeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void MinValue_Range_FloatOverload()
    {
        var g = Build(4f, 3f, 2f, 1f);
        // 0.5..1.0 → indices [2,4)
        g.MinValue(0.5f, 1.0f).Verify().ToBeApproximately(1f, 0.0001f);
    }

    // ── Negate ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Negate_SubtractsEachValueFromMax()
    {
        var g = Build(1f, 3f, 2f);
        // max = 3 → negated: [2, 0, 1]
        g.Negate();
        g.MinValue().Verify().ToBeApproximately(0f, 0.0001f);
        g.MaxValue().Verify().ToBeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void Negate_DoubleNegate_RestoresOriginal()
    {
        var g = Build(1f, 3f, 2f);
        var original = g.AverageValue();
        g.Negate();
        g.Negate();
        g.AverageValue().Verify().ToBeApproximately(original, 0.0001f);
    }

    // ── RankFilter ───────────────────────────────────────────────────────────

    [TestMethod]
    public void RankFilter_SmoothesCentralValues()
    {
        var g = Build(0f, 0f, 10f, 0f, 0f);
        g.RankFilter(3);
        // center element [2] should be smoothed: (0+10+0)/3 ≈ 3.33
        (g.MaxValue() < 10f).Verify().ToBeTrue();
    }

    // ── AddPeak ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void AddPeak_IncreasesCount()
    {
        var g = new TestGraph();
        g.AddPeak(1f);
        g.AddPeak(2f);
        g.MaxValue().Verify().ToBeApproximately(2f, 0.0001f);
    }

    // ── IsOutsideAllPeaks ────────────────────────────────────────────────────

    [TestMethod]
    public void IsOutsideAllPeaks_ReturnsTrueWhenOutside()
    {
        var peaks = new List<Peak> { new Peak(5, 10) };
        var g = new TestGraph();
        g.IsOutsideAllPeaks(peaks, 3).Verify().ToBeTrue();
        g.IsOutsideAllPeaks(peaks, 11).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IsOutsideAllPeaks_ReturnsFalseWhenInside()
    {
        var peaks = new List<Peak> { new Peak(5, 10) };
        var g = new TestGraph();
        g.IsOutsideAllPeaks(peaks, 7).Verify().ToBeFalse();
    }

    // ── IndexOfLeftPeakRel / IndexOfRightPeakRel ─────────────────────────────

    [TestMethod]
    public void IndexOfLeftPeakRel_StopsAtFoot()
    {
        // Values: 0 0 0 0 10 0 0 — peak at index 4
        var g = Build(0f, 0f, 0f, 0f, 10f, 0f, 0f);
        var left = g.IndexOfLeftPeakRel(4, 0.5);
        (left < 4).Verify().ToBeTrue();
    }

    [TestMethod]
    public void IndexOfRightPeakRel_StopsAtFoot()
    {
        var g = Build(0f, 0f, 0f, 0f, 10f, 0f, 0f);
        var right = g.IndexOfRightPeakRel(4, 0.5);
        (right > 4).Verify().ToBeTrue();
    }

    // ── AveragePeakDiff / MaximumPeakDiff ────────────────────────────────────

    [TestMethod]
    public void AveragePeakDiff_ReturnsCorrectMean()
    {
        var peaks = new List<Peak>
        {
            new Peak(0, 4),   // diff = 4
            new Peak(10, 16), // diff = 6
        };
        var g = new TestGraph();
        g.AveragePeakDiff(peaks).Verify().ToBeApproximately(5f, 0.0001f);
    }

    [TestMethod]
    public void MaximumPeakDiff_ReturnsLargestInRange()
    {
        var peaks = new List<Peak>
        {
            new Peak(0, 4),   // diff = 4
            new Peak(10, 16), // diff = 6
            new Peak(20, 21), // diff = 1
        };
        var g = new TestGraph();
        g.MaximumPeakDiff(peaks, 0, 2).Verify().ToBeApproximately(6f, 0.0001f);
    }
}
