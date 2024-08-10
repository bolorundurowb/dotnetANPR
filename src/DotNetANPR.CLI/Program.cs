// See https://aka.ms/new-console-template for more information

using DotNetANPR;

const string sourceFilePath = "/Users/bolorundurowb/Downloads/snapshots/test_093.jpg";
const string reportPath = "/Users/bolorundurowb/Downloads/report";
var result = ANPR.Recognize(sourceFilePath);

Console.WriteLine("The result is: " + result);
