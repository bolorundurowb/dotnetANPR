using System.Linq;
using DotNetANPR.Configuration;
using DotNetANPR.Recognizer;
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

        Assert.NotNull(classifier);
    }

    [Fact]
    public void Recognize_ReturnsSortedPatterns()
    {
        var classifier = new KnnPatternClassifier();

        using var character = LoadEmbeddedCharacter('a');
        var result = classifier.Recognize(character);

        Assert.True(result.IsSorted);
        Assert.NotNull(result.Patterns);
        Assert.Equal(36, result.Patterns.Count);
    }

    private static ImageAnalysis.Character LoadEmbeddedCharacter(char label)
    {
        using var stream = Utilities.ResourceHelper.OpenStream($"Resources/alphabets/alphabet_8x13/{label}_8x13.jpg")!;
        return new ImageAnalysis.Character(stream);
    }
}
