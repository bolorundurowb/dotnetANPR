using dotnetANPR.ImageAnalysis;

namespace dotnetANPR.Recognizer;

/// <summary>
/// Abstract base class for character recognition algorithms.
/// Provides the alphabet of supported characters and feature templates used by derived classifiers.
/// </summary>
public abstract class CharacterRecognizer
{
    /// <summary>
    /// The supported alphanumeric characters that can be recognised (0-9, A-Z).
    /// </summary>
    public static char[] Alphabet =
    [
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z'
    ];

    /// <summary>
    /// Predefined 2x2 feature templates used for edge-based feature extraction.
    /// </summary>
    public static readonly float[][] Features =
    [
        [0, 1, 0, 1],
        [1, 0, 1, 0],
        [0, 0, 1, 1],
        [1, 1, 0, 0],
        [0, 0, 0, 1],
        [1, 0, 0, 0],
        [1, 1, 1, 0],
        [0, 1, 1, 1],
        [0, 0, 1, 0],
        [0, 1, 0, 0],
        [1, 0, 1, 1],
        [1, 1, 0, 1]
    ];

    /// <summary>
    /// Recognises the character and returns ranked pattern matches with associated costs.
    /// </summary>
    public abstract RecognizedCharacter Recognize(Character character);
}