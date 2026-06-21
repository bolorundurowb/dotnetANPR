using System;
using System.Collections.Generic;

namespace dotnetANPR;

/// <summary>
/// Per-character diagnostic data for a plate candidate.
/// </summary>
public sealed class CharacterDiagnostic
{
    public char Character { get; init; }
    public float ClassificationCost { get; init; }
    public int PositionIndex { get; init; }
}

/// <summary>
/// Diagnostic data for a single plate candidate evaluated during recognition.
/// </summary>
public sealed class PlateCandidateResult
{
    public string RawText { get; init; } = "";
    public string? CorrectedText { get; init; }
    public double Score { get; init; }
    public int BandIndex { get; init; }
    public int PlateIndex { get; init; }
    public IReadOnlyList<CharacterDiagnostic> Characters { get; init; } = Array.Empty<CharacterDiagnostic>();
}

/// <summary>
/// Result of a licence plate recognition operation.
/// </summary>
public sealed class RecognitionResult
{
    public string? Text { get; init; }
    public double Confidence { get; init; }
    public TimeSpan Duration { get; init; }
    public bool Success => Text is not null;
    public IReadOnlyList<PlateCandidateResult>? Candidates { get; init; }
    public IReadOnlyDictionary<string, TimeSpan>? StageTimings { get; init; }
}
