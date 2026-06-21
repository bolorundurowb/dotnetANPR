using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using dotnetANPR.Configuration;
using dotnetANPR.Extensions;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Intelligence;
using dotnetANPR.Intelligence.Parser;
using dotnetANPR.Pipeline;
using dotnetANPR.Recognizer;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace dotnetANPR;

/// <summary>
/// Primary entry point for licence plate recognition.
/// </summary>
public sealed class AnprEngine
{
    private readonly AnprSettings _settings;
    private readonly Configurator _configurator;
    private readonly ResourceLocator _resourceLocator;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Lazy<ICharacterRecognizer> _recognizer;
    private readonly Lazy<Parser> _parser;
    private readonly Lazy<PlateRecognitionPipeline> _pipeline;

    public AnprEngine(AnprOptions options)
    {
        _resourceLocator = new ResourceLocator();
        _loggerFactory = options.LoggerFactory ?? Logging.Factory;
        Logging.Configure(_loggerFactory);

        _configurator = options.ConfigFilePath is not null
            ? new Configurator(_resourceLocator.Resolve(options.ConfigFilePath))
            : Configurator.CreateDefault(_resourceLocator);

        if (options.AlphabetPath is not null)
            _configurator.Set("char_learnAlphabetPath", options.AlphabetPath);
        if (options.SyntaxFilePath is not null)
            _configurator.Set("intelligence_syntaxDescriptionFile", options.SyntaxFilePath);
        if (options.NeuralNetworkPath is not null)
            _configurator.Set("char_neuralNetworkPath", options.NeuralNetworkPath);

        _settings = _configurator.CreateSettings(_resourceLocator);

        var logger = _loggerFactory.CreateLogger<AnprEngine>();
        _recognizer = new Lazy<ICharacterRecognizer>(() => CreateRecognizer(_settings, logger));
        _parser = new Lazy<Parser>(() => new Parser(_settings.IntelligenceSyntaxDescriptionFile, logger));
        _pipeline = new Lazy<PlateRecognitionPipeline>(() =>
            new PlateRecognitionPipeline(_recognizer.Value, new SyntaxCorrector(_parser.Value)));
    }

    /// <summary>
    /// Recognises a licence plate from a file path.
    /// </summary>
    public RecognitionResult Recognize(string imagePath, RecognitionOptions? options = null)
    {
        if (!File.Exists(imagePath))
            throw new ArgumentException("Invalid image path: " + imagePath, nameof(imagePath));

        using var bitmap = SkiaSharpAdapter.LoadBitmap(imagePath);
        var recognizeOptions = options ?? new RecognitionOptions();
        return Recognize(bitmap, new RecognitionOptions
        {
            DumpStagesDirectory = recognizeOptions.DumpStagesDirectory,
            EnableSkewCorrection = recognizeOptions.EnableSkewCorrection,
            OwnsInputImage = true,
            CancellationToken = recognizeOptions.CancellationToken,
            DumpSkewDiagnostics = recognizeOptions.DumpSkewDiagnostics,
        });
    }

    /// <summary>
    /// Recognises a licence plate from an image stream.
    /// </summary>
    public RecognitionResult Recognize(Stream imageStream, RecognitionOptions? options = null)
    {
        using var skData = SKData.Create(imageStream);
        var bitmap = SKBitmap.Decode(skData);
        var recognizeOptions = options ?? new RecognitionOptions();
        return Recognize(bitmap, new RecognitionOptions
        {
            DumpStagesDirectory = recognizeOptions.DumpStagesDirectory,
            EnableSkewCorrection = recognizeOptions.EnableSkewCorrection,
            OwnsInputImage = true,
            CancellationToken = recognizeOptions.CancellationToken,
            DumpSkewDiagnostics = recognizeOptions.DumpSkewDiagnostics,
        });
    }

    /// <summary>
    /// Recognises a licence plate from a <see cref="SKBitmap"/>.
    /// Caller retains ownership unless <see cref="RecognitionOptions.OwnsInputImage"/> is true.
    /// </summary>
    public RecognitionResult Recognize(SKBitmap image, RecognitionOptions? options = null)
    {
        options ??= new RecognitionOptions();
        var timeMeter = new TimeMeter();
        var stageWriter = options.DumpStagesDirectory is null
            ? null
            : new StageWriter(options.DumpStagesDirectory);

        var context = new PipelineContext(
            _settings,
            _loggerFactory.CreateLogger<PlateRecognitionPipeline>(),
            stageWriter,
            options.CancellationToken,
            options.EnableSkewCorrection,
            options.DumpSkewDiagnostics);

        using var snapshot = new CarSnapshot(image, context);
        var (best, all) = _pipeline.Value.Process(snapshot, context);
        var duration = TimeSpan.FromMilliseconds(timeMeter.GetTime());

        if (options.OwnsInputImage)
            image.Dispose();

        if (best is null)
            return new RecognitionResult
            {
                Text = null,
                Confidence = 0,
                Duration = duration,
                Candidates = BuildCandidateDiagnostics(all, null),
            };

        var syntaxMode = (SyntaxAnalysisMode)_settings.IntelligenceSyntaxAnalysis;
        var text = new SyntaxCorrector(_parser.Value).Correct(best.RecognizedPlate, syntaxMode);
        best.Score = new PlateScorer().Score(best, _settings);

        return new RecognitionResult
        {
            Text = text,
            Confidence = best.Score,
            Duration = duration,
            Candidates = BuildCandidateDiagnostics(all, best, text),
        };
    }

