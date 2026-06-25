using dotnetANPR.Recognizer;
using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class RecognizedCharacterTests
{
    // ── IsSorted ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void Patterns_IsNullBeforeSort()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 1f));
        rc.IsSorted.Verify().ToBeFalse();
        rc.Patterns.Verify().ToBeNull();
    }

    [TestMethod]
    public void Patterns_IsAccessibleAfterSort()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 1f));
        rc.Sort(false);
        rc.IsSorted.Verify().ToBeTrue();
        rc.Patterns.Verify().NotToBeNull();
    }

    // ── Sort ascending (false) ────────────────────────────────────────────────

    [TestMethod]
    public void Sort_Ascending_OrdersLowestCostFirst()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('C', 90f));
        rc.AddPattern(new RecognizedPattern('A', 10f));
        rc.AddPattern(new RecognizedPattern('B', 50f));
        rc.Sort(false);

        rc.Patterns![0].Char.Verify().ToBe('A');
        rc.Patterns[1].Char.Verify().ToBe('B');
        rc.Patterns[2].Char.Verify().ToBe('C');
    }

    [TestMethod]
    public void Sort_Ascending_FirstPatternHasLowestCost()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('X', 75f));
        rc.AddPattern(new RecognizedPattern('Y', 25f));
        rc.Sort(false);

        (rc.Patterns![0].Cost <= rc.Patterns[1].Cost).Verify().ToBeTrue();
    }

    // ── Sort descending (true) ────────────────────────────────────────────────

    [TestMethod]
    public void Sort_Descending_OrdersHighestCostFirst()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 10f));
        rc.AddPattern(new RecognizedPattern('B', 80f));
        rc.Sort(true);

        rc.Patterns![0].Char.Verify().ToBe('B');
        rc.Patterns[1].Char.Verify().ToBe('A');
    }

    // ── Sort idempotence ──────────────────────────────────────────────────────

    [TestMethod]
    public void Sort_CalledTwice_DoesNotChangeOrder()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('Z', 5f));
        rc.AddPattern(new RecognizedPattern('A', 99f));
        rc.Sort(false);

        var firstChar = rc.Patterns![0].Char;
        rc.Sort(false); // second call should be a no-op
        rc.Patterns[0].Char.Verify().ToBe(firstChar);
    }

    // ── Pattern(index) accessor ───────────────────────────────────────────────

    [TestMethod]
    public void Pattern_ReturnsNullBeforeSort()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 1f));
        rc.Pattern(0).Verify().ToBeNull();
    }

    [TestMethod]
    public void Pattern_ReturnsCorrectElementAfterSort()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 10f));
        rc.AddPattern(new RecognizedPattern('B', 20f));
        rc.Sort(false);

        var first = rc.Pattern(0);
        first.Verify().NotToBeNull();
        first!.Char.Verify().ToBe('A');
    }

    [TestMethod]
    public void Pattern_OutOfRangeIndex_ReturnsNull()
    {
        var rc = new RecognizedCharacter();
        rc.AddPattern(new RecognizedPattern('A', 1f));
        rc.Sort(false);
        rc.Pattern(99).Verify().ToBeNull();
    }

    // ── RecognizedPattern ─────────────────────────────────────────────────────

    [TestMethod]
    public void RecognizedPattern_StoresCharAndCost()
    {
        var rp = new RecognizedPattern('G', 42.5f);
        rp.Char.Verify().ToBe('G');
        rp.Cost.Verify().ToBeApproximately(42.5f, 0.0001f);
    }

    [TestMethod]
    public void RecognizedPattern_ZeroCost_IsValid()
    {
        var rp = new RecognizedPattern('0', 0f);
        rp.Cost.Verify().ToBeApproximately(0f, 0.0001f);
    }
}
