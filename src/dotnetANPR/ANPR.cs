using System;
using System.Drawing;
using System.IO;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Recognizer;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace dotnetANPR;

public class ANPR
{
    private static readonly ILogger<ANPR> Logger = Logging.GetLogger<ANPR>();

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

    /// <summary>
    /// Recognises the licence plate in <paramref name="imagePath"/>.
    /// </summary>
    /// <param name="imagePath">Path to the source image.</param>
    /// <param name="dumpDir">
    /// Optional directory to dump intermediate processing stages as sequentially-numbered JPEGs.
    /// The directory is created if it does not exist.
    /// </param>
    public static string? Recognize(string imagePath, string? dumpDir = null)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        return Recognize(new Bitmap(imagePath), dumpDir);
    }

    /// <inheritdoc cref="Recognize(string, string?)"/>
    public static string? Recognize(Stream imageStream, string? dumpDir = null) =>
        Recognize(new Bitmap(imageStream), dumpDir);

    /// <inheritdoc cref="Recognize(string, string?)"/>
    public static string? Recognize(Bitmap image, string? dumpDir = null)
    {
        if (dumpDir is not null && string.IsNullOrWhiteSpace(dumpDir))
            throw new ArgumentException("Invalid dump directory: " + dumpDir, nameof(dumpDir));

        var stageWriter = dumpDir == null ? null : new StageWriter(dumpDir);
        var intelligence = new Intelligence.Intelligence();
        return intelligence.Recognize(new CarSnapshot(image), stageWriter);
    }

    /// <summary>
    /// Exports the default configuration to a file.
    /// </summary>
    /// <param name="outputFilePath">The path to the file where the configuration will be exported.</param>
    public static void ExportDefaultConfig(string outputFilePath) => Configurator.Instance.Save(outputFilePath);

    public static void TrainNetworkAndExport(string outputFilePath)
    {
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
