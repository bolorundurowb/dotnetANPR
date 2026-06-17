using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace DotNetANPR.Configuration;

/// <summary>
/// Singleton configuration manager that stores all ANPR settings as key-value pairs.
/// Replaces the Java Properties-based configuration with a simple dictionary-backed
/// POCO that supports XML persistence compatible with Java Properties XML format.
/// </summary>
public sealed class Configurator
{
    private static Configurator? _instance;
    private static readonly object Lock = new();

    private readonly Dictionary<string, string> _properties = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the singleton instance of the <see cref="Configurator"/>.
    /// On first access, loads defaults and attempts to load from the default config file.
    /// </summary>
    public static Configurator Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (Lock)
                {
                    _instance ??= new Configurator();
                }
            }

            return _instance;
        }
    }

    private Configurator()
    {
        SetDefaults();

        var defaultPath = GetDefaultConfigPath();
        if (File.Exists(defaultPath))
        {
            LoadFromXml(defaultPath);
        }
    }

    /// <summary>
    /// Creates a new <see cref="Configurator"/> with defaults, then loads overrides
    /// from the specified XML file.
    /// </summary>
    /// <param name="filePath">Path to the XML configuration file.</param>
    public Configurator(string filePath) : this()
    {
        if (File.Exists(filePath))
        {
            LoadFromXml(filePath);
        }
    }

    /// <summary>
    /// Gets the value associated with the specified configuration key, converted to
    /// the requested type.
    /// </summary>
    /// <typeparam name="T">The target type (e.g. <see cref="int"/>, <see cref="double"/>, <see cref="string"/>).</typeparam>
    /// <param name="name">The configuration key.</param>
    /// <returns>The value converted to <typeparamref name="T"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when <paramref name="name"/> is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted.</exception>
    public T Get<T>(string name)
    {
        if (!_properties.TryGetValue(name, out var rawValue))
        {
            throw new KeyNotFoundException($"Configuration key '{name}' not found.");
        }

        return (T)Convert.ChangeType(rawValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Sets the value for the specified configuration key.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="name">The configuration key.</param>
    /// <param name="value">The value to store.</param>
    public void Set<T>(string name, T value)
    {
        _properties[name] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>
    /// Gets a file-system path value for the specified key, normalizing directory separators
    /// for the current platform.
    /// </summary>
    /// <param name="name">The configuration key whose value is a path.</param>
    /// <returns>The path with platform-appropriate directory separators.</returns>
    public string GetPath(string name)
    {
        var rawValue = Get<string>(name);
        return rawValue.Replace('/', Path.DirectorySeparatorChar)
                       .Replace('\\', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Saves the current configuration to an XML file using a format compatible with
    /// Java's <c>Properties.storeToXML</c>:
    /// <code>
    /// &lt;properties&gt;
    ///   &lt;entry key="name"&gt;value&lt;/entry&gt;
    /// &lt;/properties&gt;
    /// </code>
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    public void SaveToXml(string path)
    {
        var root = new XElement("properties");
        foreach (var kvp in _properties)
        {
            root.Add(new XElement("entry", new XAttribute("key", kvp.Key), kvp.Value));
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XDocType("properties", null, "http://java.sun.com/dtd/properties.dtd", null),
            root);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        doc.Save(path);
    }

    /// <summary>
    /// Saves the current configuration to the default config file location.
    /// </summary>
    public void Save() => SaveToXml(GetDefaultConfigPath());

    /// <summary>
    /// Loads configuration values from an XML file in Java Properties XML format.
    /// Values from the file override any existing defaults.
    /// </summary>
    /// <param name="path">The file path to read from.</param>
    public void LoadFromXml(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var doc = XDocument.Load(path);
        var root = doc.Element("properties");
        if (root is null)
        {
            return;
        }

        foreach (var entry in root.Elements("entry"))
        {
            var key = entry.Attribute("key")?.Value;
            if (key is not null)
            {
                _properties[key] = entry.Value;
            }
        }
    }

    /// <summary>
    /// Resets the singleton instance. Useful for testing.
    /// </summary>
    public static void Reset()
    {
        lock (Lock)
        {
            _instance = null;
        }
    }

    private static string GetDefaultConfigPath() => Path.Combine("Resources", "config.xml");

    private void SetDefaults()
    {
        // PHOTO
        Set("photo_adaptivethresholdingradius", 7); // 7 is recommended

        // BANDGRAPH
        Set("bandgraph_peakfootconstant", 0.55); //0.75
        Set("bandgraph_peakDiffMultiplicationConstant", 0.2);

        // CARSNAPSHOT
        Set("carsnapshot_distributormargins", 25);
        Set("carsnapshot_graphrankfilter", 9);

        // CARSNAPSHOTGRAPH
        Set("carsnapshotgraph_peakfootconstant", 0.55); //0.55
        Set("carsnapshotgraph_peakDiffMultiplicationConstant", 0.1);

        Set("intelligence_skewdetection", 0);

        // CHAR
        Set("char_normalizeddimensions_x", 8);   //8
        Set("char_normalizeddimensions_y", 13);  //13
        Set("char_resizeMethod", 1);             // 0=linear 1=average
        Set("char_featuresExtractionMethod", 0); //0=map, 1=edge
        Set("char_neuralNetworkPath", "./Resources/NeuralNetworks/network_avgres_813_map.xml");
        Set("char_learnAlphabetPath", "./Resources/Alphabets/Alphabet_8x13");
        Set("intelligence_classification_method", 0); // 0 = pattern match ,1=nn

        // PLATEGRAPH
        Set("plategraph_peakfootconstant", 0.7);
        Set("plategraph_rel_minpeaksize", 0.86); // 0.85

        // PLATEGRAPHHORIZONTALGRAPH
        Set("platehorizontalgraph_peakfootconstant", 0.05);
        Set("platehorizontalgraph_detectionType", 1); // 1=edgedetection 0=magnitudederivate

        // PLATEVERICALGRAPH
        Set("plateverticalgraph_peakfootconstant", 0.42);

        // INTELLIGENCE
        Set("intelligence_numberOfBands", 3);
        Set("intelligence_numberOfPlates", 3);
        Set("intelligence_numberOfChars", 20);

        Set("intelligence_minimumChars", 5);
        Set("intelligence_maximumChars", 15);

        // PLATE HEURISTICS
        Set("intelligence_maxCharWidthDispersion", 0.5); // in plate
        Set("intelligence_minPlateWidthHeightRatio", 0.5);
        Set("intelligence_maxPlateWidthHeightRatio", 15.0);

        // CHAR HEURISTICS
        Set("intelligence_minCharWidthHeightRatio", 0.1);
        Set("intelligence_maxCharWidthHeightRatio", 0.92);
        Set("intelligence_maxBrightnessCostDispersion", 0.161);
        Set("intelligence_maxContrastCostDispersion", 0.1);
        Set("intelligence_maxHueCostDispersion", 0.145);
        Set("intelligence_maxSaturationCostDispersion", 0.24); //0.15
        Set("intelligence_maxHeightCostDispersion", 0.2);
        Set("intelligence_maxSimilarityCostDispersion", 100);

        // RECOGNITION
        Set("intelligence_syntaxanalysis", 2);
        Set("intelligence_syntaxDescriptionFile", "./Resources/syntax.xml");

        // NEURAL NETWORK
        //int maxK, double eps, double lambda, double micro
        Set("neural_maxk", 8000);   // maximum K
        Set("neural_eps", 0.07);    // epsilon
        Set("neural_lambda", 0.05); // lambda factor
        Set("neural_micro", 0.5);   // micro
        Set("neural_topology", 20);

        Set("reportgeneratorcss", "./Resources/ReportGenerator/style.css");
    }
}
