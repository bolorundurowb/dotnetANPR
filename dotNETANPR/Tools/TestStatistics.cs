using System;
using System.Collections.Generic;
using System.IO;

namespace dotNETANPR.Tools
{
    public class TestStatistics
    {
        public static String helpText = "" +
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
                // DONE display help
                Console.WriteLine(helpText);
            }
        }
    }

    public class TestReport
    {
        public class TestRecord
        {
            public string Name { get; set; }
            public string Plate { get; set; }
            public string RecognizedPlate { get; set; }
            public int Good { get; private set; }
            public int Length { get; private set; }

            public bool IsOk
            {
                get { return Length == Good; }
            }

            public TestRecord(string name, string plate, string recognizedPlate)
            {
                this.Name = name.Trim();
                this.Plate = plate.Trim();
                this.RecognizedPlate = recognizedPlate.Trim();
                Compute();
            }

            private void Compute()
            {
                this.Length = Math.Max(Plate.Length, RecognizedPlate.Length);
                int g1 = 0;
                int g2 = 0;
                for (int i = 0; i < this.Length; i++)
                {
                    if (GetChar(Plate, i) == GetChar(RecognizedPlate, i))
                    {
                        g1++;
                    }
                }
                for (int i = 0; i < this.Length; i++)
                {
                    if (GetChar(Plate, this.Length - i - 1) == GetChar(RecognizedPlate, this.Length - i - 1))
                    {
                        g2++;
                    }
                }
                this.Good = Math.Max(g1, g2);
            }

            private char GetChar(string item, int position)
            {
                if (position >= item.Length)
                    return ' ';
                if (position < 0)
                    return ' ';
                return item[position];
            }
        }

        private List<TestRecord> records;

        public TestReport()
        {
            this.records = new List<TestRecord>();
        }

        public void AddRecord(TestRecord testRecord)
        {
           this.records.Add(testRecord);
        }

        public void PrintStatistics()
        {
            int weightedScoreCount = 0;
            int binaryScoreCount = 0;
            int characterCount = 0;

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Defective Plates\n");

            foreach (TestRecord record in this.records)
            {
                characterCount += record.Length;
                weightedScoreCount += record.Good;
                binaryScoreCount += (record.IsOk ? 1 : 0);
                if (!record.IsOk)
                {
                    Console.WriteLine(record.Plate + " ~ " + record.RecognizedPlate + " ( " +
                                      (float) record.Good / record.Length * 100 + "% ok");
                }
            }
            Console.WriteLine("\n------------------------------------------");
            Console.WriteLine("test Report Statistics\n");
            Console.WriteLine("Total Number of Plates    :" + this.records.Count);
            Console.WriteLine("Total Number of Characters:" + characterCount);
            Console.WriteLine("Binary Score              :" + (float) binaryScoreCount / this.records.Count * 100);
            Console.WriteLine("Weighted Score            :" + (float)weightedScoreCount/characterCount);
        }
    }
}
