using System;
using System.IO;
using dotNETANPR.Gui;
using dotNETANPR.ImageAnalysis;
using dotNETANPR.Recognizer;

namespace dotNETANPR
{
    public class Program
    {
        public static ReportGenerator ReportGenerator = new ReportGenerator();
        public static Intelligence.Intelligence systemLogic;

        public static string helpText = "" +
                                        "-----------------------------------------------------------\n" +
                                        "Automatic Number Plate recognition System\n" +
                                        "Copyright (c) Bolorunduro Winner-Timothy, 2014-2017\n" +
                                        "Based on the work of Ondrej Martinsky\n" +
                                        "\n" +
                                        "Licensed under the GNU GPLv3,\n" +
                                        "\n" +
                                        "Usage : mono dotNETANPR.exe [-options]\n" +
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

        public static void NewAlphabet(string source, string dest)
        {
            if (!Directory.Exists(source))
            {
                throw new IOException("Source folder doesn't exist");
            }
            if (!Directory.Exists(dest))
            {
                throw new IOException("Destination folder doesn't exist");
            }
            int x = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_x");
            int y = Intelligence.Intelligence.Configurator.GetIntProperty("char_normalizeddimensions_y");
            Console.WriteLine($"\nCreating new alphabet ({x} x {y} px)... \n");
            foreach (string file in Directory.EnumerateFiles(source))
            {
                Character c = new Character(source + Path.DirectorySeparatorChar + file);
                c.Normalize();
                c.SaveImage(dest + Path.DirectorySeparatorChar + file);
                Console.WriteLine(file + " done.");
            }
        }

        public static void LearnAlphabet(string dest)
        {
            try
            {
                File.Create(dest);
            }
            catch (Exception e)
            {
                throw new IOException("Cannot find the path specified");
            }
            Console.WriteLine();
            NeuralPatternClassificator npc = new NeuralPatternClassificator(true);
            npc.Network.SaveToXml(dest);
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length == 1 && args[0].Equals("-gui"))
            {
                // TODO: run GUI
            }
            else if (args.Length == 3 &&
                     args[0].Equals("-recognize") &&
                     args[1].Equals("-i")
            )
            {
                try
                {
                    systemLogic = new Intelligence.Intelligence(false);
                    Console.WriteLine(systemLogic.Recognize(new CarSnapshot(args[2])));
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
                try
                {
                    ReportGenerator = new ReportGenerator(args[4]);
                    systemLogic = new Intelligence.Intelligence(true);
                    systemLogic.Recognize(new CarSnapshot(args[2]));
                    ReportGenerator.Finish();
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
                try
                {
                    LearnAlphabet(args[2]);
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
                try
                {
                    NewAlphabet(args[2], args[4]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine(helpText);
            }
        }
    }
}