namespace dotnetANPR.Configuration;

/// <summary>
/// Immutable snapshot of all pipeline configuration values.
/// </summary>
internal sealed class AnprSettings
{
    public int PhotoAdaptiveThresholdingRadius { get; init; }

    public double BandGraphPeakFootConstant { get; init; }
    public double BandGraphPeakDiffMultiplicationConstant { get; init; }

    public int CarSnapshotDistributorMargins { get; init; }
    public int CarSnapshotGraphRankFilter { get; init; }
    public double CarSnapshotGraphPeakFootConstant { get; init; }
    public double CarSnapshotGraphPeakDiffMultiplicationConstant { get; init; }

    public int IntelligenceSkewDetection { get; init; }

    public int CharNormalizedDimensionsX { get; init; }
    public int CharNormalizedDimensionsY { get; init; }
    public int CharResizeMethod { get; init; }
    public int CharFeaturesExtractionMethod { get; init; }
    public string CharNeuralNetworkPath { get; init; } = "";
    public string CharLearnAlphabetPath { get; init; } = "";
    public int IntelligenceClassificationMethod { get; init; }

    public double PlateGraphPeakFootConstant { get; init; }
    public double PlateGraphRelMinPeakSize { get; init; }
    public double PlateHorizontalGraphPeakFootConstant { get; init; }
    public int PlateHorizontalGraphDetectionType { get; init; }
    public double PlateVerticalGraphPeakFootConstant { get; init; }

    public int IntelligenceNumberOfBands { get; init; }
    public int IntelligenceNumberOfPlates { get; init; }
    public int IntelligenceNumberOfChars { get; init; }
    public int IntelligenceMinimumChars { get; init; }
    public int IntelligenceMaximumChars { get; init; }

    public double IntelligenceMaxCharWidthDispersion { get; init; }
    public double IntelligenceMinPlateWidthHeightRatio { get; init; }
    public double IntelligenceMaxPlateWidthHeightRatio { get; init; }

    public double IntelligenceMinCharWidthHeightRatio { get; init; }
    public double IntelligenceMaxCharWidthHeightRatio { get; init; }
    public double IntelligenceMinEdgeCharWidthHeightRatio { get; init; }
    public double IntelligenceMaxBrightnessCostDispersion { get; init; }
    public double IntelligenceMaxContrastCostDispersion { get; init; }
    public double IntelligenceMaxHueCostDispersion { get; init; }
    public double IntelligenceMaxSaturationCostDispersion { get; init; }
    public double IntelligenceMaxHeightCostDispersion { get; init; }
    public double IntelligenceMaxSimilarityCostDispersion { get; init; }

    public int IntelligenceSyntaxAnalysis { get; init; }
    public string IntelligenceSyntaxDescriptionFile { get; init; } = "";

    public int NeuralMaxK { get; init; }
    public double NeuralEps { get; init; }
    public double NeuralLambda { get; init; }
    public double NeuralMicro { get; init; }
    public int NeuralTopology { get; init; }

