using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer
{
    public abstract class CharacterRecognizer
    {
        public static char[] Alphabet =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
            'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static float[][] Features =
        {
            new float[] { 0, 1, 0, 1 }, // 0
            new float[] { 1, 0, 1, 0 }, // 1
            new float[] { 0, 0, 1, 1 }, // 2
            new float[] { 1, 1, 0, 0 }, // 3
            new float[] { 0, 0, 0, 1 }, // 4
            new float[] { 1, 0, 0, 0 }, // 5
            new float[] { 1, 1, 1, 0 }, // 6
            new float[] { 0, 1, 1, 1 }, // 7
            new float[] { 0, 0, 1, 0 }, // 8
            new float[] { 0, 1, 0, 0 }, // 9
            new float[] { 1, 0, 1, 1 }, // 10
            new float[] { 1, 1, 0, 1 }  // 11
        };

        public abstract RecognizedCharacter Recognize(Character character);
    }
}
