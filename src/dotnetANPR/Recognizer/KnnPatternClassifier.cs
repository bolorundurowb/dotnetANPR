using System;
using System.Collections.Generic;
using System.Linq;
using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace dotnetANPR.Recognizer;

/// <summary>
/// Character recognition using k-Nearest Neighbour (KNN) pattern matching.
/// Compares extracted character features against a learned alphabet using simplified Euclidean distance.
/// </summary>
public class KnnPatternClassifier : CharacterRecognizer
{
    private static readonly ILogger<KnnPatternClassifier> Logger = Logging.GetLogger<KnnPatternClassifier>();

    private readonly List<List<double>> _learnLists;

    /// <summary>
    /// Initialises the classifier by loading and normalising the alphabet images from the configured path.
    /// </summary>
    public KnnPatternClassifier()
    {
        var path = Configurator.Instance.GetPath("char_learnAlphabetPath");
        _learnLists = new List<List<double>>(36);
        var filenames = Character.AlphabetList(path);

        foreach (var imgChar in filenames.Select(fileName => new Character(fileName)))
        {
            imgChar.Normalize();
            _learnLists.Add(imgChar.ExtractFeatures());
        }

        // check vector elements
        foreach (var _ in _learnLists.Where(learnList => learnList == null))
            Logger.LogWarning("Alphabet in {} is not complete", path);
    }

    /// <summary>
    /// Recognises a character by computing Euclidean distances against all learned alphabet patterns.
    /// </summary>
    public override RecognizedCharacter Recognize(Character chr)
    {
        var features = chr.ExtractFeatures();
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
