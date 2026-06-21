using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace dotnetANPR.Configuration;

/// <summary>
/// Singleton configuration store for dotnetANPR.
/// Values are loaded from a JSONC file and exposed via a flat string-keyed
/// dictionary so that all existing callers remain unchanged.
/// </summary>
public sealed class Configurator
{
    private static Configurator? _configurator;
    private readonly Dictionary<string, string?> _properties;

    private static readonly string FileName = "Resources/config.jsonc";

    // -------------------------------------------------------------------------
    // Construction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initialise with hard-coded defaults. Every key known to the rest of the
    /// codebase must appear here so that the system works even without a config
    /// file on disk.
    /// </summary>
    private Configurator()
    {
        _properties = new Dictionary<string, string?>(StringComparer.Ordinal);
        ApplyDefaults();
    }

    /// <summary>Initialise with defaults, then overlay values from <paramref name="filePath"/>.</summary>
    public Configurator(string filePath) : this() => LoadConfiguration(filePath);

    public static Configurator Instance => _configurator ??= new Configurator(FileName);

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Returns the value for <paramref name="name"/>, replacing '/' with the
    /// platform directory separator so paths work on Windows and Unix alike.</summary>
    public string GetPath(string name)
    {
        var rawValue = Get<string>(name);
        return rawValue.Replace('/', Path.DirectorySeparatorChar);
    }

    public T Get<T>(string name)
    {
        if (!_properties.TryGetValue(name, out var rawValue) || rawValue is null)
            throw new KeyNotFoundException($"Configuration key '{name}' not found.");

        return (T)Convert.ChangeType(rawValue, typeof(T));
    }

    public void Set<T>(string name, T value) => _properties[name] = value?.ToString();

    // -------------------------------------------------------------------------
    // Defaults
    // -------------------------------------------------------------------------

    private void ApplyDefaults()
    {
        // photo
        _properties["photo_adaptivethresholdingradius"] = "7";

        // bandGraph
        _properties["bandgraph_peakfootconstant"]                = "0.55";
        _properties["bandgraph_peakDiffMultiplicationConstant"]  = "0.2";

        // carSnapshot
        _properties["carsnapshot_distributormargins"] = "25";
        _properties["carsnapshot_graphrankfilter"]    = "9";

        // carSnapshotGraph
        _properties["carsnapshotgraph_peakfootconstant"]               = "0.55";
        _properties["carsnapshotgraph_peakDiffMultiplicationConstant"] = "0.1";

        // character
        _properties["char_normalizeddimensions_x"]   = "8";
        _properties["char_normalizeddimensions_y"]   = "13";
        _properties["char_resizeMethod"]             = "1";
        _properties["char_featuresExtractionMethod"] = "0";
        _properties["char_neuralNetworkPath"]  = "./Resources/NeuralNetworks/network_avgres_813_map.xml";
        _properties["char_learnAlphabetPath"]  = "./Resources/Alphabets/Alphabet_8x13";

        // plateGraph
        _properties["plategraph_peakfootconstant"]  = "0.7";
        _properties["plategraph_rel_minpeaksize"]   = "0.86";

        // plateHorizontalGraph
        _properties["platehorizontalgraph_peakfootconstant"] = "0.05";
        _properties["platehorizontalgraph_detectionType"]    = "1";

        // plateVerticalGraph
        _properties["plateverticalgraph_peakfootconstant"] = "0.42";

        // intelligence
        _properties["intelligence_classification_method"]   = "0";
        _properties["intelligence_numberOfBands"]           = "3";
        _properties["intelligence_numberOfPlates"]          = "3";
        _properties["intelligence_numberOfChars"]           = "20";
        _properties["intelligence_minimumChars"]            = "5";
        _properties["intelligence_maximumChars"]            = "15";
        _properties["intelligence_skewdetection"]           = "0";
        _properties["intelligence_syntaxanalysis"]          = "2";
        _properties["intelligence_syntaxDescriptionFile"]   = "./Resources/syntax.jsonc";
        _properties["intelligence_maxCharWidthDispersion"]  = "0.5";
        _properties["intelligence_minPlateWidthHeightRatio"]= "0.5";
        _properties["intelligence_maxPlateWidthHeightRatio"]= "15.0";
        _properties["intelligence_minCharWidthHeightRatio"] = "0.1";
        _properties["intelligence_maxCharWidthHeightRatio"] = "0.92";
        _properties["intelligence_maxBrightnessCostDispersion"]  = "0.161";
        _properties["intelligence_maxContrastCostDispersion"]    = "0.1";
        _properties["intelligence_maxHueCostDispersion"]         = "0.145";
        _properties["intelligence_maxSaturationCostDispersion"]  = "0.24";
        _properties["intelligence_maxHeightCostDispersion"]      = "0.2";
        _properties["intelligence_maxSimilarityCostDispersion"]  = "100.0";

        // neuralNetwork
        _properties["neural_maxk"]     = "8000";
        _properties["neural_eps"]      = "0.07";
        _properties["neural_lambda"]   = "0.05";
        _properties["neural_micro"]    = "0.5";
        _properties["neural_topology"] = "20";
    }

    // -------------------------------------------------------------------------
    // JSONC loading
    // -------------------------------------------------------------------------

    private void LoadConfiguration(string filePath)
    {
        if (!File.Exists(filePath))
            return; // keep defaults when the file is absent

        var json = File.ReadAllText(filePath);
        var docOptions = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };

        using var doc = JsonDocument.Parse(json, docOptions);
        var root = doc.RootElement;

