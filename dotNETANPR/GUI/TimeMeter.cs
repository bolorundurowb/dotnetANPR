using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.GUI
{
    class TimeMeter
    {
        private long startTime;
        public TimeMeter()
        {
            this.startTime = DateTime.Now.Millisecond;
        }
        public long GetTime()
        {
            return DateTime.Now.Millisecond - this.startTime;
        }
    }
}
