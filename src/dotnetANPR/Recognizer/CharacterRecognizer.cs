using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Pipeline;

namespace dotnetANPR.Recognizer;

internal abstract class CharacterRecognizer : ICharacterRecognizer
{
    public static char[] Alphabet =
    [
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
        'U', 'V', 'W', 'X', 'Y', 'Z'
    ];

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

    public abstract RecognizedCharacter Recognize(Character character, AnprSettings settings);
}
