using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNETANPR.GUI
{
    public class TimeMeter
    {
        private long startTime;
        public TimeMeter()
        {
            this.startTime = DateTime.Now.Millisecond;
        }
        public long getTime()
        {
            return DateTime.Now.Millisecond -this.startTime;
        }
    }
}
