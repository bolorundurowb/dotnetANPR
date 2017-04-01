using dotNETANPR.Gui;

namespace dotNETANPR
{
    public class Program
    {
        public static ReportGenerator ReportGenerator = new ReportGenerator();
        public static Intelligence.Intelligence systemLogic;

        public static string helpText = "" +
                                        "-----------------------------------------------------------\n" +
                                        "Automatic number plate recognition system\n" +
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
    }
}