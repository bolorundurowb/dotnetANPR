using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetANPR.Utilities;

namespace DotNetANPR.Configuration;

/// <summary>
/// Strongly-typed ANPR configuration backed by JSON serialization.
/// Access the singleton via <see cref="Instance"/> or load a custom config with <see cref="Load"/>.
/// </summary>
public sealed class AnprConfig
{
    private static AnprConfig? _instance;
    private static readonly object Lock = new();

    /// <summary>
    /// Gets or sets the singleton configuration instance. Lazily initialized with defaults.
    /// </summary>
    public static AnprConfig Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (Lock)
                {
                    if (_instance is null)
                    {
                        _instance = new AnprConfig();
                        var json = ResourceHelper.ReadText("Resources/config.json");
                        if (!string.IsNullOrEmpty(json))
                        {
                            var loaded = JsonSerializer.Deserialize<AnprConfig>(json);
                            if (loaded is not null)
                                _instance = loaded;
                        }
                    }
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Loads configuration from a JSON file, replacing the singleton instance.
    /// </summary>
    /// <param name="filePath">Path to the JSON configuration file.</param>
    /// <returns>The loaded configuration.</returns>
    public static AnprConfig Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<AnprConfig>(json) ?? new AnprConfig();
        lock (Lock) { _instance = config; }
        return config;
    }

    /// <summary>
    /// Resets the singleton instance to defaults. Useful for testing.
    /// </summary>
    public static void Reset()
    {
        lock (Lock) { _instance = null; }
    }

    /// <summary>Photo processing settings.</summary>
    [JsonPropertyName("photo")]
    public PhotoConfig Photo { get; set; } = new();

    /// <summary>Band graph analysis settings.</summary>
    [JsonPropertyName("bandGraph")]
    public BandGraphConfig BandGraph { get; set; } = new();

    /// <summary>Car snapshot processing settings.</summary>
    [JsonPropertyName("carSnapshot")]
    public CarSnapshotConfig CarSnapshot { get; set; } = new();

    /// <summary>Car snapshot graph analysis settings.</summary>
    [JsonPropertyName("carSnapshotGraph")]
    public CarSnapshotGraphConfig CarSnapshotGraph { get; set; } = new();

    /// <summary>Character normalization and feature extraction settings.</summary>
    [JsonPropertyName("character")]
    public CharacterConfig Character { get; set; } = new();

    /// <summary>Plate graph analysis settings.</summary>
    [JsonPropertyName("plateGraph")]
    public PlateGraphConfig PlateGraph { get; set; } = new();

    /// <summary>Plate horizontal graph settings.</summary>
    [JsonPropertyName("plateHorizontalGraph")]
    public PlateHorizontalGraphConfig PlateHorizontalGraph { get; set; } = new();

    /// <summary>Plate vertical graph settings.</summary>
    [JsonPropertyName("plateVerticalGraph")]
    public PlateVerticalGraphConfig PlateVerticalGraph { get; set; } = new();

    /// <summary>Intelligence pipeline settings (recognition, heuristics, syntax).</summary>
    [JsonPropertyName("intelligence")]
    public IntelligenceConfig Intelligence { get; set; } = new();

    /// <summary>Neural network training settings.</summary>
    [JsonPropertyName("neuralNetwork")]
    public NeuralNetworkConfig NeuralNetwork { get; set; } = new();
}

/// <summary>Photo adaptive thresholding settings.</summary>
public sealed class PhotoConfig
{
    /// <summary>Radius for adaptive thresholding. Default: 7.</summary>
    [JsonPropertyName("adaptiveThresholdingRadius")]
    public int AdaptiveThresholdingRadius { get; set; } = 7;
}

/// <summary>Band graph peak detection settings.</summary>
public sealed class BandGraphConfig
{
    /// <summary>Peak foot constant. Default: 0.55.</summary>
    [JsonPropertyName("peakFootConstant")]
    public double PeakFootConstant { get; set; } = 0.55;

    /// <summary>Peak difference multiplication constant. Default: 0.2.</summary>
    [JsonPropertyName("peakDiffMultiplicationConstant")]
    public double PeakDiffMultiplicationConstant { get; set; } = 0.2;
}

/// <summary>Car snapshot segmentation settings.</summary>
public sealed class CarSnapshotConfig
{
    /// <summary>Distributor margins in pixels. Default: 25.</summary>
    [JsonPropertyName("distributorMargins")]
    public int DistributorMargins { get; set; } = 25;