    public Task<RecognitionResult> RecognizeAsync(
        string imagePath,
        RecognitionOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Recognize(imagePath, MergeCancellation(options, cancellationToken)), cancellationToken);

    public Task<RecognitionResult> RecognizeAsync(
        Stream imageStream,
        RecognitionOptions? options = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Recognize(imageStream, MergeCancellation(options, cancellationToken)), cancellationToken);

    public void ExportDefaultConfig(string outputFilePath)
    {
        new Configurator().Save(outputFilePath);
    }

    public void TrainNetworkAndExport(string outputFilePath)
    {
        if (File.Exists(outputFilePath))
            throw new IOException("Destination file already exists.");

        var logger = _loggerFactory.CreateLogger<NeuralPatternClassifier>();
        var classifier = new NeuralPatternClassifier(_settings, logger, true);
        classifier.NeuralNetwork.SaveToXml(outputFilePath);
    }

    public void NormalizeAlphabets(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        if (!Directory.Exists(sourceDirectoryPath))
            throw new ArgumentException("Source directory does not exist.");

        if (Directory.GetFiles(sourceDirectoryPath).Length == 0)
            throw new ArgumentException("Source directory is empty.");

        if (!Directory.Exists(destinationDirectoryPath))
            Directory.CreateDirectory(destinationDirectoryPath);

        var x = _settings.CharNormalizedDimensionsX;
        var y = _settings.CharNormalizedDimensionsY;
        var logger = _loggerFactory.CreateLogger<AnprEngine>();
        logger.LogInformation("Creating new alphabet ({Width} x {Height} px)...", x, y);

        foreach (var fileName in Character.AlphabetList(sourceDirectoryPath))
        {
            using var character = new Character(fileName, _settings);
            character.Normalize(_settings);
            character.Save(Path.Combine(destinationDirectoryPath, fileName));
            logger.LogInformation("{FileName} done", fileName);
        }
    }

    private static ICharacterRecognizer CreateRecognizer(AnprSettings settings, ILogger logger) =>
        settings.IntelligenceClassificationMethod == 0
            ? new KnnPatternClassifier(settings, logger)
            : new NeuralPatternClassifier(settings, logger);

    private static RecognitionOptions MergeCancellation(RecognitionOptions? options, CancellationToken ct)
    {
        options ??= new RecognitionOptions();
        if (ct == default)
            return options;

        return new RecognitionOptions
        {
            DumpStagesDirectory = options.DumpStagesDirectory,
            EnableSkewCorrection = options.EnableSkewCorrection,
            OwnsInputImage = options.OwnsInputImage,
            CancellationToken = ct,
            DumpSkewDiagnostics = options.DumpSkewDiagnostics,
        };
    }

    private static List<PlateCandidateResult> BuildCandidateDiagnostics(
        List<PlateCandidate> candidates,
        PlateCandidate? best,
        string? bestCorrectedText = null)
    {
        if (candidates.Count == 0)
            return [];

        List<PlateCandidateResult> results = [];
        foreach (var candidate in candidates)
        {
            List<CharacterDiagnostic> charDiagnostics = [];
            for (var i = 0; i < candidate.RecognizedPlate.Characters.Count; i++)
            {
                var rc = candidate.RecognizedPlate.Characters[i];
                if (rc.Patterns is null || rc.Patterns.Count == 0)
                    continue;

                charDiagnostics.Add(new CharacterDiagnostic
                {
                    Character = rc.Patterns[0].Char,
                    ClassificationCost = rc.Patterns[0].Cost,
                    PositionIndex = i,
                });
            }

            results.Add(new PlateCandidateResult
            {
                RawText = candidate.RawText,
                CorrectedText = ReferenceEquals(candidate, best) ? bestCorrectedText : null,
                Score = candidate.Score,
                BandIndex = candidate.BandIndex,
                PlateIndex = candidate.PlateIndex,
                Characters = charDiagnostics,
            });
        }

        return results;
    }
}
