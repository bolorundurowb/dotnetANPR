using System;
using System.Drawing;
using System.IO;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace DotNetANPR;

public class ANPR
{
    private static readonly ILogger<ANPR> Logger =
        LoggerFactory.Create(_ => { }).CreateLogger<ANPR>();

    private static void NewAlphabet(string srcDir, string dstDir)
    {
        var x = Configurator.Instance.Get<int>("char_normalizeddimensions_x");
        var y = Configurator.Instance.Get<int>("char_normalizeddimensions_y");
        Logger.LogInformation("Creating new alphabet (" + x + " x " + y + " px)... \n");

        foreach (var fileName in Character.AlphabetList(srcDir))
        {
            using var character = new Character(fileName);
            character.Normalize();
            character.Save(Path.Combine(dstDir, fileName));

            Logger.LogInformation(fileName + "{name} done", "ARG0");
        }
    }

    public static string? Recognize(string imagePath, string? reportPath = null)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        return Recognize(new Bitmap(imagePath), reportPath);
    }

    public static string? Recognize(Stream imageStream, string? reportPath = null) =>
        Recognize(new Bitmap(imageStream), reportPath);

    public static string? Recognize(Bitmap image, string? reportPath = null)
    {
        if (reportPath is not null && string.IsNullOrWhiteSpace(reportPath))
            throw new ArgumentException("Invalid report path: " + reportPath, nameof(reportPath));

        // load snapshot arg[2] and generate report into arg[4]
        var reportGenerator = reportPath == null ? null : new ReportGenerator(reportPath);
        var intelligence = new Intelligence.Intelligence();
        return intelligence.Recognize(new CarSnapshot(image), reportGenerator);
    }

    /// <summary>
    /// Exports the default configuration to a file.
    /// </summary>
    /// <param name="outputFilePath">The path to the file where the configuration will be exported.</param>
    public static void ExportDefaultConfig(string outputFilePath) => Configurator.Instance.Save(outputFilePath);

    public static void TrainNetworkAndExport(string outputFilePath)
    {
        // learn new neural network and save it into args[2]
        if (File.Exists(outputFilePath))
            throw new IOException("Destination file already exists.");

        var npc = new NeuralPatternClassifier(true);
        npc.NeuralNetwork.SaveToXml(outputFilePath);
    }

    public static void NormalizeAlphabets(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        if (!Directory.Exists(sourceDirectoryPath))
            throw new ArgumentException("Source directory does not exist.");

        if (Directory.GetFiles(sourceDirectoryPath).Length == 0)
            throw new ArgumentException("Source directory is empty.");

        if (!Directory.Exists(destinationDirectoryPath))
            Directory.CreateDirectory(destinationDirectoryPath);

        NewAlphabet(sourceDirectoryPath, destinationDirectoryPath);
    }
}
