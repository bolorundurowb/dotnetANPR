using System;
using System.IO;
using SkiaSharp;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
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
    /// Recognises the licence plate in the specified image.
    /// </summary>
    /// <param name="imagePath">Path to the source image file.</param>
    /// <param name="dumpDir">
    /// Optional directory to dump intermediate processing stages as sequentially-numbered JPEGs.
    /// The directory is created if it does not exist.
    /// </param>
    /// <returns>The recognised plate text, or <c>null</c> if no plate was found.</returns>
    public static string? Recognize(string imagePath, string? dumpDir = null)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        return Recognize(SkiaSharpAdapter.LoadBitmap(imagePath), dumpDir);
    }

    /// <summary>
    /// Recognises the licence plate in the specified image stream.
    /// </summary>
    /// <inheritdoc cref="Recognize(string, string?)" path="/param[@name='dumpDir']"/>
    /// <inheritdoc cref="Recognize(string, string?)" path="/returns"/>
    public static string? Recognize(Stream imageStream, string? dumpDir = null)
    {
        using var skData = SKData.Create(imageStream);
        var image = SKBitmap.Decode(skData);
        return Recognize(image, dumpDir);
    }

    /// <summary>
    /// Recognises the licence plate in the specified <see cref="SKBitmap"/>.
    /// </summary>
    /// <inheritdoc cref="Recognize(string, string?)" path="/param[@name='dumpDir']"/>
    /// <inheritdoc cref="Recognize(string, string?)" path="/returns"/>
    public static string? Recognize(SKBitmap image, string? dumpDir = null)
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

    /// <summary>
    /// Trains a new neural network on the default alphabet and exports it to the specified file.
    /// </summary>
    /// <param name="outputFilePath">The file path to export the trained network to.</param>
    /// <exception cref="IOException">Thrown if the destination file already exists.</exception>
    public static void TrainNetworkAndExport(string outputFilePath)
    {
        if (File.Exists(outputFilePath))
            throw new IOException("Destination file already exists.");

        var npc = new NeuralPatternClassifier(true);
        npc.NeuralNetwork.SaveToXml(outputFilePath);
    }

    /// <summary>
    /// Normalises all character images in the source alphabet directory and saves them to the destination.
    /// </summary>
    /// <param name="sourceDirectoryPath">Directory containing the source alphabet images.</param>
    /// <param name="destinationDirectoryPath">Directory to write the normalised images to.</param>
    /// <exception cref="ArgumentException">Thrown if the source directory does not exist or is empty.</exception>
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
