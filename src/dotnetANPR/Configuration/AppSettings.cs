using System.Text.Json.Serialization;

namespace DotNetANPR.Configuration;



// Main settings object
public class AppSettings
{
    [JsonPropertyName("imageAnalysis")] public ImageAnalysisSettings ImageAnalysis { get; set; }

    [JsonPropertyName("plateCandidates")] public PlateCandidateSettings PlateCandidates { get; set; }

    [JsonPropertyName("heuristics")] public HeuristicSettings Heuristics { get; set; }

    [JsonPropertyName("recognition")] public RecognitionSettings Recognition { get; set; }

    [JsonPropertyName("neuralNetwork")] public NeuralNetworkSettings NeuralNetwork { get; set; }

    [JsonPropertyName("paths")] public PathSettings Paths { get; set; }
}

public class ImageAnalysisSettings
{
    [JsonPropertyName("photoAdaptiveThresholdingRadius")]
    public int PhotoAdaptiveThresholdingRadius { get; set; }

    [JsonPropertyName("skewDetection")] public int SkewDetection { get; set; }
    // ... other properties from config.json imageAnalysis section
}

public class PlateCandidateSettings
{
    [JsonPropertyName("numberOfBands")] public int NumberOfBands { get; set; }
    // ...
}

public class HeuristicSettings
{
    [JsonPropertyName("plate")] public PlateHeuristics Plate { get; set; }

    [JsonPropertyName("char")] public CharHeuristics Char { get; set; }
}

public class PlateHeuristics
{
    [JsonPropertyName("minimumChars")] public int MinimumChars { get; set; }
    // ...
}

public class CharHeuristics
{
    [JsonPropertyName("minCharWidthHeightRatio")]
    public double MinCharWidthHeightRatio { get; set; }
    // ...
}

public class RecognitionSettings
{
    [JsonPropertyName("classificationMethod")]
    public int ClassificationMethod { get; set; }

    [JsonPropertyName("charNeuralNetworkPath")]
    public string CharNeuralNetworkPath { get; set; }
    // ...
}

public class NeuralNetworkSettings
{
    [JsonPropertyName("maxK")] public int MaxK { get; set; }
    // ...
}

public class PathSettings
{
    [JsonPropertyName("helpFile")] public string HelpFile { get; set; }
    // ...
}