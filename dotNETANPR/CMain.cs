/*
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using dotNETANPR.GUI;
using dotNETANPR.ImageAnalysis;
using System.IO;
using dotNETANPR.Recognizer;

namespace dotNETANPR
{
    class CMain
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


        public static void NewAlphabet(string srcdir, String dstdir)
        {
            Configurator.Configurator configurator = new Configurator.Configurator();
            string[] folder = Directory.GetFiles(srcdir);
            if (!Directory.Exists(srcdir)) throw new IOException("Source folder doesn't exists");
            if (!Directory.Exists(dstdir)) throw new IOException("Destination folder doesn't exists");
            int x = configurator.GetIntProperty("char_normalizeddimensions_x");
            int y = configurator.GetIntProperty("char_normalizeddimensions_y");
            Console.WriteLine("\nCreating new alphabet (" + x + " x " + y + " px)... \n");
            foreach (String fileName in folder)
            {
                ImageAnalysis.Char c = new ImageAnalysis.Char(srcdir + Path.DirectorySeparatorChar + fileName);
                c.Normalize();
                c.SaveImage(dstdir + Path.DirectorySeparatorChar + fileName);
                Console.WriteLine(fileName + " done");
            }
        }


        public static void LearnAlphabet(String destinationFile)
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
            npc.Network.SaveToXml(destinationFile);
        }

        public static void Main(string[] args)
        {

            if (args.Length == 0 || (args.Length == 1 && args[0].Equals("-gui")))
            {
                systemLogic = new Intelligence.Intelligence(false);
            }
            else if (args.Length == 3 &&
                    args[0].Equals("-recognize") &&
                    args[1].Equals("-i")
                    )
            {
                try
                {
                    systemLogic = new Intelligence.Intelligence(false);
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
                try
                {
                    rg = new ReportGenerator(args[4]);    
                    systemLogic = new Intelligence.Intelligence(true); 
                    systemLogic.recognize(new CarSnapshot(args[2]));
                    rg.Finish();
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
