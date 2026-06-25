using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class RecognitionResultTests
{
    // ── Success flag ──────────────────────────────────────────────────────────

    [TestMethod]
    public void Success_IsTrueWhenTextIsNotNull()
    {
        var result = new RecognitionResult { Text = "ABC123" };
        result.Success.Verify().ToBeTrue();
    }

    [TestMethod]
    public void Success_IsFalseWhenTextIsNull()
    {
        var result = new RecognitionResult { Text = null };
        result.Success.Verify().ToBeFalse();
    }

    // ── Confidence ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Confidence_IsStoredCorrectly()
    {
        var result = new RecognitionResult { Confidence = 0.87 };
        result.Confidence.Verify().ToBeApproximately(0.87, 0.0001);
    }

    // ── Duration ────────────────────────────────────────────────────────────

    [TestMethod]
    public void Duration_IsStoredCorrectly()
    {
        var duration = TimeSpan.FromMilliseconds(42);
        var result = new RecognitionResult { Duration = duration };
        result.Duration.Verify().ToBe(duration);
    }

    // ── Candidates ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Candidates_DefaultsToNull()
    {
        var result = new RecognitionResult();
        result.Candidates.Verify().ToBeNull();
    }

    [TestMethod]
    public void Candidates_CanContainMultipleEntries()
    {
        var candidates = new[]
        {
            new PlateCandidateResult { RawText = "ABC123", Score = 0.9 },
            new PlateCandidateResult { RawText = "AB1234", Score = 0.7 },
        };
        var result = new RecognitionResult { Candidates = candidates };
        result.Candidates!.Verify().ToHaveCount(2);
    }

    // ── StageTimings ──────────────────────────────────────────────────────────

    [TestMethod]
    public void StageTimings_DefaultsToNull()
    {
        var result = new RecognitionResult();
        result.StageTimings.Verify().ToBeNull();
    }

    [TestMethod]
    public void StageTimings_CanBeProvided()
    {
        var timings = new Dictionary<string, TimeSpan>
        {
            ["band-detection"] = TimeSpan.FromMilliseconds(10),
            ["plate-detection"] = TimeSpan.FromMilliseconds(20),
        };
        var result = new RecognitionResult { StageTimings = timings };
        result.StageTimings!.Verify().ToHaveCount(2);
    }

    // ── PlateCandidateResult ──────────────────────────────────────────────────

    [TestMethod]
    public void PlateCandidateResult_RawText_DefaultsToEmpty()
    {
        var candidate = new PlateCandidateResult();
        candidate.RawText.Verify().ToBe("");
    }

    [TestMethod]
    public void PlateCandidateResult_CorrectedText_DefaultsToNull()
    {
        var candidate = new PlateCandidateResult();
        candidate.CorrectedText.Verify().ToBeNull();
    }

    [TestMethod]
    public void PlateCandidateResult_Characters_DefaultsToEmpty()
    {
        var candidate = new PlateCandidateResult();
        candidate.Characters.Verify().ToHaveCount(0);
    }

    [TestMethod]
    public void PlateCandidateResult_StoresBandAndPlateIndex()
    {
        var candidate = new PlateCandidateResult { BandIndex = 1, PlateIndex = 2 };
        candidate.BandIndex.Verify().ToBe(1);
        candidate.PlateIndex.Verify().ToBe(2);
    }

    // ── CharacterDiagnostic ───────────────────────────────────────────────────

    [TestMethod]
    public void CharacterDiagnostic_StoresAllFields()
    {
        var diag = new CharacterDiagnostic
        {
            Character = 'X',
            ClassificationCost = 12.5f,
            PositionIndex = 3,
        };

        diag.Character.Verify().ToBe('X');
        diag.ClassificationCost.Verify().ToBeApproximately(12.5f, 0.0001f);
        diag.PositionIndex.Verify().ToBe(3);
    }
}
