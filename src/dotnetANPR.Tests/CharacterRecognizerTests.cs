using DotNetANPR.Recognizer;
using OmniAssert;
using Xunit;

namespace DotNetANPR.Tests;

public class CharacterRecognizerTests
{
    [Fact]
    public void Alphabet_ContainsDigitsAndUppercaseLetters()
    {
        var alphabet = CharacterRecognizer.Alphabet;

        alphabet.Length.Verify().ToBe(36);
        alphabet[0].Verify().ToBe('0');
        alphabet[9].Verify().ToBe('9');
        alphabet[10].Verify().ToBe('A');
        alphabet[35].Verify().ToBe('Z');
    }

    [Fact]
    public void Features_IsFloatArray()
    {
        var features = CharacterRecognizer.Features;

        features.Length.Verify().ToBe(12);
        features.Verify().AllSatisfy(f => f.Length == 4);
        features.Verify().AllSatisfy(f => f.All(v => v == 0f || v == 1f));
    }
}
