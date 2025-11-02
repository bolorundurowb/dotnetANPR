using DotNetANPR.ImageAnalysis;

namespace DotNetANPR.Recognizer;

public interface ICharacterRecognizer
{
    RecognizedChar Recognize(LicensePlateChar chr);
}