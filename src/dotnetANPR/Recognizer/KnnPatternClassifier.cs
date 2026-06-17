using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
namespace DotNetANPR.Recognizer;

/// <summary>
/// K-nearest neighbor pattern classifier that recognizes characters by comparing their
/// extracted feature vectors against pre-loaded alphabet templates using Euclidean distance.
/// </summary>
public class KnnPatternClassifier : CharacterRecognizer
{
    private readonly List<List<double>> _learnLists;

    /// <summary>
    /// Initializes a new <see cref="KnnPatternClassifier"/> by loading alphabet images from the
    /// configured directory and extracting feature vectors for each character template.
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

    }

    /// <summary>
    /// Recognizes a character by computing the simplified Euclidean distance between
    /// the character's feature vector and all stored alphabet templates.
    /// </summary>
    /// <param name="chr">The character to recognize.</param>
    /// <returns>
    /// A <see cref="RecognizedCharacter"/> with patterns sorted by ascending cost (distance).
    /// </returns>
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

    #region Private Helpers

    /// <summary>
    /// Computes the squared Euclidean distance between two feature vectors.
    /// This simplified version omits the final square root for performance.
    /// </summary>
    /// <param name="vectorA">The first feature vector.</param>
    /// <param name="vectorB">The second feature vector.</param>
    /// <returns>The sum of squared element-wise differences.</returns>
    private static double SimplifiedEuclideanDistance(List<double> vectorA, List<double> vectorB) =>
        vectorA.Select((t, x) => Math.Abs(t - vectorB[x]))
               .Sum(partialDiff => partialDiff * partialDiff);

    #endregion
}
