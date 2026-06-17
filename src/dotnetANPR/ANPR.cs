using System;
using System.IO;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;
using SkiaSharp;

namespace DotNetANPR;

/// <summary>
/// Public API facade for the Automatic Number Plate Recognition system.
/// Provides methods for recognizing license plates from files, streams, or bitmaps,
/// as well as utilities for configuration export, network training, and alphabet normalization.
/// </summary>
public class ANPR
{
    /// <summary>
    /// Recognizes a license plate from an image file at the specified path.
    /// </summary>
    /// <param name="imagePath">The path to the image file.</param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    /// <exception cref="ArgumentException">Thrown when the image path is invalid.</exception>
    public static string? Recognize(string imagePath)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        using var stream = File.OpenRead(imagePath);
        var bitmap = SKBitmap.Decode(stream);
        return Recognize(bitmap);
    }

    /// <summary>
    /// Recognizes a license plate from an image provided as a <see cref="Stream"/>.
    /// </summary>
    /// <param name="imageStream">The stream containing the image data.</param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    public static string? Recognize(Stream imageStream)
    {
        var bitmap = SKBitmap.Decode(imageStream);
        return Recognize(bitmap);
    }

    /// <summary>
    /// Recognizes a license plate from an <see cref="SKBitmap"/>.
    /// </summary>
    /// <param name="image">The bitmap image to analyze.</param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    public static string? Recognize(SKBitmap image)
    {
        var intelligence = new Intelligence.Intelligence();
        return intelligence.Recognize(new CarSnapshot(image));
    }

    /// <summary>
    /// Exports the default configuration as a JSON file.
    /// </summary>
    /// <param name="outputFilePath">The path to the file where the configuration will be exported.</param>
    public static void ExportDefaultConfig(string outputFilePath)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            AnprConfig.Instance,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputFilePath, json);
    }

    /// <summary>
    /// Trains a new neural network on the configured alphabet images and exports
    /// the trained network to an XML file.
    /// </summary>
    /// <param name="learnPath">The path to the alphabet training images directory.</param>
    /// <param name="outputFilePath">The file path where the trained network XML will be saved.</param>
    /// <exception cref="IOException">Thrown when the destination file already exists.</exception>
    public static void TrainNetworkAndExport(string learnPath, string outputFilePath)
    {
        if (File.Exists(outputFilePath))
            throw new IOException("Destination file already exists.");

        AnprConfig.Instance.Character.LearnAlphabetPath = learnPath;
        var npc = new NeuralPatternClassifier(true);
        npc.NeuralNetwork.SaveToXml(outputFilePath);
    }

    /// <summary>
    /// Normalizes alphabet images from the source directory and saves them to the destination directory.
    /// Resizes each character image to the configured normalized dimensions.
    /// </summary>
    /// <param name="sourceDirectoryPath">The path to the source directory containing alphabet images.</param>
    /// <param name="destinationDirectoryPath">The path to the destination directory for normalized images.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the source directory does not exist or is empty.
    /// </exception>
    public static void NormalizeAlphabets(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        if (!Directory.Exists(sourceDirectoryPath))
            throw new ArgumentException("Source directory does not exist.");

        if (Directory.GetFiles(sourceDirectoryPath).Length == 0)
            throw new ArgumentException("Source directory is empty.");

        if (!Directory.Exists(destinationDirectoryPath))
            Directory.CreateDirectory(destinationDirectoryPath);

        foreach (var fileName in Character.AlphabetList(sourceDirectoryPath))
        {
            using var character = new Character(fileName);
            character.Normalize();
            character.Save(Path.Combine(destinationDirectoryPath, Path.GetFileName(fileName)));
        }
    }
}