    /// <summary>Graph rank filter size. Default: 9.</summary>
    [JsonPropertyName("graphRankFilter")]
    public int GraphRankFilter { get; set; } = 9;
}

/// <summary>Car snapshot graph peak detection settings.</summary>
public sealed class CarSnapshotGraphConfig
{
    /// <summary>Peak foot constant. Default: 0.55.</summary>
    [JsonPropertyName("peakFootConstant")]
    public double PeakFootConstant { get; set; } = 0.55;

    /// <summary>Peak difference multiplication constant. Default: 0.1.</summary>
    [JsonPropertyName("peakDiffMultiplicationConstant")]
    public double PeakDiffMultiplicationConstant { get; set; } = 0.1;
}

/// <summary>Character normalization and classification settings.</summary>
public sealed class CharacterConfig
{
    /// <summary>Normalized character width in pixels. Default: 8.</summary>
    [JsonPropertyName("normalizedWidth")]
    public int NormalizedWidth { get; set; } = 8;

    /// <summary>Normalized character height in pixels. Default: 13.</summary>
    [JsonPropertyName("normalizedHeight")]
    public int NormalizedHeight { get; set; } = 13;

    /// <summary>Resize method: 0 = linear, 1 = average. Default: 1.</summary>
    [JsonPropertyName("resizeMethod")]
    public int ResizeMethod { get; set; } = 1;

    /// <summary>Feature extraction method: 0 = map, 1 = edge. Default: 0.</summary>
    [JsonPropertyName("featuresExtractionMethod")]
    public int FeaturesExtractionMethod { get; set; } = 0;

    /// <summary>Path to the pre-trained neural network XML file.</summary>
    [JsonPropertyName("neuralNetworkPath")]
    public string NeuralNetworkPath { get; set; } = "./Resources/NeuralNetworks/network_avgres_813_map.xml";

    /// <summary>Path to the alphabet training images directory.</summary>
    [JsonPropertyName("learnAlphabetPath")]
    public string LearnAlphabetPath { get; set; } = "./Resources/Alphabets/Alphabet_8x13";
}

/// <summary>Plate graph peak detection settings.</summary>
public sealed class PlateGraphConfig
{
    /// <summary>Peak foot constant. Default: 0.7.</summary>
    [JsonPropertyName("peakFootConstant")]
    public double PeakFootConstant { get; set; } = 0.7;

    /// <summary>Minimum relative peak size. Default: 0.86.</summary>
    [JsonPropertyName("relativeMinPeakSize")]
    public double RelativeMinPeakSize { get; set; } = 0.86;
}

/// <summary>Plate horizontal graph settings.</summary>
public sealed class PlateHorizontalGraphConfig
{
    /// <summary>Peak foot constant. Default: 0.05.</summary>
    [JsonPropertyName("peakFootConstant")]
    public double PeakFootConstant { get; set; } = 0.05;

    /// <summary>Detection type: 0 = magnitude derivative, 1 = edge detection. Default: 1.</summary>
    [JsonPropertyName("detectionType")]
    public int DetectionType { get; set; } = 1;
}

/// <summary>Plate vertical graph settings.</summary>
public sealed class PlateVerticalGraphConfig
{
    /// <summary>Peak foot constant. Default: 0.42.</summary>
    [JsonPropertyName("peakFootConstant")]
    public double PeakFootConstant { get; set; } = 0.42;
}

/// <summary>Intelligence pipeline and heuristic settings.</summary>
public sealed class IntelligenceConfig
{
    /// <summary>Classification method: 0 = KNN, 1 = neural network. Default: 0.</summary>
    [JsonPropertyName("classificationMethod")]
    public int ClassificationMethod { get; set; } = 0;

    /// <summary>Number of horizontal bands to extract. Default: 3.</summary>
    [JsonPropertyName("numberOfBands")]
    public int NumberOfBands { get; set; } = 3;

    /// <summary>Number of plate candidates per band. Default: 3.</summary>
    [JsonPropertyName("numberOfPlates")]
    public int NumberOfPlates { get; set; } = 3;

    /// <summary>Number of character candidates per plate. Default: 20.</summary>
    [JsonPropertyName("numberOfChars")]
    public int NumberOfChars { get; set; } = 20;

