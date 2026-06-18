using DotNetANPR.Recognizer;
using Xunit;

namespace DotNetANPR.Tests;

public class CharacterRecognizerTests
{
    [Fact]
    public void Alphabet_ContainsDigitsAndUppercaseLetters()
    {
        var alphabet = CharacterRecognizer.Alphabet;

        Assert.Equal(36, alphabet.Length);
        Assert.Equal('0', alphabet[0]);
        Assert.Equal('9', alphabet[9]);
        Assert.Equal('A', alphabet[10]);
        Assert.Equal('Z', alphabet[35]);
    }

    [Fact]
    public void Features_IsFloatArray()
    {
        var features = CharacterRecognizer.Features;

        Assert.Equal(12, features.Length);
        Assert.All(features, f => Assert.Equal(4, f.Length));
        Assert.All(features, f => Assert.All(f, v => Assert.True(v == 0f || v == 1f)));
    }
}
