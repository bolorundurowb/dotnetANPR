using dotnetANPR.Configuration;
using dotnetANPR.ImageAnalysis;
using dotnetANPR.Recognizer;

namespace dotnetANPR.Pipeline;

internal interface ICharacterRecognizer
{
    RecognizedCharacter Recognize(Character character, AnprSettings settings);
}
