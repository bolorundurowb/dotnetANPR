using System;
using DotNetANPR.Config;
using DotNetANPR.Intelligence;
using DotNetANPR.Recognizer;
using DotNetANPR.ImageAnalysis;
using SkiaSharp;
using System.IO;

namespace DotNetANPR;

/// <summary>
/// Main public entry point for the ANPR library.
/// Manages configuration, services, and the recognition pipeline.
/// </summary>
public class AnprService
{
    private readonly AppSettings _settings;
    private readonly AnprEngine _engine;

    public AnprService(string configPath = "./resources/config.json")
    {
        var configService = new ConfigurationService(configPath);
        _settings = configService.Settings;

        // Select the recognition method based on config
        ICharacterRecognizer recognizer = _settings.Recognition.ClassificationMethod switch
        {
            0 => new KnnPatternClassificator(_settings),
            1 => new NeuralPatternClassificator(_settings),
            _ => throw new ArgumentException("Invalid 'classificationMethod' in config.json.")
        };

        var parser = new SyntaxParser(_settings);
        _engine = new AnprEngine(_settings, recognizer, parser);
    }

    /// <summary>
    /// Recognizes a license plate from an image file.
    /// </summary>
    /// <param name="imagePath">The absolute or relative path to the image file.</param>
    /// <returns>The best recognized plate, or null if no plate is found.</returns>
    public RecognizedPlate? Recognize(string imagePath)
    {
        using var snapshot = new CarSnapshot(imagePath, _settings);
        return _engine.Recognize(snapshot);
    }

    /// <summary>
    /// Recognizes a license plate from an image stream.
    /// </summary>
    /// <param name="imageStream">The stream containing the image data (e.g., from a file, memory, or network).</param>
    /// <returns>The best recognized plate, or null if no plate is found.</returns>
    public RecognizedPlate? Recognize(Stream imageStream)
    {
        using var bitmap = SKBitmap.Decode(imageStream);
        if (bitmap == null)
        {
            throw new IOException("Failed to decode image from stream.");
        }
        using var snapshot = new CarSnapshot(bitmap, _settings);
        return _engine.Recognize(snapshot);
    }
}