using System.Text.Json.Serialization;

namespace DotNetANPR.Config;

public class AppSettings
{
    [JsonPropertyName("imageAnalysis")]
    public ImageAnalysisSettings ImageAnalysis { get; set; }

    [JsonPropertyName("plateCandidates")]
    public PlateCandidateSettings PlateCandidates { get; set; }

    [JsonPropertyName("heuristics")]
    public HeuristicSettings Heuristics { get; set; }

    [JsonPropertyName("recognition")]
    public RecognitionSettings Recognition { get; set; }

    [JsonPropertyName("neuralNetwork")]
    public NeuralNetworkSettings NeuralNetwork { get; set; }
}

public class ImageAnalysisSettings
{
    [JsonPropertyName("photoAdaptiveThresholdingRadius")]
    public int PhotoAdaptiveThresholdingRadius { get; set; }

    [JsonPropertyName("skewDetection")]
    public int SkewDetection { get; set; }

    [JsonPropertyName("bandGraphPeakFootConstant")]
    public double BandGraphPeakFootConstant { get; set; }

    [JsonPropertyName("bandGraphPeakDiffMultiplicationConstant")]
    public double BandGraphPeakDiffMultiplicationConstant { get; set; }

    [JsonPropertyName("carSnapshotDistributorMargins")]
    public int CarSnapshotDistributorMargins { get; set; }

    [JsonPropertyName("carSnapshotGraphRankFilter")]
    public int CarSnapshotGraphRankFilter { get; set; }

    [JsonPropertyName("carSnapshotGraphPeakFootConstant")]
    public double CarSnapshotGraphPeakFootConstant { get; set; }

    [JsonPropertyName("carSnapshotGraphPeakDiffMultiplicationConstant")]
    public double CarSnapshotGraphPeakDiffMultiplicationConstant { get; set; }

    [JsonPropertyName("plateGraphPeakFootConstant")]
    public double PlateGraphPeakFootConstant { get; set; }

    [JsonPropertyName("plateGraphRelativeMinPeakSize")]
    public double PlateGraphRelativeMinPeakSize { get; set; }

    [JsonPropertyName("plateHorizontalGraphPeakFootConstant")]
    public double PlateHorizontalGraphPeakFootConstant { get; set; }

    [JsonPropertyName("plateHorizontalGraphDetectionType")]
    public int PlateHorizontalGraphDetectionType { get; set; }

    [JsonPropertyName("plateVerticalGraphPeakFootConstant")]
    public double PlateVerticalGraphPeakFootConstant { get; set; }
}

public class PlateCandidateSettings
{
    [JsonPropertyName("numberOfBands")]
    public int NumberOfBands { get; set; }

    [JsonPropertyName("numberOfPlates")]
    public int NumberOfPlates { get; set; }

    [JsonPropertyName("numberOfChars")]
    public int NumberOfChars { get; set; }
}

public class HeuristicSettings
{
    [JsonPropertyName("plate")]
    public PlateHeuristics Plate { get; set; }

    [JsonPropertyName("char")]
    public CharHeuristics Char { get; set; }
}

public class PlateHeuristics
{
    [JsonPropertyName("minimumChars")]
    public int MinimumChars { get; set; }

    [JsonPropertyName("maximumChars")]
    public int MaximumChars { get; set; }

    [JsonPropertyName("maxCharWidthDispersion")]
    public double MaxCharWidthDispersion { get; set; }

    [JsonPropertyName("minPlateWidthHeightRatio")]
    public double MinPlateWidthHeightRatio { get; set; }

    [JsonPropertyName("maxPlateWidthHeightRatio")]
    public double MaxPlateWidthHeightRatio { get; set; }
}

public class CharHeuristics
{
    [JsonPropertyName("minCharWidthHeightRatio")]
    public double MinCharWidthHeightRatio { get; set; }

    [JsonPropertyName("maxCharWidthHeightRatio")]
    public double MaxCharWidthHeightRatio { get; set; }

    // ... other properties ...
}

public class RecognitionSettings
{
    [JsonPropertyName("syntaxAnalysisMode")]
    public int SyntaxAnalysisMode { get; set; }

    [JsonPropertyName("syntaxDescriptionFile")]
    public string SyntaxDescriptionFile { get; set; }

    [JsonPropertyName("classificationMethod")]
    public int ClassificationMethod { get; set; }

    [JsonPropertyName("charNormalizedDimensionsX")]
    public int CharNormalizedDimensionsX { get; set; }

    [JsonPropertyName("charNormalizedDimensionsY")]
    public int CharNormalizedDimensionsY { get; set; }

    [JsonPropertyName("charResizeMethod")]
    public int CharResizeMethod { get; set; }

    [JsonPropertyName("charFeaturesExtractionMethod")]
    public int CharFeaturesExtractionMethod { get; set; }

    [JsonPropertyName("charNeuralNetworkPath")]
    public string CharNeuralNetworkPath { get; set; }

    [JsonPropertyName("charLearnAlphabetPath")]
    public string CharLearnAlphabetPath { get; set; }
}

public class NeuralNetworkSettings
{
    [JsonPropertyName("maxK")]
    public int MaxK { get; set; }
    // ... other properties ...
}