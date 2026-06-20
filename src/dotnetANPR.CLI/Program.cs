using System;
using System.IO;
using dotnetANPR;

// Usage:
//   dotnetANPR.CLI <image-path> [--dump-stages <dir>]
//
// Options:
//   <image-path>          Path to the car image to analyse (required).
//   --dump-stages <dir>   Directory to write intermediate processing stage images into.
//                         Files are written as 01-vertical-rank-filter.jpg, 02-..., etc.

string? imagePath = null;
string? dumpDir = null;

var cliArgs = Environment.GetCommandLineArgs()[1..]; // skip executable name

for (var i = 0; i < cliArgs.Length; i++)
{
    if (cliArgs[i] == "--dump-stages")
    {
        if (i + 1 >= cliArgs.Length)
        {
            Console.Error.WriteLine("Error: --dump-stages requires a directory path.");
            Environment.Exit(1);
        }

        dumpDir = cliArgs[++i];
    }
    else if (imagePath == null)
    {
        imagePath = cliArgs[i];
    }
    else
    {
        Console.Error.WriteLine($"Error: unexpected argument '{cliArgs[i]}'.");
        PrintUsage();
        Environment.Exit(1);
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

var result = ANPR.Recognize(imagePath, dumpDir);

if (result != null)
    Console.WriteLine($"Recognised plate: {result}");
else
    Console.WriteLine("No plate recognised.");

static void PrintUsage() =>
    Console.Error.WriteLine("Usage: dotnetANPR.CLI <image-path> [--dump-stages <dir>]");
