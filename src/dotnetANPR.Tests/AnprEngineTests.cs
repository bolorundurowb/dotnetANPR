using dotnetANPR;

namespace dotnetANPR.Tests;

[TestClass]
public class AnprEngineTests
{
    [TestMethod]
    public void Engine_ConstructsWithDefaults()
    {
        var engine = new AnprEngine(new AnprOptions());
        Assert.IsNotNull(engine);
    }

    [TestMethod]
    public void Recognize_InvalidPath_Throws()
    {
        var engine = new AnprEngine(new AnprOptions());
        Assert.ThrowsExactly<ArgumentException>(() => engine.Recognize("nonexistent.jpg"));
    }
}
