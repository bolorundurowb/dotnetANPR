using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class SmokeTests
{
    [TestMethod]
    public void OmniAssertIsWiredAndPasses()
    {
        const int answer = 42;

        answer.Verify().ToBe(42);
    }
}
