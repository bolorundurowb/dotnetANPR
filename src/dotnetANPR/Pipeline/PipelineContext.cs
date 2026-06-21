using System.Threading;
using dotnetANPR.Configuration;
using dotnetANPR.Utilities;
using Microsoft.Extensions.Logging;

namespace dotnetANPR.Pipeline;

/// <summary>
/// Per-recognition execution context passed through the pipeline.
/// </summary>
internal sealed class PipelineContext
{
    public AnprSettings Settings { get; }
    public ILogger Logger { get; }
    public StageWriter? StageWriter { get; }
    public CancellationToken CancellationToken { get; }
    public bool EnableSkewCorrection { get; }
    public bool EnableSkewDiagnostics { get; }

    public PipelineContext(
        AnprSettings settings,
        ILogger logger,
        StageWriter? stageWriter = null,
        CancellationToken cancellationToken = default,
        bool enableSkewCorrection = false,
        bool enableSkewDiagnostics = true)
    {
        Settings = settings;
        Logger = logger;
        StageWriter = stageWriter;
        CancellationToken = cancellationToken;
        EnableSkewCorrection = enableSkewCorrection;
        EnableSkewDiagnostics = enableSkewDiagnostics;
    }
}
