using System;
using System.IO;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using DotNetANPR.Recognizer;
using DotNetANPR.Utilities;
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
    /// <param name="reportPath">
    /// An optional directory path for generating an HTML diagnostic report.
    /// Pass <c>null</c> to skip report generation.
    /// </param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    /// <exception cref="ArgumentException">Thrown when the image path is invalid.</exception>
    public static string? Recognize(string imagePath, string? reportPath = null)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        using var stream = File.OpenRead(imagePath);
        var bitmap = SKBitmap.Decode(stream);
        return Recognize(bitmap, reportPath);
    }

    /// <summary>
    /// Recognizes a license plate from an image provided as a <see cref="Stream"/>.
    /// </summary>
    /// <param name="imageStream">The stream containing the image data.</param>
    /// <param name="reportPath">
    /// An optional directory path for generating an HTML diagnostic report.
    /// Pass <c>null</c> to skip report generation.
    /// </param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    public static string? Recognize(Stream imageStream, string? reportPath = null)
    {
        var bitmap = SKBitmap.Decode(imageStream);
        return Recognize(bitmap, reportPath);
    }

    /// <summary>
    /// Recognizes a license plate from an <see cref="SKBitmap"/>.
    /// </summary>
    /// <param name="image">The bitmap image to analyze.</param>
    /// <param name="reportPath">
    /// An optional directory path for generating an HTML diagnostic report.
    /// Pass <c>null</c> to skip report generation.
    /// </param>
    /// <returns>The recognized plate text, or <c>null</c> if no plate was found.</returns>
    /// <exception cref="ArgumentException">Thrown when the report path is invalid.</exception>
    public static string? Recognize(SKBitmap image, string? reportPath = null)
    {
        if (reportPath is not null && string.IsNullOrWhiteSpace(reportPath))
            throw new ArgumentException("Invalid report path: " + reportPath, nameof(reportPath));

        var reportGenerator = reportPath == null ? null : new ReportGenerator(reportPath);
        var intelligence = new Intelligence.Intelligence();
        return intelligence.Recognize(new CarSnapshot(image), reportGenerator);
    }

    /// <summary>
    /// Exports the default configuration to a file.
    /// </summary>
    /// <param name="outputFilePath">The path to the file where the configuration will be exported.</param>
    public static void ExportDefaultConfig(string outputFilePath) =>
        Configurator.Instance.SaveToXml(outputFilePath);

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

        // Temporarily set the learn path in configuration, then train and export
        Configurator.Instance.Set("char_learnAlphabetPath", learnPath);
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

        NewAlphabet(sourceDirectoryPath, destinationDirectoryPath);
    }

    #region Private Helpers

    private static void NewAlphabet(string srcDir, string dstDir)
    {
        foreach (var fileName in Character.AlphabetList(srcDir))
        {
            using var character = new Character(fileName);
            character.Normalize();
            character.Save(Path.Combine(dstDir, Path.GetFileName(fileName)));
        }
    }

    #endregion
}
