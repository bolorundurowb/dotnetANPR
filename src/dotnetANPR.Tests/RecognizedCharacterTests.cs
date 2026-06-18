using DotNetANPR.Recognizer;
using Xunit;

namespace DotNetANPR.Tests;

public class RecognizedCharacterTests
{
    [Fact]
    public void Patterns_IsNull_BeforeSorting()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 0.5f));

        Assert.Null(rc.Patterns);
        Assert.False(rc.IsSorted);
    }

    [Fact]
    public void Sort_OrdersPatternsByCost()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('C', 0.9f));
        rc.AddPattern(new RecognizedPattern('A', 0.1f));
        rc.AddPattern(new RecognizedPattern('B', 0.5f));

        rc.Sort(false);

        Assert.True(rc.IsSorted);
        Assert.NotNull(rc.Patterns);
        Assert.Equal('A', rc.Patterns[0].Char);
        Assert.Equal('B', rc.Patterns[1].Char);
        Assert.Equal('C', rc.Patterns[2].Char);
    }

    [Fact]
    public void Pattern_ReturnsCorrectPattern()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('X', 0.2f));
        rc.Sort(false);

        var pattern = rc.Pattern(0);

        Assert.NotNull(pattern);
        Assert.Equal('X', pattern.Char);
    }

    [Fact]
    public void Pattern_ReturnsNull_ForOutOfRangeIndex()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('X', 0.2f));
        rc.Sort(false);

        Assert.Null(rc.Pattern(5));
    }
}