        // Each Map* call reads a property from the JSON element and writes the
        // value into the flat-key dictionary under the name the codebase expects.
        // The mapping is explicit so the JSON can use idiomatic camelCase while
        // the internal keys preserve their original (sometimes mixed) casing.

        // photo
        if (root.TryGetProperty("photo", out var photo))
        {
            Map(photo, "adaptiveThresholdingRadius", "photo_adaptivethresholdingradius");
        }

        // bandGraph
        if (root.TryGetProperty("bandGraph", out var bandGraph))
        {
            Map(bandGraph, "peakFootConstant",               "bandgraph_peakfootconstant");
            Map(bandGraph, "peakDiffMultiplicationConstant", "bandgraph_peakDiffMultiplicationConstant");
        }

        // carSnapshot
        if (root.TryGetProperty("carSnapshot", out var carSnapshot))
        {
            Map(carSnapshot, "distributorMargins", "carsnapshot_distributormargins");
            Map(carSnapshot, "graphRankFilter",    "carsnapshot_graphrankfilter");
        }

        // carSnapshotGraph
        if (root.TryGetProperty("carSnapshotGraph", out var carSnapshotGraph))
        {
            Map(carSnapshotGraph, "peakFootConstant",               "carsnapshotgraph_peakfootconstant");
            Map(carSnapshotGraph, "peakDiffMultiplicationConstant", "carsnapshotgraph_peakDiffMultiplicationConstant");
        }

        // character
        if (root.TryGetProperty("character", out var character))
        {
            Map(character, "normalizedWidth",          "char_normalizeddimensions_x");
            Map(character, "normalizedHeight",         "char_normalizeddimensions_y");
            Map(character, "resizeMethod",             "char_resizeMethod");
            Map(character, "featuresExtractionMethod", "char_featuresExtractionMethod");
            Map(character, "neuralNetworkPath",        "char_neuralNetworkPath");
            Map(character, "learnAlphabetPath",        "char_learnAlphabetPath");
        }

        // plateGraph
        if (root.TryGetProperty("plateGraph", out var plateGraph))
        {
            Map(plateGraph, "peakFootConstant",   "plategraph_peakfootconstant");
            Map(plateGraph, "relativeMinPeakSize","plategraph_rel_minpeaksize");
        }

        // plateHorizontalGraph
        if (root.TryGetProperty("plateHorizontalGraph", out var plateHorizontalGraph))
        {
            Map(plateHorizontalGraph, "peakFootConstant", "platehorizontalgraph_peakfootconstant");
            Map(plateHorizontalGraph, "detectionType",    "platehorizontalgraph_detectionType");
        }

        // plateVerticalGraph
        if (root.TryGetProperty("plateVerticalGraph", out var plateVerticalGraph))
        {
            Map(plateVerticalGraph, "peakFootConstant", "plateverticalgraph_peakfootconstant");
        }

        // intelligence
        if (root.TryGetProperty("intelligence", out var intelligence))
        {
            Map(intelligence, "classificationMethod",      "intelligence_classification_method");
            Map(intelligence, "numberOfBands",             "intelligence_numberOfBands");
            Map(intelligence, "numberOfPlates",            "intelligence_numberOfPlates");
            Map(intelligence, "numberOfChars",             "intelligence_numberOfChars");
            Map(intelligence, "minimumChars",              "intelligence_minimumChars");
            Map(intelligence, "maximumChars",              "intelligence_maximumChars");
            Map(intelligence, "skewDetection",             "intelligence_skewdetection");
            Map(intelligence, "syntaxAnalysis",            "intelligence_syntaxanalysis");
            Map(intelligence, "syntaxDescriptionFile",     "intelligence_syntaxDescriptionFile");
            Map(intelligence, "maxCharWidthDispersion",    "intelligence_maxCharWidthDispersion");
            Map(intelligence, "minPlateWidthHeightRatio",  "intelligence_minPlateWidthHeightRatio");
            Map(intelligence, "maxPlateWidthHeightRatio",  "intelligence_maxPlateWidthHeightRatio");
            Map(intelligence, "minCharWidthHeightRatio",   "intelligence_minCharWidthHeightRatio");
            Map(intelligence, "maxCharWidthHeightRatio",   "intelligence_maxCharWidthHeightRatio");
            Map(intelligence, "maxBrightnessCostDispersion",  "intelligence_maxBrightnessCostDispersion");
            Map(intelligence, "maxContrastCostDispersion",    "intelligence_maxContrastCostDispersion");
            Map(intelligence, "maxHueCostDispersion",         "intelligence_maxHueCostDispersion");
            Map(intelligence, "maxSaturationCostDispersion",  "intelligence_maxSaturationCostDispersion");
            Map(intelligence, "maxHeightCostDispersion",      "intelligence_maxHeightCostDispersion");
            Map(intelligence, "maxSimilarityCostDispersion",  "intelligence_maxSimilarityCostDispersion");
        }

        // neuralNetwork
        if (root.TryGetProperty("neuralNetwork", out var neuralNetwork))
        {
            Map(neuralNetwork, "maxK",     "neural_maxk");
            Map(neuralNetwork, "eps",      "neural_eps");
            Map(neuralNetwork, "lambda",   "neural_lambda");
            Map(neuralNetwork, "micro",    "neural_micro");
            Map(neuralNetwork, "topology", "neural_topology");
        }
    }

    /// <summary>
    /// Reads <paramref name="jsonKey"/> from <paramref name="section"/> and, if
    /// present, stores its raw text representation under <paramref name="flatKey"/>.
    /// </summary>
    private void Map(JsonElement section, string jsonKey, string flatKey)
    {
        if (section.TryGetProperty(jsonKey, out var element))
            _properties[flatKey] = element.ToString();
    }
}
