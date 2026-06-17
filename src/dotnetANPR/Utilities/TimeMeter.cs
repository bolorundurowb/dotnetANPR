using System.Diagnostics;

namespace DotNetANPR.Utilities;

/// <summary>
/// A simple stopwatch wrapper used to measure elapsed time for ANPR processing stages.
/// Starts timing on construction and returns the elapsed milliseconds when <see cref="GetTime"/> is called.
/// </summary>
public class TimeMeter
{
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// Initializes a new <see cref="TimeMeter"/> and immediately starts timing.
    /// </summary>
    public TimeMeter()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops the internal stopwatch and returns the total elapsed time in milliseconds.
    /// </summary>
    /// <returns>Elapsed time in milliseconds since construction.</returns>
    public long GetTime()
    {
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }
}