    /// <summary>Minimum characters for a valid plate. Default: 5.</summary>
    [JsonPropertyName("minimumChars")]
    public int MinimumChars { get; set; } = 5;

    /// <summary>Maximum characters for a valid plate. Default: 15.</summary>
    [JsonPropertyName("maximumChars")]
    public int MaximumChars { get; set; } = 15;

    /// <summary>Skew detection mode: 0 = off, nonzero = on. Default: 0.</summary>
    [JsonPropertyName("skewDetection")]
    public int SkewDetection { get; set; } = 0;

    /// <summary>Syntax analysis mode: 0 = none, 1 = equal length, 2 = equal or shorter. Default: 2.</summary>
    [JsonPropertyName("syntaxAnalysis")]
    public int SyntaxAnalysis { get; set; } = 2;

    /// <summary>Path to syntax description XML file.</summary>
    [JsonPropertyName("syntaxDescriptionFile")]
    public string SyntaxDescriptionFile { get; set; } = "./Resources/syntax.xml";

    /// <summary>Maximum character width dispersion within a plate. Default: 0.5.</summary>
    [JsonPropertyName("maxCharWidthDispersion")]
    public double MaxCharWidthDispersion { get; set; } = 0.5;

    /// <summary>Minimum plate width/height ratio. Default: 0.5.</summary>
    [JsonPropertyName("minPlateWidthHeightRatio")]
    public double MinPlateWidthHeightRatio { get; set; } = 0.5;

    /// <summary>Maximum plate width/height ratio. Default: 15.0.</summary>
    [JsonPropertyName("maxPlateWidthHeightRatio")]
    public double MaxPlateWidthHeightRatio { get; set; } = 15.0;

    /// <summary>Minimum character width/height ratio. Default: 0.1.</summary>
    [JsonPropertyName("minCharWidthHeightRatio")]
    public double MinCharWidthHeightRatio { get; set; } = 0.1;

    /// <summary>Maximum character width/height ratio. Default: 0.92.</summary>
    [JsonPropertyName("maxCharWidthHeightRatio")]
    public double MaxCharWidthHeightRatio { get; set; } = 0.92;

    /// <summary>Maximum brightness cost dispersion. Default: 0.161.</summary>
    [JsonPropertyName("maxBrightnessCostDispersion")]
    public double MaxBrightnessCostDispersion { get; set; } = 0.161;

    /// <summary>Maximum contrast cost dispersion. Default: 0.1.</summary>
    [JsonPropertyName("maxContrastCostDispersion")]
    public double MaxContrastCostDispersion { get; set; } = 0.1;

    /// <summary>Maximum hue cost dispersion. Default: 0.145.</summary>
    [JsonPropertyName("maxHueCostDispersion")]
    public double MaxHueCostDispersion { get; set; } = 0.145;

    /// <summary>Maximum saturation cost dispersion. Default: 0.24.</summary>
    [JsonPropertyName("maxSaturationCostDispersion")]
    public double MaxSaturationCostDispersion { get; set; } = 0.24;

    /// <summary>Maximum height cost dispersion. Default: 0.2.</summary>
    [JsonPropertyName("maxHeightCostDispersion")]
    public double MaxHeightCostDispersion { get; set; } = 0.2;

    /// <summary>Maximum similarity cost dispersion. Default: 100.</summary>
    [JsonPropertyName("maxSimilarityCostDispersion")]
    public double MaxSimilarityCostDispersion { get; set; } = 100;
}

/// <summary>Neural network training hyperparameters.</summary>
public sealed class NeuralNetworkConfig
{
    /// <summary>Maximum training iterations. Default: 8000.</summary>
    [JsonPropertyName("maxK")]
    public int MaxK { get; set; } = 8000;

    /// <summary>Epsilon (learning rate). Default: 0.07.</summary>
    [JsonPropertyName("eps")]
    public double Eps { get; set; } = 0.07;

    /// <summary>Lambda factor. Default: 0.05.</summary>
    [JsonPropertyName("lambda")]
    public double Lambda { get; set; } = 0.05;

    /// <summary>Micro factor. Default: 0.5.</summary>
    [JsonPropertyName("micro")]
    public double Micro { get; set; } = 0.5;

    /// <summary>Hidden layer topology (number of hidden neurons). Default: 20.</summary>
    [JsonPropertyName("topology")]
    public int Topology { get; set; } = 20;
}
