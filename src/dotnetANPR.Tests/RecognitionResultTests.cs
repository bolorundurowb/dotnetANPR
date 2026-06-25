using OmniAssert;

namespace dotnetANPR.Tests;

[TestClass]
public class RecognitionResultTests
{
    [TestMethod]
    public void Success_IsTrueWhenTextIsNotNull()
    {
        var result = new RecognitionResult { Text = "ABC123" };
        result.Success.Must().BeTrue();
    }

    [TestMethod]
    public void Success_IsFalseWhenTextIsNull()
    {
        var result = new RecognitionResult { Text = null };
        result.Success.Must().BeFalse();
    }

    [TestMethod]
    public void Confidence_IsStoredCorrectly()
    {
        var result = new RecognitionResult { Confidence = 0.87 };
        result.Confidence.Must().BeApproximately(0.87, 0.0001);
    }

    [TestMethod]
    public void Duration_IsStoredCorrectly()
    {
        var duration = TimeSpan.FromMilliseconds(42);
        var result = new RecognitionResult { Duration = duration };
        result.Duration.Must().Be(duration);
    }

    [TestMethod]
    public void Candidates_DefaultsToNull()
    {
        var result = new RecognitionResult();
        ((object?)result.Candidates).Must().BeNull();
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
        result.Candidates!.Must().HaveCount(2);
    }

    [TestMethod]
    public void StageTimings_DefaultsToNull()
    {
        var result = new RecognitionResult();
        result.StageTimings.Must().BeNull();
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
        result.StageTimings!.Must().HaveCount(2);
    }

    [TestMethod]
    public void PlateCandidateResult_RawText_DefaultsToEmpty()
    {
        var candidate = new PlateCandidateResult();
        candidate.RawText.Must().Be("");
    }

    [TestMethod]
    public void PlateCandidateResult_CorrectedText_DefaultsToNull()
    {
        var candidate = new PlateCandidateResult();
        candidate.CorrectedText.Must().BeNull();
    }

    [TestMethod]
    public void PlateCandidateResult_Characters_DefaultsToEmpty()
    {
        var candidate = new PlateCandidateResult();
        candidate.Characters.Must().HaveCount(0);
    }

    [TestMethod]
    public void PlateCandidateResult_StoresBandAndPlateIndex()
    {
        var candidate = new PlateCandidateResult { BandIndex = 1, PlateIndex = 2 };
        candidate.BandIndex.Must().Be(1);
        candidate.PlateIndex.Must().Be(2);
    }

    [TestMethod]
    public void CharacterDiagnostic_StoresAllFields()
    {
        var diag = new CharacterDiagnostic
        {
            Character = 'X',
            ClassificationCost = 12.5f,
            PositionIndex = 3,
        };

        diag.Character.Must().Be('X');
        diag.ClassificationCost.Must().BeApproximately(12.5f, 0.0001f);
        diag.PositionIndex.Must().Be(3);
    }
}
