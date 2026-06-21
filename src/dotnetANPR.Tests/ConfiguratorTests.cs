using dotnetANPR.Configuration;

namespace dotnetANPR.Tests;

[TestClass]
public class ConfiguratorTests
{
    [TestMethod]
    public void DefaultSettings_IncludeEdgeCharRatio()
    {
        var locator = new ResourceLocator();
        var settings = new Configurator().CreateSettings(locator);

        Assert.AreEqual(0.12, settings.IntelligenceMinEdgeCharWidthHeightRatio);
        Assert.AreEqual(8, settings.CharNormalizedDimensionsX);
        Assert.AreEqual(13, settings.CharNormalizedDimensionsY);
    }

    [TestMethod]
    public void ResourceLocator_ResolvesRelativePathFromBaseDirectory()
    {
        var locator = new ResourceLocator();
        var resolved = locator.Resolve("Resources/syntax.xml");
        Assert.IsTrue(File.Exists(resolved), $"Expected syntax file at {resolved}");
    }
}
