using System;

namespace dotNETANPR.Gui
{
    public class TimeMeter
    {
        private readonly long _startTime;

        public TimeMeter()
        {
            _startTime = (long) (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public long GetTime()
        {
            return (long) (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds - _startTime;
        }
    }
}