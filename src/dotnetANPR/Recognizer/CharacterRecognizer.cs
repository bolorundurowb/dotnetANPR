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
        new[] { 0f, 1f, 0f, 1f }, // 0
        new[] { 1f, 0f, 1f, 0f }, // 1
        new[] { 0f, 0f, 1f, 1f }, // 2
        new[] { 1f, 1f, 0f, 0f }, // 3
        new[] { 0f, 0f, 0f, 1f }, // 4
        new[] { 1f, 0f, 0f, 0f }, // 5
        new[] { 1f, 1f, 1f, 0f }, // 6
        new[] { 0f, 1f, 1f, 1f }, // 7
        new[] { 0f, 0f, 1f, 0f }, // 8
        new[] { 0f, 1f, 0f, 0f }, // 9
        new[] { 1f, 0f, 1f, 1f }, // 10
        new[] { 1f, 1f, 0f, 1f }  // 11
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
