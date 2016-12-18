using System;

namespace dotNETANPR.GUI
{
    public class TimeMeter
    {
        private long startTime;

        public TimeMeter()
        {
            this.startTime = (long) DateTime.Now.TimeOfDay.TotalMilliseconds;
        }
        public long GetTime()
        {
            return (long) (DateTime.Now.TimeOfDay.TotalMilliseconds - this.startTime);
        }
    }
}
