using dotNETANPR.ImageAnalysis;

namespace dotNETANPR.Recognizer
{
    public interface ICharacterRecognizer
    {
        CharacterRecognizer.RecognizedChar Recognize(Character character);
    }
}
