using DotNetANPR.Recognizer;
using OmniAssert;
using Xunit;

namespace DotNetANPR.Tests;

public class RecognizedCharacterTests
{
    [Fact]
    public void Patterns_IsNull_BeforeSorting()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 0.5f));

        (rc.Patterns == null).Verify().ToBeTrue();
        rc.IsSorted.Verify().ToBeFalse();
    }

    [Fact]
    public void Sort_OrdersPatternsByCost()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('C', 0.9f));
        rc.AddPattern(new RecognizedPattern('A', 0.1f));
        rc.AddPattern(new RecognizedPattern('B', 0.5f));

        rc.Sort(false);

        rc.IsSorted.Verify().ToBeTrue();
        (rc.Patterns != null).Verify().ToBeTrue();
        rc.Patterns![0].Char.Verify().ToBe('A');
        rc.Patterns[1].Char.Verify().ToBe('B');
        rc.Patterns[2].Char.Verify().ToBe('C');
    }

    [Fact]
    public void Pattern_ReturnsCorrectPattern()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('X', 0.2f));
        rc.Sort(false);

        var pattern = rc.Pattern(0);

        pattern.Verify().NotToBeNull();
        pattern!.Char.Verify().ToBe('X');
    }

    [Fact]
    public void Pattern_ReturnsNull_ForOutOfRangeIndex()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('X', 0.2f));
        rc.Sort(false);

        rc.Pattern(5).Verify().ToBeNull();
    }
}
