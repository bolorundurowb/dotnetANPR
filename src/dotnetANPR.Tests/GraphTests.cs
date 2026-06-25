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
    private static TestGraph Build(params float[] values)
    {
        var g = new TestGraph();
        g.SetValues(values);
        return g;
    }

    [TestMethod]
    public void AverageValue_ReturnsCorrectMean()
    {
        var g = Build(1f, 2f, 3f, 4f);
        g.AverageValue().Must().BeApproximately(2.5f, 0.0001f);
    }

    [TestMethod]
    public void AverageValue_Range_SumsOnlySlice()
    {
        var g = Build(1f, 2f, 3f, 4f);
        g.AverageValue(1, 3).Must().BeApproximately(1.25f, 0.0001f);
    }

    [TestMethod]
    public void AverageValue_IsCached_AfterFirstCall()
    {
        var g = Build(10f, 20f);
        var first = g.AverageValue();
        var second = g.AverageValue();
        second.Must().Be(first);
    }

    [TestMethod]
    public void DeActualizeFlags_ForcesRecompute()
    {
        var g = Build(10f, 20f);
        _ = g.AverageValue();
        g.SetValues(new[] { 5f, 5f });
        g.AverageValue().Must().BeApproximately(5f, 0.0001f);
    }

    // ── MaxValue ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void MaxValue_ReturnsLargest()
    {
        var g = Build(3f, 7f, 1f, 5f);
        g.MaxValue().Must().BeApproximately(7f, 0.0001f);
    }

    [TestMethod]
    public void MaxValue_Range_IntOverload()
    {
        var g = Build(9f, 2f, 8f, 1f);
        g.MaxValue(1, 2).Must().BeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void MaxValue_Range_FloatOverload()
    {
        var g = Build(1f, 2f, 3f, 4f);
        g.MaxValue(0.5f, 1.0f).Must().BeApproximately(4f, 0.0001f);
    }

    [TestMethod]
    public void MaxValueIndex_ReturnsIndexOfLargest()
    {
        var g = Build(1f, 5f, 3f);
        g.MaxValueIndex(0, 3).Must().Be(1);
    }

    [TestMethod]
    public void MinValue_ReturnsSmallest()
    {
        var g = Build(4f, 1f, 9f, 2f);
        g.MinValue().Must().BeApproximately(1f, 0.0001f);
    }

    [TestMethod]
    public void MinValue_Range_IntOverload()
    {
        var g = Build(9f, 2f, 8f, 1f);
        g.MinValue(1, 2).Must().BeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void MinValue_Range_FloatOverload()
    {
        var g = Build(4f, 3f, 2f, 1f);
        g.MinValue(0.5f, 1.0f).Must().BeApproximately(1f, 0.0001f);
    }

    [TestMethod]
    public void Negate_SubtractsEachValueFromMax()
    {
        var g = Build(1f, 3f, 2f);
        g.Negate();
        g.MinValue().Must().BeApproximately(0f, 0.0001f);
        g.MaxValue().Must().BeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void Negate_DoubleNegate_RestoresOriginal()
    {
        var g = Build(0f, 2f, 1f);
        var original = g.AverageValue();
        g.Negate();
        g.Negate();
        g.AverageValue().Must().BeApproximately(original, 0.0001f);
    }

    [TestMethod]
    public void RankFilter_SmoothesCentralValues()
    {
        var g = Build(0f, 0f, 10f, 0f, 0f);
        g.RankFilter(3);
        (g.MaxValue() < 10f).Must().BeTrue();
    }

    [TestMethod]
    public void AddPeak_IncreasesCount()
    {
        var g = new TestGraph();
        g.AddPeak(1f);
        g.AddPeak(2f);
        g.MaxValue().Must().BeApproximately(2f, 0.0001f);
    }

    [TestMethod]
    public void IsOutsideAllPeaks_ReturnsTrueWhenOutside()
    {
        var peaks = new List<Peak> { new Peak(5, 10) };
        var g = new TestGraph();
        g.IsOutsideAllPeaks(peaks, 3).Must().BeTrue();
        g.IsOutsideAllPeaks(peaks, 11).Must().BeTrue();
    }

    [TestMethod]
    public void IsOutsideAllPeaks_ReturnsFalseWhenInside()
    {
        var peaks = new List<Peak> { new Peak(5, 10) };
        var g = new TestGraph();
        g.IsOutsideAllPeaks(peaks, 7).Must().BeFalse();
    }

    [TestMethod]
    public void IndexOfLeftPeakRel_StopsAtFoot()
    {
        var g = Build(0f, 0f, 0f, 0f, 10f, 0f, 0f);
        var left = g.IndexOfLeftPeakRel(4, 0.5);
        (left < 4).Must().BeTrue();
    }

    [TestMethod]
    public void IndexOfRightPeakRel_StopsAtFoot()
    {
        var g = Build(0f, 0f, 0f, 0f, 10f, 0f, 0f);
        var right = g.IndexOfRightPeakRel(4, 0.5);
        (right > 4).Must().BeTrue();
    }

    [TestMethod]
    public void AveragePeakDiff_ReturnsCorrectMean()
    {
        var peaks = new List<Peak>
        {
            new Peak(0, 4),
            new Peak(10, 16),
        };
        var g = new TestGraph();
        g.AveragePeakDiff(peaks).Must().BeApproximately(5f, 0.0001f);
    }

    [TestMethod]
    public void MaximumPeakDiff_ReturnsLargestInRange()
    {
        var peaks = new List<Peak>
        {
            new Peak(0, 4),
            new Peak(10, 16),
            new Peak(20, 21),
        };
        var g = new TestGraph();
        g.MaximumPeakDiff(peaks, 0, 2).Must().BeApproximately(6f, 0.0001f);
    }
}
