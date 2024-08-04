// See https://aka.ms/new-console-template for more information

using DotNetANPR;

const string sourceFilePath = "C:\\Users\\bolorundurowb\\Downloads\\snapshots\\test_089.jpg";
const string reportPath = "C:\\Users\\bolorundurowb\\Downloads\\anpr_report";
var result = ANPR.Recognize(sourceFilePath, reportPath);
// var result = ANPR.Recognize(sourceFilePath);

Console.WriteLine("The result is: " + result);
