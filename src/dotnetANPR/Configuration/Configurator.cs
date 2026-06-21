using System;
using System.IO;
using PropertyConfig;

namespace dotnetANPR.Configuration;

/// <summary>
/// Holds configurable parameters for the recognition pipeline.
/// Parameters are stored as key-value pairs and can be loaded from or saved to an XML file.
/// </summary>
internal sealed class Configurator
{
    private readonly Properties _properties;

    private const string DefaultConfigRelativePath = "Resources/config.xml";

    public Configurator()
    {
        _properties = [];
        SetDefaults();
    }

    public Configurator(string filePath) : this()
    {
        if (File.Exists(filePath))
            LoadConfiguration(filePath);
    }

    public static Configurator CreateDefault(ResourceLocator locator)
    {
        var resolved = locator.Resolve(DefaultConfigRelativePath);
        return File.Exists(resolved) ? new Configurator(resolved) : new Configurator();
    }

    public string GetPath(string name)
    {
        var rawValue = Get<string>(name);
        return rawValue.Replace('/', Path.DirectorySeparatorChar);
    }

    public T Get<T>(string name)
    {
        var rawValue = _properties[name];
        return (T)Convert.ChangeType(rawValue, typeof(T));
    }

    public void Set<T>(string name, T value) => _properties[name] = value?.ToString();

    public void Save(string outputFilePath) => _properties.StoreToXml(outputFilePath);

    public AnprSettings CreateSettings(ResourceLocator locator) => AnprSettings.FromConfigurator(this, locator);

    private void LoadConfiguration(string filePath) => _properties.LoadFromXml(filePath);

    private void SetDefaults()
    {
        Set("photo_adaptivethresholdingradius", 7);

        Set("bandgraph_peakfootconstant", 0.55);
        Set("bandgraph_peakDiffMultiplicationConstant", 0.2);

        Set("carsnapshot_distributormargins", 25);
        Set("carsnapshot_graphrankfilter", 9);
        Set("carsnapshotgraph_peakfootconstant", 0.55);
        Set("carsnapshotgraph_peakDiffMultiplicationConstant", 0.1);

        Set("intelligence_skewdetection", 0);

        Set("char_normalizeddimensions_x", 8);
        Set("char_normalizeddimensions_y", 13);
        Set("char_resizeMethod", 1);
        Set("char_featuresExtractionMethod", 0);
        Set("char_neuralNetworkPath", "Resources/neuralnetworks/network_avgres_813_map.xml");
        Set("char_learnAlphabetPath", "Resources/alphabets/alphabet_8x13");
        Set("intelligence_classification_method", 0);

        Set("plategraph_peakfootconstant", 0.7);
        Set("plategraph_rel_minpeaksize", 0.86);
        Set("platehorizontalgraph_peakfootconstant", 0.05);
        Set("platehorizontalgraph_detectionType", 1);
        Set("plateverticalgraph_peakfootconstant", 0.42);

        Set("intelligence_numberOfBands", 3);
        Set("intelligence_numberOfPlates", 3);
        Set("intelligence_numberOfChars", 20);
        Set("intelligence_minimumChars", 5);
        Set("intelligence_maximumChars", 15);

        Set("intelligence_maxCharWidthDispersion", 0.5);
        Set("intelligence_minPlateWidthHeightRatio", 0.5);
        Set("intelligence_maxPlateWidthHeightRatio", 15.0);

        Set("intelligence_minCharWidthHeightRatio", 0.1);
        Set("intelligence_maxCharWidthHeightRatio", 0.92);
        Set("intelligence_minEdgeCharWidthHeightRatio", 0.12);
        Set("intelligence_maxBrightnessCostDispersion", 0.161);
        Set("intelligence_maxContrastCostDispersion", 0.1);
        Set("intelligence_maxHueCostDispersion", 0.145);
        Set("intelligence_maxSaturationCostDispersion", 0.24);
        Set("intelligence_maxHeightCostDispersion", 0.2);
        Set("intelligence_maxSimilarityCostDispersion", 100);

        Set("intelligence_syntaxanalysis", 2);
        Set("intelligence_syntaxDescriptionFile", "Resources/syntax.xml");

        Set("neural_maxk", 8000);
        Set("neural_eps", 0.07);
        Set("neural_lambda", 0.05);
        Set("neural_micro", 0.5);
        Set("neural_topology", 20);
    }
}
