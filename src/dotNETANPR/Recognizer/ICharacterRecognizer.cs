using dotnetANPR.ImageAnalysis;

namespace dotnetANPR.Recognizer
{
    public interface ICharacterRecognizer
    {
        CharacterRecognizer.RecognizedChar Recognize(Character character);
    }
}
