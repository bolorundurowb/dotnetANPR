using System;
using System.Collections.Generic;
using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.ImageAnalysis;
using Microsoft.Extensions.Logging;

namespace DotNetANPR.Recognizer;

public class KnnPatternClassifier : CharacterRecognizer
{
    private static readonly ILogger<KnnPatternClassifier> Logger =
        LoggerFactory.Create(_ => { }).CreateLogger<KnnPatternClassifier>();

    private readonly List<List<double>> _learnLists;

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
    /// Simple vector distance.
    /// </summary>
    /// <param name="vectorA">Vector A</param>
    /// <param name="vectorB">Vector B</param>
    /// <returns>Their simple distance</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="SimplifiedEuclideanDistance(List{double}, List{double})"/> instead, which works better.
    /// </remarks>
    public double SimpleVectorDistance(List<double> vectorA, List<double> vectorB)
    {
        var distance = 0.0;
        for (var i = 0; i < vectorA.Count; i++)
        {
            distance += Math.Abs(vectorA[i] - vectorB[i]);
        }

        return distance;
    }

    private double Difference(List<double> vectorA, List<double> vectorB) =>
        vectorA.Select((t, x) => Math.Abs(t - vectorB[x])).Sum();

    /// <summary>
    /// Calculates the Euclidean distance between two vectors.
    /// Worked better than the simple vector distance.
    /// </summary>
    /// <param name="vectorA">Vector A</param>
    /// <param name="vectorB">Vector B</param>
    /// <returns>The Euclidean distance of A and B</returns>
    public double CalculateEuclideanDistance(double[] vectorA, double[] vectorB)
    {
        // Calculate the Euclidean distance using the formula:
        // sqrt(sum((a_i - b_i)^2))
        var distance = vectorA.Select((t, i) => t - vectorB[i]).Sum(diff => diff * diff);
        return Math.Sqrt(distance);
    }

    private static double SimplifiedEuclideanDistance(List<double> vectorA, List<double> vectorB) => vectorA
        .Select((t, x) => Math.Abs(t - vectorB[x])).Sum(partialDiff => partialDiff * partialDiff);

    #endregion
}
