using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Tools
{
    public class TestStatistics
    {
        public static String helpText = "" +
            "-----------------------------------------------------------\n" +
            "dotNETANPR Statistics Generator\n" +
            "Modified by Bolorunduro Winner-Timothy for .NET\n" +
            "Copyright (c) Ondrej Martinsky, 2006-2007 (javaanpr)\n" +
            "\n" +
            "Licensed under the Educational Community License,\n" +
            "\n" +
            "Command line arguments\n" +
            "\n" +
            "    -help         Displays this help\n" +
            "    -i <file>     Create statistics for test file\n" +
            "\n" +
            "Test file must be have a CSV format\n" +
            "Each row must contain name of analysed snapshot,\n" +
            "real plate and recognized plate string\n" +
            "Example : \n" +
            "001.jpg, 1B01234, 1B012??";

        public TestStatistics()
        {

        }

        public static void Main(String[] args)
        {
            if (args.Length == 2 && args[0].Equals("-i"))
            { // proceed analysis
                try
                {
                    StreamReader input = new StreamReader(args[1]);
                    String line;
                    int lineCount = 0;
                    String[] split;
                    TestReport testReport = new TestReport();
                    while ((line = input.ReadLine()) != null)
                    {
                        lineCount++;
                        split = line.Split(new string[] { "," }, 4, StringSplitOptions.None);
                        if (split.Length != 3)
                        {
                            Console.WriteLine("Warning: line " + lineCount + " contains invalid CSV data (skipping)");
                            continue;
                        }
                        testReport.addRecord(new TestReport.TestRecord(split[0], split[1], split[2]));
                    }
                    input.Close();
                    testReport.printStatistics();
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.StackTrace + Environment.NewLine + e.Message);
                }
            }
            else
            {

                // DONE display help
                Console.WriteLine(helpText);
            }

        }
    }

    public class TestReport
    {
        public  class TestRecord
        {
            public String name, plate, recognizedPlate;
            int good;
            int Length;

            public TestRecord(String name, String plate, String recognizedPlate)
            {
                this.name = name.Trim();
                this.plate = plate.Trim();
                this.recognizedPlate = recognizedPlate.Trim();
                compute();
            }

            private void compute()
            {
                this.Length = Math.Max(plate.Length, recognizedPlate.Length);
                int g1 = 0;
                int g2 = 0;
                for (int i = 0; i < this.Length; i++)
                { // POROVNAVAT ODPREDU (napr. BA123AB vs. BA123ABX)
                    if (getChar(plate, i) == getChar(recognizedPlate, i)) g1++;
                }
                for (int i = 0; i < this.Length; i++)
                { // POROVNAVAT ODZADU (napr. BA123AB vs. XBA123AB)
                    if (getChar(plate, this.Length - i - 1) == getChar(recognizedPlate, this.Length - i - 1)) g2++;
                }
                this.good = Math.Max(g1, g2);
            }

            private char getChar(String strin, int position)
            {
                if (position >= strin.Length) return ' ';
                if (position < 0) return ' ';
                return strin[position];
            }

            public int getGoodCount
            {
                get
                {
                    return this.good;
                }
            }
            public int getLength
            {
                get
                {
                    return this.Length;
                }
            }
            public bool isOk
            {
                get
                {
                    if (this.Length != this.good) return false; else return true;
                }
            }
        }


        List<TestRecord> records;


        public void addRecord(TestRecord testRecord)
        {
            this.records.Add(testRecord);
        }

        public void printStatistics()
        {
            int weightedScoreCount = 0;
            int binaryScoreCount = 0;
            int characterCount = 0;
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("Defective plates\n");

            foreach (TestRecord record in this.records)
            {
                characterCount += record.getLength;
                weightedScoreCount += record.getGoodCount;
                binaryScoreCount += (record.isOk ? 1 : 0);
                if (!record.isOk)
                {
                    Console.WriteLine(record.plate + " ~ " + record.recognizedPlate + " (" + (float)record.getGoodCount / record.getLength * 100 + "% ok)");
                }
            }
            Console.WriteLine("\n----------------------------------------------");
            Console.WriteLine("Test report statistics\n");
            Console.WriteLine("Total number of plates     : " + this.records.Count);
            Console.WriteLine("Total number of characters : " + characterCount);
            Console.WriteLine("Binary score               : " + (float)binaryScoreCount / this.records.Count * 100);
            Console.WriteLine("Weighted score             : " + (float)weightedScoreCount / characterCount * 100);
        }



        public TestReport()
        {
            this.records = new List<TestRecord>();
        }
    }
}
