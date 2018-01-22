using System;
using System.IO;

namespace Tools
{
    public class TestStatistics
    {
        public static String HelpText = "" +
                                        "-----------------------------------------------------------\n"+
                                        "ANPR Statistics Generator\n"+
                                        "\n"+
                                        "Command line arguments\n"+
                                        "\n"+
                                        "    -help         Displays this help\n"+
                                        "    -i <file>     Create statistics for test file\n"+
                                        "\n"+
                                        "Test file must be have a CSV format\n"+
                                        "Each row must contain name of analysed snapshot,\n" +
                                        "real plate and recognized plate string\n" +
                                        "Example : \n"+
                                        "001.jpg, 1B01234, 1B012??";
        public static void Main(string[] args)
        {
            if (args.Length == 2 && args[0].Equals("-i")
            )
            {
                // proceed analysis
                try
                {
                    StreamReader input = new StreamReader(args[1]);
                    string line;
                    int lineCount = 0;
                    string[] split;
                    TestReport testReport = new TestReport();
                    while ((line = input.ReadLine()) != null)
                    {
                        lineCount++;
                        split = line.Split(',');
                        if (split.Length != 3)
                        {
                            Console.WriteLine("Warning: line " + lineCount + " contains invalid CSV data (skipping)");
                            continue;
                        }
                        testReport.AddRecord(new TestReport.TestRecord(split[0], split[1], split[2]))
                            ;
                    }
                    input.Close();
                    testReport.PrintStatistics();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine(HelpText);
            }
        }
    }
}