    public static AnprSettings FromConfigurator(Configurator configurator, ResourceLocator locator)
    {
        return new AnprSettings
        {
            PhotoAdaptiveThresholdingRadius = configurator.Get<int>("photo_adaptivethresholdingradius"),
            BandGraphPeakFootConstant = configurator.Get<double>("bandgraph_peakfootconstant"),
            BandGraphPeakDiffMultiplicationConstant =
                configurator.Get<double>("bandgraph_peakDiffMultiplicationConstant"),
            CarSnapshotDistributorMargins = configurator.Get<int>("carsnapshot_distributormargins"),
            CarSnapshotGraphRankFilter = configurator.Get<int>("carsnapshot_graphrankfilter"),
            CarSnapshotGraphPeakFootConstant = configurator.Get<double>("carsnapshotgraph_peakfootconstant"),
            CarSnapshotGraphPeakDiffMultiplicationConstant =
                configurator.Get<double>("carsnapshotgraph_peakDiffMultiplicationConstant"),
            IntelligenceSkewDetection = configurator.Get<int>("intelligence_skewdetection"),
            CharNormalizedDimensionsX = configurator.Get<int>("char_normalizeddimensions_x"),
            CharNormalizedDimensionsY = configurator.Get<int>("char_normalizeddimensions_y"),
            CharResizeMethod = configurator.Get<int>("char_resizeMethod"),
            CharFeaturesExtractionMethod = configurator.Get<int>("char_featuresExtractionMethod"),
            CharNeuralNetworkPath = locator.Resolve(configurator.GetPath("char_neuralNetworkPath")),
            CharLearnAlphabetPath = locator.Resolve(configurator.GetPath("char_learnAlphabetPath")),
            IntelligenceClassificationMethod = configurator.Get<int>("intelligence_classification_method"),
            PlateGraphPeakFootConstant = configurator.Get<double>("plategraph_peakfootconstant"),
            PlateGraphRelMinPeakSize = configurator.Get<double>("plategraph_rel_minpeaksize"),
            PlateHorizontalGraphPeakFootConstant =
                configurator.Get<double>("platehorizontalgraph_peakfootconstant"),
            PlateHorizontalGraphDetectionType = configurator.Get<int>("platehorizontalgraph_detectionType"),
            PlateVerticalGraphPeakFootConstant =
                configurator.Get<double>("plateverticalgraph_peakfootconstant"),
            IntelligenceNumberOfBands = configurator.Get<int>("intelligence_numberOfBands"),
            IntelligenceNumberOfPlates = configurator.Get<int>("intelligence_numberOfPlates"),
            IntelligenceNumberOfChars = configurator.Get<int>("intelligence_numberOfChars"),
            IntelligenceMinimumChars = configurator.Get<int>("intelligence_minimumChars"),
            IntelligenceMaximumChars = configurator.Get<int>("intelligence_maximumChars"),
            IntelligenceMaxCharWidthDispersion = configurator.Get<double>("intelligence_maxCharWidthDispersion"),
            IntelligenceMinPlateWidthHeightRatio =
                configurator.Get<double>("intelligence_minPlateWidthHeightRatio"),
            IntelligenceMaxPlateWidthHeightRatio =
                configurator.Get<double>("intelligence_maxPlateWidthHeightRatio"),
            IntelligenceMinCharWidthHeightRatio =
                configurator.Get<double>("intelligence_minCharWidthHeightRatio"),
            IntelligenceMaxCharWidthHeightRatio =
                configurator.Get<double>("intelligence_maxCharWidthHeightRatio"),
            IntelligenceMinEdgeCharWidthHeightRatio =
                configurator.Get<double>("intelligence_minEdgeCharWidthHeightRatio"),
            IntelligenceMaxBrightnessCostDispersion =
                configurator.Get<double>("intelligence_maxBrightnessCostDispersion"),
            IntelligenceMaxContrastCostDispersion =
                configurator.Get<double>("intelligence_maxContrastCostDispersion"),
            IntelligenceMaxHueCostDispersion = configurator.Get<double>("intelligence_maxHueCostDispersion"),
            IntelligenceMaxSaturationCostDispersion =
                configurator.Get<double>("intelligence_maxSaturationCostDispersion"),
            IntelligenceMaxHeightCostDispersion =
                configurator.Get<double>("intelligence_maxHeightCostDispersion"),
            IntelligenceMaxSimilarityCostDispersion =
                configurator.Get<double>("intelligence_maxSimilarityCostDispersion"),
            IntelligenceSyntaxAnalysis = configurator.Get<int>("intelligence_syntaxanalysis"),
            IntelligenceSyntaxDescriptionFile =
                locator.Resolve(configurator.GetPath("intelligence_syntaxDescriptionFile")),
            NeuralMaxK = configurator.Get<int>("neural_maxk"),
            NeuralEps = configurator.Get<double>("neural_eps"),
            NeuralLambda = configurator.Get<double>("neural_lambda"),
            NeuralMicro = configurator.Get<double>("neural_micro"),
            NeuralTopology = configurator.Get<int>("neural_topology"),
        };
    }
}
