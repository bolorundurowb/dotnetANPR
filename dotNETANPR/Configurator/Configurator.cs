using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace dotNETANPR.Configurator
{
    class Configurator
    {
        /* Default name of configuration file */
        private string fileName = "config.xml";
        /* Configuration file's comment */
        private string comment = "This is global configuration file for Automatic Number Plate Recognition System";
        private Properties list;

        public Configurator()
        {
            list = new Properties();
            /* ***** BEGIN *** Definition of property defaults  ******* */

            // PHOTO

            // adaptive thresholding radius (0 = no adaptive)
            this.SetIntProperty("photo_adaptivethresholdingradius", 7); // 7 is recommended

            // BANDGRAPH - spracovanie horizontalnej projekcie detekovanej oblasti znacky
            // na ose X sa detekuje peak, peakfoot, a nakoniec sa to nasobi p.d.m.c konstantou
            this.SetDoubleProperty("bandgraph_peakfootconstant", 0.55); //0.75
            this.SetDoubleProperty("bandgraph_peakDiffMultiplicationConstant", 0.2);

            // CARSNAPSHOT
            this.SetIntProperty("carsnapshot_distributormargins", 25);
            this.SetIntProperty("carsnapshot_graphrankfilter", 9);


            // CARSNAPSHOTGRAPH
            this.SetDoubleProperty("carsnapshotgraph_peakfootconstant", 0.55); //0.55
            this.SetDoubleProperty("carsnapshotgraph_peakDiffMultiplicationConstant", 0.1);


            this.SetIntProperty("intelligence_skewdetection", 0);

            // CHAR
            // this.SetDoubleProperty("char_contrastnormalizationconstant", 0.5);  //1.0
            this.SetIntProperty("char_normalizeddimensions_x", 8); //8
            this.SetIntProperty("char_normalizeddimensions_y", 13); //13
            this.SetIntProperty("char_resizeMethod", 1); // 0=linear 1=average
            this.SetIntProperty("char_featuresExtractionMethod", 0); //0=map, 1=edge
            this.SetStrProperty("char_neuralNetworkPath", "./resources/neuralnetworks/network_avgres_813_map.xml");
            this.SetStrProperty("char_learnAlphabetPath", "./resources/alphabets/alphabet_8x13");
            this.SetIntProperty("intelligence_public classification_method", 0); // 0 = pattern match ,1=nn

            // PLATEGRAPH
            this.SetDoubleProperty("plategraph_peakfootconstant", 0.7); // urci sirku detekovanej medzery
            this.SetDoubleProperty("plategraph_rel_minpeaksize", 0.86); // 0.85 // mensie cislo seka znaky, vacsie zase nespravne zdruzuje

            // PLATEGRAPHHORIZONTALGRAPH
            this.SetDoubleProperty("platehorizontalgraph_peakfootconstant", 0.05);
            this.SetIntProperty("platehorizontalgraph_detectionType", 1); // 1=edgedetection 0=magnitudederivate

            // PLATEVERICALGRAPH
            this.SetDoubleProperty("plateverticalgraph_peakfootconstant", 0.42);

            // INTELLIGENCE
            this.SetIntProperty("intelligence_numberOfBands", 3);
            this.SetIntProperty("intelligence_numberOfPlates", 3);
            this.SetIntProperty("intelligence_numberOfChars", 20);

            this.SetIntProperty("intelligence_minimumChars", 5);
            this.SetIntProperty("intelligence_maximumChars", 15);

            // plate heuristics
            this.SetDoubleProperty("intelligence_maxCharWidthDispersion", 0.5); // in plate
            this.SetDoubleProperty("intelligence_minPlateWidthHeightRatio", 0.5);
            this.SetDoubleProperty("intelligence_maxPlateWidthHeightRatio", 15.0);

            // char heuristics
            this.SetDoubleProperty("intelligence_minCharWidthHeightRatio", 0.1);
            this.SetDoubleProperty("intelligence_maxCharWidthHeightRatio", 0.92);
            this.SetDoubleProperty("intelligence_maxBrightnessCostDispersion", 0.161);
            this.SetDoubleProperty("intelligence_maxContrastCostDispersion", 0.1);
            this.SetDoubleProperty("intelligence_maxHueCostDispersion", 0.145);
            this.SetDoubleProperty("intelligence_maxSaturationCostDispersion", 0.24); //0.15
            this.SetDoubleProperty("intelligence_maxHeightCostDispersion", 0.2);
            this.SetDoubleProperty("intelligence_maxSimilarityCostDispersion", 100);

            // RECOGNITION
            this.SetIntProperty("intelligence_syntaxanalysis", 2);
            this.SetStrProperty("intelligence_syntaxDescriptionFile", "./resources/syntax/syntax.xml");

            // NEURAL NETWORK
            //int maxK, double eps, double lambda, double micro

            this.SetIntProperty("neural_maxk", 8000); // maximum K - maximalny pocet iteracii
            this.SetDoubleProperty("neural_eps", 0.07); // epsilon - pozadovana presnost
            this.SetDoubleProperty("neural_lambda", 0.05); // lambda factor - rychlost ucenia, velkost gradientu
            this.SetDoubleProperty("neural_micro", 0.5); // micro - momentovy clen pre prekonavanie lokalnych extremov
            // top(log(m recognized units)) = 6
            this.SetIntProperty("neural_topology", 20); // topologia strednej vrstvy

            /* ***** END ***** Definition of property defaults  ******* */

            this.SetStrProperty("help_file_help", "./resources/help/help.html");
            this.SetStrProperty("help_file_about", "./resources/help/about.html");
            this.SetStrProperty("reportgeneratorcss", "./resources/reportgenerator/style.css");
        }

        public Configurator(string path)
        {
            new Configurator();
            try
            {
                loadConfiguration(path);
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Couldn't load configuration file " + path);
                Application.Exit(new CancelEventArgs(true));
            }
        }

        public string ConfigurationFileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        public string GetStrProperty(string name)
        {
            return list.GetProperty(name).ToString();
        }

        public string GetPathProperty(string name)
        {
            return this.GetStrProperty(name).Replace('/', Path.DirectorySeparatorChar);

        }

        public void SetStrProperty(string name, string value)
        {
            list.SetProperty(name, value);
        }

        public int GetIntProperty(string name)
        {
            return Int32.Parse(list.GetProperty(name));
        }

        public void SetIntProperty(string name, int value)
        {
            list.SetProperty(name, value.ToString());
        }

        public double GetDoubleProperty(string name)
        {
            return Double.Parse(list.GetProperty(name));
        }

        public void SetDoubleProperty(string name, double value)
        {
            list.SetProperty(name, value.ToString());
        }

        public Color GetColorProperty(string name)
        {
            return Color.FromArgb(Int32.Parse(list.GetProperty(name)));
        }

        public void SetColorProperty(string name, Color value)
        {
            list.SetProperty(name, value.ToArgb().ToString());
        }

        public void SaveConfiguration()
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            list.StoreToXML(fs, comment);
        }

        public void SaveConfiguration(string arg_file)
        {
            FileStream os = new FileStream(arg_file, FileMode.OpenOrCreate);
            list.StoreToXML(os, comment);
        }

        public void LoadConfiguration()
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            list.LoadFromXML(fs);
        }

        public void loadConfiguration(string arg_file)
        {
            FileStream fs = new FileStream(arg_file, FileMode.OpenOrCreate);
            list.LoadFromXML(fs);
        }

    }
}
