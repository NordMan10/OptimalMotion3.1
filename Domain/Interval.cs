

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

        /// <summary>
        /// проверяет пересечение интервалов
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool IsIntervalsIntersects(Interval interval)
        {
            return interval.LastMoment > FirstMoment && interval.FirstMoment < LastMoment;
        }

        public static Interval operator +(Interval firstSummand, Interval secondSummand)
        {
            return new Interval(firstSummand.FirstMoment + secondSummand.FirstMoment, firstSummand.LastMoment + secondSummand.LastMoment);
        }

        /// <summary>
        /// Возвращает разность между последним и первый моментом
        /// </summary>
        /// <returns></returns>
        public int GetIntervalDuration()
        {
            return LastMoment - FirstMoment;
        }
    }
}
