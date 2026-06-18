using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Recognizer;
using OmniAssert;
using Xunit;

namespace DotNetANPR.Tests;

public class KnnPatternClassifierTests
{
    public KnnPatternClassifierTests()
    {
        AnprConfig.Reset();
    }

    [Fact]
    public void Constructor_LoadsDefaultAlphabet()
    {
        var classifier = new KnnPatternClassifier();

        classifier.Verify().NotToBeNull();
    }

    [Fact]
    public void Recognize_ReturnsSortedPatterns()
    {
        var classifier = new KnnPatternClassifier();

        using var character = LoadEmbeddedCharacter('a');
        var result = classifier.Recognize(character);

        result.IsSorted.Verify().ToBeTrue();
        result.Patterns!.Verify().ToHaveCount(36);
    }

    private static ImageAnalysis.Character LoadEmbeddedCharacter(char label)
    {
        using var stream = Utilities.ResourceHelper.OpenStream($"Resources/alphabets/alphabet_8x13/{label}_8x13.jpg")!;
        return new ImageAnalysis.Character(stream);
    }
}
