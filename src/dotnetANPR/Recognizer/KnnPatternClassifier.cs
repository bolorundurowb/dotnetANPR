using System;
using System.Collections.Generic;
using System.Linq;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using Microsoft.Extensions.Logging;

namespace dotnetANPR.Recognizer;

internal sealed class KnnPatternClassifier : CharacterRecognizer
{
    private readonly List<List<double>> _learnLists;
    private readonly ILogger _logger;

    public KnnPatternClassifier(AnprSettings settings, ILogger logger)
    {
        _logger = logger;
        var path = settings.CharLearnAlphabetPath;
        _learnLists = new List<List<double>>(36);
        var filenames = Character.AlphabetList(path);

        foreach (var fileName in filenames)
        {
            using var imgChar = new Character(fileName, settings);
            imgChar.Normalize(settings);
            _learnLists.Add(imgChar.ExtractFeatures(settings));
        }

        foreach (var _ in _learnLists.Where(learnList => learnList == null))
            _logger.LogWarning("Alphabet in {Path} is not complete", path);
    }

    public override RecognizedCharacter Recognize(Character chr, AnprSettings settings)
    {
        var features = chr.ExtractFeatures(settings);
        var recognized = new RecognizedCharacter();

        for (var x = 0; x < _learnLists.Count; x++)
        {
            var fx = SimplifiedEuclideanDistance(features, _learnLists[x]);
            recognized.AddPattern(new RecognizedPattern(Alphabet[x], (float)fx));
        }

        recognized.Sort(false);
        return recognized;
    }

    private static double SimplifiedEuclideanDistance(List<double> vectorA, List<double> vectorB) => vectorA
        .Select((t, x) => Math.Abs(t - vectorB[x])).Sum(partialDiff => partialDiff * partialDiff);
}
