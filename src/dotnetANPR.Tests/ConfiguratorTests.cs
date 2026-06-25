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

        settings.IntelligenceMinEdgeCharWidthHeightRatio.Verify().ToBe(0.12);
        settings.CharNormalizedDimensionsX.Verify().ToBe(8);
        settings.CharNormalizedDimensionsY.Verify().ToBe(13);
    }

    [TestMethod]
    public void ResourceLocator_ResolvesRelativePathFromBaseDirectory()
    {
        var locator = new ResourceLocator();
        var resolved = locator.Resolve("Resources/syntax.xml");
        File.Exists(resolved).Verify().ToBeTrue();
    }

    [TestMethod]
    public void Set_ThenGet_RoundtripsInteger()
    {
        var config = new Configurator();
        config.Set("char_normalizeddimensions_x", 16);
        config.Get<int>("char_normalizeddimensions_x").Verify().ToBe(16);
    }

    [TestMethod]
    public void Set_ThenGet_RoundtripsDouble()
    {
        var config = new Configurator();
        config.Set("bandgraph_peakfootconstant", 0.99);
        config.Get<double>("bandgraph_peakfootconstant").Verify().ToBeApproximately(0.99, 0.0001);
    }

    [TestMethod]
    public void Set_OverridesDefault()
    {
        var config = new Configurator();
        config.Set("intelligence_minimumChars", 3);
        var settings = config.CreateSettings(new ResourceLocator());
        settings.IntelligenceMinimumChars.Verify().ToBe(3);
    }

    [TestMethod]
    public void DefaultSettings_CharCountRange_IsReasonable()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinimumChars > 0).Verify().ToBeTrue();
        (settings.IntelligenceMaximumChars > settings.IntelligenceMinimumChars).Verify().ToBeTrue();
    }

    [TestMethod]
    public void DefaultSettings_PlateRatioRange_IsOrdered()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinPlateWidthHeightRatio <
         settings.IntelligenceMaxPlateWidthHeightRatio).Verify().ToBeTrue();
    }

    [TestMethod]
    public void DefaultSettings_CharRatioRange_IsOrdered()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        (settings.IntelligenceMinCharWidthHeightRatio <
         settings.IntelligenceMaxCharWidthHeightRatio).Verify().ToBeTrue();
    }

    [TestMethod]
    public void DefaultSettings_NeuralNetworkPath_FileExists()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        File.Exists(settings.CharNeuralNetworkPath).Verify().ToBeTrue();
    }

    [TestMethod]
    public void DefaultSettings_SyntaxDescriptionFile_FileExists()
    {
        var settings = new Configurator().CreateSettings(new ResourceLocator());
        File.Exists(settings.IntelligenceSyntaxDescriptionFile).Verify().ToBeTrue();
    }

    [TestMethod]
    public void GetPath_NormalisesForwardSlashes()
    {
        var config = new Configurator();
        var path = config.GetPath("char_neuralNetworkPath");
        (path.Contains('/') && Path.DirectorySeparatorChar == '\\').Verify().ToBeFalse();
    }

    [TestMethod]
    public void ResourceLocator_ResolvesNeuralNetworkFile()
    {
        var locator = new ResourceLocator();
        var resolved = locator.Resolve("Resources/neuralnetworks/network_avgres_813_map.xml");
        File.Exists(resolved).Verify().ToBeTrue();
    }
}
