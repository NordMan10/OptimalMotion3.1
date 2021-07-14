using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Domain
{
    public class Interval
    {
        public Interval(int firstMoment, int lastMoment)
        {
            FirstMoment = firstMoment;
            LastMoment = lastMoment;
        }

        public int FirstMoment { get; }
        public int LastMoment { get; }
    }
}
