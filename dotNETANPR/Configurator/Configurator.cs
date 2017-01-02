using System;
using System.Drawing;
using System.IO;

namespace dotNETANPR.Configurator
{
    public class Configurator
    {
        // Default config file name
        public string ConfigurationFileName { get; set; } = "config.xml";

        // Default class file comment
        private string comment = "This is the global configuration file for dotNETANPR";

        private PropertyConfig list;

        public Configurator()
        {
            list = new PropertyConfig();

            // PHOTO
            SetIntProperty("photo_adaptivethresholdingradius", 7); // 7 is recommended

            // BANDGRAPH
            SetDoubleProperty("bandgraph_peakfootconstant", 0.55); //0.75
            SetDoubleProperty("bandgraph_peakDiffMultiplicationConstant", 0.2);

            // CARSNAPSHOT
            SetIntProperty("carsnapshot_distributormargins", 25);
            SetIntProperty("carsnapshot_graphrankfilter", 9);


            // CARSNAPSHOTGRAPH
            SetDoubleProperty("carsnapshotgraph_peakfootconstant", 0.55); //0.55
            SetDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant", 0.1);


            SetIntProperty("intelligence_skewdetection", 0);

            // CHAR
            SetIntProperty("char_normalizeddimensions_x", 8); //8
            SetIntProperty("char_normalizeddimensions_y", 13); //13
            SetIntProperty("char_resizeMethod", 1); // 0=linear 1=average
            SetIntProperty("char_featuresExtractionMethod", 0); //0=map, 1=edge
            SetStringProperty("char_neuralNetworkPath", "./resources/neuralnetworks/network_avgres_813_map.xml");
            SetStringProperty("char_learnAlphabetPath", "./resources/alphabets/alphabet_8x13");
            SetIntProperty("intelligence_classification_method", 0); // 0 = pattern match ,1=nn

            // PLATEGRAPH
            SetDoubleProperty("plategraph_peakfootconstant", 0.7);
            SetDoubleProperty("plategraph_rel_minpeaksize", 0.86); // 0.85

            // PLATEGRAPHHORIZONTALGRAPH
            SetDoubleProperty("platehorizontalgraph_peakfootconstant", 0.05);
            SetIntProperty("platehorizontalgraph_detectionType", 1); // 1=edgedetection 0=magnitudederivate

            // PLATEVERICALGRAPH
            SetDoubleProperty("plateverticalgraph_peakfootconstant", 0.42);

            // INTELLIGENCE
            SetIntProperty("intelligence_numberOfBands", 3);
            SetIntProperty("intelligence_numberOfPlates", 3);
            SetIntProperty("intelligence_numberOfChars", 20);

            SetIntProperty("intelligence_minimumChars", 5);
            SetIntProperty("intelligence_maximumChars", 15);

            // PLATE HEURISTICS
            SetDoubleProperty("intelligence_maxCharWidthDispersion", 0.5); // in plate
            SetDoubleProperty("intelligence_minPlateWidthHeightRatio", 0.5);
            SetDoubleProperty("intelligence_maxPlateWidthHeightRatio", 15.0);

            // CHAR HEURISTICS
            SetDoubleProperty("intelligence_minCharWidthHeightRatio", 0.1);
            SetDoubleProperty("intelligence_maxCharWidthHeightRatio", 0.92);
            SetDoubleProperty("intelligence_maxBrightnessCostDispersion", 0.161);
            SetDoubleProperty("intelligence_maxContrastCostDispersion", 0.1);
            SetDoubleProperty("intelligence_maxHueCostDispersion", 0.145);
            SetDoubleProperty("intelligence_maxSaturationCostDispersion", 0.24); //0.15
            SetDoubleProperty("intelligence_maxHeightCostDispersion", 0.2);
            SetDoubleProperty("intelligence_maxSimilarityCostDispersion", 100);

            // RECOGNITION
            SetIntProperty("intelligence_syntaxanalysis", 2);
            SetStringProperty("intelligence_syntaxDescriptionFile", "./resources/syntax/syntax.xml");

            // NEURAL NETWORK
            //int maxK, double eps, double lambda, double micro

            SetIntProperty("neural_maxk", 8000); // maximum K
            SetDoubleProperty("neural_eps", 0.07); // epsilon
            SetDoubleProperty("neural_lambda", 0.05); // lambda factor
            SetDoubleProperty("neural_micro", 0.5); // micro
            SetIntProperty("neural_topology", 20);

            /* ***** END ***** Definition of property defaults  ******* */

            SetStringProperty("help_file_help", "./resources/help/help.html");
            SetStringProperty("help_file_about", "./resources/help/about.html");
            SetStringProperty("reportgeneratorcss", "./resources/reportgenerator/style.css");
        }

        public Configurator(string path)
        {
            new Configurator();
            try
            {
                LoadConfiguration(path);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: couldn't load configuration file " + path);

            }
        }

        public string GetStringProperty(string name)
        {
            return list[name];
        }

        public string GetPathProperty(string name)
        {
            return list[name].Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        public void SetStringProperty(string name, string value)
        {
            list[name] = value;
        }

        public int GetIntProperty(string name)
        {
            return int.Parse(list[name]);
        }

        public void SetIntProperty(string name, int value)
        {
            list[name] = value.ToString();
        }

        public double GetDoubleProperty(string name)
        {
            return double.Parse(list[name]);
        }

        public void SetDoubleProperty(string name, double value)
        {
            list[name] = value.ToString();
        }

        public Color GetColorProperty(string name)
        {
            return Color.FromArgb(int.Parse(list[name]));
        }

        public void SetColorProperty(string name, Color value)
        {
            list[name] = value.ToArgb().ToString();
        }

        public void SaveConfiguration()
        {
            list.StoreToXml(ConfigurationFileName, comment);
        }

        public void SaveConfiguration(string file)
        {
            list.StoreToXml(file, comment);
        }

        public void LoadConfiguration()
        {
            list.LoadFromXml(ConfigurationFileName);
        }

        public void LoadConfiguration(string file)
        {
            list.LoadFromXml(file);
        }
    }
}
