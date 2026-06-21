using System.Diagnostics;

namespace dotnetANPR.Utilities;

internal class TimeMeter
{
    private readonly Stopwatch _stopwatch;

    public TimeMeter()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public long GetTime()
    {
        _stopwatch.Stop();
        return _stopwatch.ElapsedMilliseconds;
    }
}
