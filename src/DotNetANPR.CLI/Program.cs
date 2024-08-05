// See https://aka.ms/new-console-template for more information

using DotNetANPR;

const string sourceFilePath = "";
var result = ANPR.Recognize(sourceFilePath);

Console.WriteLine("The result is: " + result);
