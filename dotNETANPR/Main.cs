using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotNETANPR.GUI;
using dotNETANPR.ImageAnalysis;
using System.IO;
using dotNETANPR.Recognizer;
using dotNETANPR.Intelligence;

namespace dotNETANPR
{
    public class Main
    {
        public static ReportGenerator rg = new ReportGenerator();
        public static Intelligence.Intelligence systemLogic;
        public static String helpText = "" +
                "-----------------------------------------------------------\n" +
                "Automatic number plate recognition system\n" +
                "Modified for .NET by Bolorunduro Winner-Timothy" +
                "Copyright (c) Ondrej Martinsky, 2006-2007\n" +
                "\n" +
                "Licensed under the Educational Community License,\n" +
                "\n" +
                "Usage : dotNETANPR [-options]\n" +
                "\n" +
                "Where options include:\n" +
                "\n" +
                "    -help         Displays this help\n" +
                "    -gui          Run GUI viewer (default choice)\n" +
                "    -recognize -i <snapshot>\n" +
                "                  Recognize single snapshot\n" +
                "    -recognize -i <snapshot> -o <dstdir>\n" +
                "                  Recognize single snapshot and\n" +
                "                  save report html into specified\n" +
                "                  directory\n" +
                "    -newconfig -o <file>\n" +
                "                  Generate default configuration file\n" +
                "    -newnetwork -o <file>\n" +
                "                  Train neural network according to\n" +
                "                  specified feature extraction method and\n" +
                "                  learning parameters (in config. file)\n" +
                "                  and saves it into output file\n" +
                "    -newalphabet -i <srcdir> -o <dstdir>\n" +
                "                  Normalize all images in <srcdir> and save\n" +
                "                  it to <dstdir>.";


        // normalizuje abecedu v zdrojovom adresari a vysledok ulozi do cieloveho adresara
        public static void newAlphabet(String srcdir, String dstdir)
        { // NOT USED
            Configurator.Configurator configurator = new Configurator.Configurator();
            string[] folder = Directory.GetFiles(srcdir);
            if (!Directory.Exists(srcdir)) throw new IOException("Source folder doesn't exists");
            if (!Directory.Exists(dstdir)) throw new IOException("Destination folder doesn't exists");
            int x = configurator.getIntProperty("char_normalizeddimensions_x");
            int y = configurator.getIntProperty("char_normalizeddimensions_y");
            Console.WriteLine("\nCreating new alphabet (" + x + " x " + y + " px)... \n");
            foreach (String fileName in folder)
            {
                ImageAnalysis.Char c = new ImageAnalysis.Char(srcdir + Path.DirectorySeparatorChar + fileName);
                c.normalize();
                c.saveImage(dstdir + Path.DirectorySeparatorChar + fileName);
                Console.WriteLine(fileName + " done");
            }
        }

        // DONE z danej abecedy precita deskriptory, tie sa nauci, a ulozi neuronovu siet
        public static void learnAlphabet(String destinationFile)
        {
            try
            {
                File.Create(destinationFile);
            }
            catch (Exception e)
            {
                throw new IOException("Can't find the path specified");
            }
            Console.WriteLine();
            NeuralPatternClassificator npc = new NeuralPatternClassificator(true);
            npc.network.saveToXml(destinationFile);
        }

        public static void Main(String[] args)
        {

            if (args.Length == 0 || (args.Length == 1 && args[0].Equals("-gui")))
            {
                // DONE run gui
                //UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
                //FrameComponentInit frameComponentInit = new FrameComponentInit(); // show wait
                dotNETANPR.Main.systemLogic = new Intelligence.Intelligence(false);
                //frameComponentInit.dispose(); // hide wait
                //FrameMain mainFrame = new FrameMain();
            }
            else if (args.Length == 3 &&
                    args[0].Equals("-recognize") &&
                    args[1].Equals("-i")
                    )
            {
                // DONE load snapshot args[2] and recognize it
                try
                {
                    dotNETANPR.Main.systemLogic = new Intelligence.Intelligence(false);
                    Console.WriteLine(systemLogic.recognize(new CarSnapshot(args[2])));
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (args.Length == 5 &&
                    args[0].Equals("-recognize") &&
                    args[1].Equals("-i") &&
                    args[3].Equals("-o")
                    )
            {
                // load snapshot arg[2] and generate report into arg[4]
                try
                {
                    dotNETANPR.Main.rg = new ReportGenerator(args[4]);     //prepare report generator
                    dotNETANPR.Main.systemLogic = new Intelligence(true); //prepare intelligence
                    dotNETANPR.Main.systemLogic.recognize(new CarSnapshot(args[2]));
                    dotNETANPR.Main.rg.Finish();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            else if (args.Length == 3 &&
                    args[0].Equals("-newconfig") &&
                    args[1].Equals("-o")
                    )
            {
                // DONE save default config into args[2]
                Configurator.Configurator configurator = new Configurator.Configurator();
                try
                {
                    configurator.SaveConfiguration(args[2]);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (args.Length == 3 &&
                    args[0].Equals("-newnetwork") &&
                    args[1].Equals("-o")
                    )
            {
                // DONE learn new neural network and save it into into args[2]
                try
                {
                    learnAlphabet(args[2]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (args.Length == 5 &&
                    args[0].Equals("-newalphabet") &&
                    args[1].Equals("-i") &&
                    args[3].Equals("-o")
                    )
            {
                // DONE transform alphabets from args[2] -> args[4]
                try
                {
                    newAlphabet(args[2], args[4]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                // DONE display help
                Console.WriteLine(helpText);
            }
        }
    }
}
