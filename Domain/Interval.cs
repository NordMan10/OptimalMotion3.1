

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

        public bool IsIntervalsIntersects(Interval interval)
        {
            return interval.LastMoment >= FirstMoment && interval.FirstMoment <= LastMoment;
        }
    }
}
