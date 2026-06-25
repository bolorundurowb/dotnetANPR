using dotnetANPR;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class AnprEngineTests
{
    [TestMethod]
    public void Engine_ConstructsWithDefaults()
    {
        var engine = new AnprEngine(new AnprOptions());
        engine.Verify().NotToBeNull();
    }

    [TestMethod]
    public void Recognize_InvalidPath_Throws()
    {
        var engine = new AnprEngine(new AnprOptions());
        ((Action)(() => engine.Recognize("nonexistent.jpg"))).Throws<ArgumentException>();
    }
}
