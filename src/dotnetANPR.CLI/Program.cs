using System;
using System.IO;
using Microsoft.Extensions.Logging;
using dotnetANPR;

string? imagePath = null;
string? dumpDir = null;
string? configPath = null;
bool enableSkew = false;

var cliArgs = Environment.GetCommandLineArgs()[1..];

for (var i = 0; i < cliArgs.Length; i++)
{
    switch (cliArgs[i])
    {
        case "--dump-stages":
            if (i + 1 >= cliArgs.Length)
            {
                Console.Error.WriteLine("Error: --dump-stages requires a directory path.");
                Environment.Exit(1);
            }
            dumpDir = cliArgs[++i];
            break;
        case "--config":
            if (i + 1 >= cliArgs.Length)
            {
                Console.Error.WriteLine("Error: --config requires a file path.");
                Environment.Exit(1);
            }
            configPath = cliArgs[++i];
            break;
        case "--skew":
            enableSkew = true;
            break;
        default:
            if (imagePath == null)
                imagePath = cliArgs[i];
            else
            {
                Console.Error.WriteLine($"Error: unexpected argument '{cliArgs[i]}'.");
                PrintUsage();
                Environment.Exit(1);
            }
            break;
    }
}

if (imagePath == null)
{
    Console.Error.WriteLine("Error: image path is required.");
    PrintUsage();
    Environment.Exit(1);
}

if (!File.Exists(imagePath))
{
    Console.Error.WriteLine($"Error: file not found: {imagePath}");
    Environment.Exit(1);
}

if (dumpDir != null)
    Console.WriteLine($"Dumping processing stages to: {Path.GetFullPath(dumpDir)}");

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var engine = new AnprEngine(new AnprOptions
{
    ConfigFilePath = configPath,
    LoggerFactory = loggerFactory,
});

var result = engine.Recognize(imagePath, new RecognitionOptions
{
    DumpStagesDirectory = dumpDir,
    EnableSkewCorrection = enableSkew,
});

if (result.Success)
    Console.WriteLine($"Recognised plate: {result.Text}");
else
    Console.WriteLine("No plate recognised.");

Console.WriteLine($"Confidence: {result.Confidence:F3}");
Console.WriteLine($"Duration: {result.Duration.TotalMilliseconds:F0} ms");

static void PrintUsage() =>
    Console.Error.WriteLine(
        "Usage: dotnetANPR.CLI <image-path> [--dump-stages <dir>] [--config <path>] [--skew]");
