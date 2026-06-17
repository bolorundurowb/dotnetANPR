using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer;

/// <summary>
/// Abstract base class for character recognition. Defines the 36-character alphabet (0-9, A-Z)
/// and the edge detection feature patterns used by concrete recognizer implementations.
/// </summary>
public abstract class CharacterRecognizer
{
    /// <summary>
    /// The 36-character alphabet used for plate recognition: digits 0-9 followed by letters A-Z.
    /// </summary>
    public static readonly char[] Alphabet =
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z'
    };

    /// <summary>
    /// Edge detection feature patterns. Each row defines a 4-directional pattern
    /// (4 directions x 3 patterns = 12 total) used for feature extraction.
    /// </summary>
    public static readonly float[][] Features =
    {
        new[] { 0, 1, 0, 1 }, // 0
        new[] { 1, 0, 1, 0 }, // 1
        new[] { 0, 0, 1, 1 }, // 2
        new[] { 1, 1, 0, 0 }, // 3
        new[] { 0, 0, 0, 1 }, // 4
        new[] { 1, 0, 0, 0 }, // 5
        new[] { 1, 1, 1, 0 }, // 6
        new[] { 0, 1, 1, 1 }, // 7
        new[] { 0, 0, 1, 0 }, // 8
        new[] { 0, 1, 0, 0 }, // 9
        new[] { 1, 0, 1, 1 }, // 10
        new[] { 1, 1, 0, 1 }  // 11
    };

    /// <summary>
    /// Recognizes a single character from its image representation.
    /// </summary>
    /// <param name="character">The character image to recognize.</param>
    /// <returns>
    /// A <see cref="RecognizedCharacter"/> containing all candidate pattern matches, sorted by cost.
    /// </returns>
    public abstract RecognizedCharacter Recognize(Character character);
}
