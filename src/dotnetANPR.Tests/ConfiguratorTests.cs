using dotnetANPR.Configuration;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class ConfiguratorTests
{
    [TestMethod]
    public void DefaultSettings_IncludeEdgeCharRatio()
    {
        var locator = new ResourceLocator();
        var settings = new Configurator().CreateSettings(locator);

        settings.IntelligenceMinEdgeCharWidthHeightRatio.Must().Be(0.12);
        settings.CharNormalizedDimensionsX.Must().Be(8);
        settings.CharNormalizedDimensionsY.Must().Be(13);
    }

    [TestMethod]
    public void ResourceLocator_ResolvesRelativePathFromBaseDirectory()
    {
        var locator = new ResourceLocator();
        var resolved = locator.Resolve("Resources/syntax.xml");
        File.Exists(resolved).Must().BeTrue();
    }

    [TestMethod]
    public void Set_ThenGet_RoundtripsInteger()
    {
        var config = new Configurator();
        config.Set("char_normalizeddimensions_x", 16);
        config.Get<int>("char_normalizeddimensions_x").Must().Be(16);
    }

    [TestMethod]
    public void Set_ThenGet_RoundtripsDouble()
    {
        var config = new Configurator();
        config.Set("bandgraph_peakfootconstant", 0.99);
        config.Get<double>("bandgraph_peakfootconstant").Must().BeApproximately(0.99, 0.0001);
    }

    [TestMethod]
    public void Set_OverridesDefault()
    {
        var config = new Configurator();
        config.Set("intelligence_minimumChars", 3);
        var settings = config.CreateSettings(new ResourceLocator());
        settings.IntelligenceMinimumChars.Must().Be(3);
    }

    [TestMethod]
    public void DefaultSettings_CharCountRange_IsReasonable()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinimumChars > 0).Must().BeTrue();
        (settings.IntelligenceMaximumChars > settings.IntelligenceMinimumChars).Must().BeTrue();
    }

    [TestMethod]
    public void DefaultSettings_PlateRatioRange_IsOrdered()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinPlateWidthHeightRatio <
         settings.IntelligenceMaxPlateWidthHeightRatio).Must().BeTrue();
    }

    [TestMethod]
    public void DefaultSettings_CharRatioRange_IsOrdered()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinCharWidthHeightRatio <
         settings.IntelligenceMaxCharWidthHeightRatio).Must().BeTrue();
    }

    [TestMethod]
    public void DefaultSettings_NeuralNetworkPath_FileExists()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        File.Exists(settings.CharNeuralNetworkPath).Must().BeTrue();
    }

    [TestMethod]
    public void DefaultSettings_SyntaxDescriptionFile_FileExists()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        File.Exists(settings.IntelligenceSyntaxDescriptionFile).Must().BeTrue();
    }

    [TestMethod]
    public void GetPath_NormalisesForwardSlashes()
    {
        var config = new Configurator();
        var path = config.GetPath("char_neuralNetworkPath");
        (path.Contains('/') && Path.DirectorySeparatorChar == '\\').Must().BeFalse();
    }

    [TestMethod]
    public void ResourceLocator_ResolvesNeuralNetworkFile()
    {
        var locator = new ResourceLocator();
        var resolved = locator.Resolve("Resources/neuralnetworks/network_avgres_813_map.xml");
        File.Exists(resolved).Must().BeTrue();
    }
}
