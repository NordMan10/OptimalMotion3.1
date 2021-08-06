

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Представление интервала времени
    /// </summary>
    public class Interval
    {
        public Interval(int firstMoment, int lastMoment)
        {
            StartMoment = firstMoment;
            EndMoment = lastMoment;
        }

        /// <summary>
        /// Начальный момент
        /// </summary>
        public int StartMoment { get; }
        /// <summary>
        /// Конечный момент
        /// </summary>
        public int EndMoment { get; }

        /// <summary>
        /// Проверяет пересечение интервалов
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public bool IsIntervalsIntersects(Interval interval)
        {
            return interval.EndMoment > StartMoment && interval.StartMoment < EndMoment;
        }

        public static Interval operator +(Interval firstSummand, Interval secondSummand)
        {
            return new Interval(firstSummand.StartMoment + secondSummand.StartMoment, firstSummand.EndMoment + secondSummand.EndMoment);
        }

        /// <summary>
        /// Возвращает разность между последним и первый моментом
        /// </summary>
        /// <returns></returns>
        public int GetIntervalDuration()
        {
            return EndMoment - StartMoment;
        }

        /// <summary>
        /// Возвращает true, если переданным момент попадаем в данный интервал (включая границы), и false, если нет
        /// </summary>
        /// <param name="moment"></param>
        /// <returns></returns>
        public bool IsMomentInInterval(int moment)
        {
            return StartMoment <= moment && EndMoment >= moment;
        }
    }
}
