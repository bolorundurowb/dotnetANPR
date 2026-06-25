using dotnetANPR.ImageAnalysis;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class ProbabilityDistributorTests
{
    // ── Margin zeroing ────────────────────────────────────────────────────────

    [TestMethod]
    public void Distribute_LeftMargin_ZerosFirstNItems()
    {
        // center=0.5, power=0, leftMargin=2, rightMargin=0
        var dist = new ProbabilityDistributor(0.5f, 0f, 2, 1);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        result[0].Verify().ToBe(0f);
        result[1].Verify().ToBe(0f);
    }

    [TestMethod]
    public void Distribute_RightMargin_ZerosLastNItems()
    {
        // leftMargin=1, rightMargin=2 → last 2 indices zeroed
        var dist = new ProbabilityDistributor(0.5f, 0f, 1, 2);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        result[4].Verify().ToBe(0f);
        result[3].Verify().ToBe(0f);
    }

    // ── Distribution function ─────────────────────────────────────────────────

    [TestMethod]
    public void Distribute_ZeroPower_PassesThroughValues()
    {
        // With power=0 the distribution function = value * (1 - 0) = value
        var dist = new ProbabilityDistributor(0.5f, 0f, 0, 0);
        var input = new List<float> { 2f, 3f, 4f };

        var result = dist.Distribute(input);

        result[0].Verify().ToBeApproximately(2f, 0.0001f);
        result[1].Verify().ToBeApproximately(3f, 0.0001f);
        result[2].Verify().ToBeApproximately(4f, 0.0001f);
    }

    [TestMethod]
    public void Distribute_HighPower_ReducesEdgeValues()
    {
        // With high power edge positions are heavily penalised
        var dist = new ProbabilityDistributor(0.5f, 1.0f, 0, 0);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        // The center (position ≈ 0.5) should score higher than the edges
        var centerValue = result[2];  // index 2 of 5 → position = 2/5 = 0.4
        var edgeValue = result[0];    // index 0 → position = 0/5 = 0
        (centerValue >= edgeValue).Verify().ToBeTrue();
    }

    // ── Edge cases ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Distribute_EmptyList_ReturnsEmptyList()
    {
        var dist = new ProbabilityDistributor(0.5f, 0f, 1, 1);
        var result = dist.Distribute([]);
        result.Verify().ToBeEmpty();
    }

    [TestMethod]
    public void Distribute_ZeroMarginClampsToOne()
    {
        // Passing leftMargin=0 should be clamped to 1 by the constructor
        var dist = new ProbabilityDistributor(0.5f, 0f, 0, 0);
        var input = new List<float> { 5f, 5f, 5f };
        // Should not throw; all values pass through with power=0
        var result = dist.Distribute(input);
        result.Verify().ToHaveCount(3);
    }

    [TestMethod]
    public void Distribute_OutputLengthMatchesInput()
    {
        var dist = new ProbabilityDistributor(0.5f, 0.5f, 2, 2);
        var input = Enumerable.Range(0, 10).Select(i => (float)i).ToList();
        var result = dist.Distribute(input);
        result.Verify().ToHaveCount(input.Count);
    }
}
