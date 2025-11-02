// See https://aka.ms/new-console-template for more information

using DotNetANPR;

const string sourceFilePath = "";
var anprService = new AnprService();
var result = anprService.Recognize(sourceFilePath);

Console.WriteLine("The result is: " + result?.Text + " with confidence " + result?.Confidence);
