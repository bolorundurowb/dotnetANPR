using System;
using System.IO;
using PropertyConfig;

namespace DotNetANPR.Configuration;

public sealed class Configurator
{
    private readonly Properties _properties;

    public string FileName { get; set; } = "config.xml";

    private Configurator()
    {
        _properties = new Properties();

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
        Set("intelligence_syntaxDescriptionFile", "./Resources/Syntax/syntax.xml");

        // NEURAL NETWORK
        //int maxK, double eps, double lambda, double micro
        Set("neural_maxk", 8000);   // maximum K
        Set("neural_eps", 0.07);    // epsilon
        Set("neural_lambda", 0.05); // lambda factor
        Set("neural_micro", 0.5);   // micro
        Set("neural_topology", 20);

        Set("help_file_help", "./Resources/Help/help.html");
        Set("help_file_about", "./Resources/Help/about.html");
        Set("reportgeneratorcss", "./Resources/ReportGenerator/style.css");
    }

    public Configurator(string filePath) : this() { LoadConfiguration(filePath); }

    public T Get<T>(string name)
    {
        var rawValue = _properties[name];
        return (T)Convert.ChangeType(rawValue, typeof(T));
    }

    public void Set<T>(string name, T value) => _properties[name] = value?.ToString();

    public void Save() => _properties.StoreToXml(FileName);

    public void LoadConfiguration(string filePath = null)
    {
        if (filePath == null)
            _properties.LoadFromXml();
        else
            _properties.LoadFromXml(filePath);
    }
}