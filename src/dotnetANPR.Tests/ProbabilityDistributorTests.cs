using dotnetANPR.ImageAnalysis;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class ProbabilityDistributorTests
{
    [TestMethod]
    public void Distribute_LeftMargin_ZerosFirstNItems()
    {
        var dist = new ProbabilityDistributor(0.5f, 0f, 2, 1);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        result[0].Verify().ToBe(0f);
        result[1].Verify().ToBe(0f);
    }

    [TestMethod]
    public void Distribute_RightMargin_ZerosLastNItems()
    {
        var dist = new ProbabilityDistributor(0.5f, 0f, 1, 2);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        result[4].Verify().ToBe(0f);
        result[3].Verify().ToBe(0f);
    }

    [TestMethod]
    public void Distribute_ZeroPower_PassesThroughValues()
    {
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
        var dist = new ProbabilityDistributor(0.5f, 1.0f, 0, 0);
        var input = new List<float> { 1f, 1f, 1f, 1f, 1f };

        var result = dist.Distribute(input);

        var centerValue = result[2];
        var edgeValue = result[0];
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
    public void Distribute_ZeroMargin_NoZeroing()
    {
        var dist = new ProbabilityDistributor(0.5f, 0f, 0, 0);
        var input = new List<float> { 5f, 5f, 5f };
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
