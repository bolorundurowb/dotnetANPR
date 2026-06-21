using System.Threading;

namespace dotnetANPR;

/// <summary>
/// Per-call options for <see cref="AnprEngine.Recognize"/> methods.
/// </summary>
public sealed record RecognitionOptions
{
    public string? DumpStagesDirectory { get; init; }
    public bool EnableSkewCorrection { get; init; }
    public bool OwnsInputImage { get; init; }
    public CancellationToken CancellationToken { get; init; } = default;
    public bool DumpSkewDiagnostics { get; init; } = true;
}
