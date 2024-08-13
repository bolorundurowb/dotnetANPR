using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer;

public abstract class CharacterRecognizer
{
    public static char[] Alphabet =
    [
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        'A',
        'B',
        'C',
        'D',
        'E',
        'F',
        'G',
        'H',
        'I',
        'J',
        'K',
        'L',
        'M',
        'N',
        'O',
        'P',
        'Q',
        'R',
        'S',
        'T',
        'U',
        'V',
        'W',
        'X',
        'Y',
        'Z'
    ];

    public static readonly float[,] Features =
    {
        { 0, 1, 0, 1 }, // 0
        { 1, 0, 1, 0 }, // 1
        { 0, 0, 1, 1 }, // 2
        { 1, 1, 0, 0 }, // 3
        { 0, 0, 0, 1 }, // 4
        { 1, 0, 0, 0 }, // 5
        { 1, 1, 1, 0 }, // 6
        { 0, 1, 1, 1 }, // 7
        { 0, 0, 1, 0 }, // 8
        { 0, 1, 0, 0 }, // 9
        { 1, 0, 1, 1 }, // 10
        { 1, 1, 0, 1 }  // 11
    };

    public abstract RecognizedCharacter Recognize(Character character);
}
