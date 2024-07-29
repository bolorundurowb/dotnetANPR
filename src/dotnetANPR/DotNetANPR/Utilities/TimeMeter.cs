using System.Diagnostics;

namespace DotNetANPR.Utilities;

public class TimeMeter
{
    private Stopwatch _stopwatch;

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
