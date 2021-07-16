using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain.Static
{
    public static class IMassServiceDeviceExtensions
    {
        /// <summary>
        /// Добавляет переданный интервал в список интервалов зоны, если он не пересекается с существующими интервалами. Подразумевает, что 
        /// был передан свободный (не пересекающийся интервал)
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="aircraftId">Id ВС</param>
        /// <param name="freeInterval">Рассчитанный свободный интервал</param>
        /// <param name="zoneIntervals">Набор существующих интервалов</param>
        public static void AddAircraftInterval(this IMassServiceZone zone, int aircraftId, Interval freeInterval, Dictionary<int, Interval> zoneIntervals)
        {
            if (CheckIntervalsIntersection(freeInterval, zoneIntervals))
                throw new ArgumentException("Интервалы пересекаются! Передайте проверенный интервал");

            zoneIntervals.Add(aircraftId, freeInterval);
        }

        /// <summary>
        /// Возвращает свободный (непересекающийся с существующими интервалами) интервал
        /// </summary>
        /// <param name="zone">Зона массового обслуживания</param>
        /// <param name="interval">Интервал, который нужно проверить</param>
        /// <param name="zoneIntervals">Набор существующих интервалов</param>
        /// <returns></returns>
        public static Interval GetFreeInterval(this IMassServiceZone zone, Interval interval, Dictionary<int, Interval> zoneIntervals)
        {
            var newInterval = new Interval(interval.FirstMoment, interval.LastMoment);
            foreach (var occupiedInterval in zoneIntervals.Values)
            {
                if (newInterval.LastMoment >= occupiedInterval.FirstMoment && newInterval.FirstMoment <= occupiedInterval.LastMoment)
                {
                    var delay = occupiedInterval.LastMoment - interval.FirstMoment;
                    newInterval = new Interval(interval.FirstMoment + delay, interval.LastMoment + delay);
                }
            }

            return newInterval;
        }

        /// <summary>
        /// Удаляет интервал из набора существующих интервалов по Id ВС
        /// </summary>
        /// <param name="zone">Зона массового обслуживания</param>
        /// <param name="aircraftId">Id Вс</param>
        /// <param name="zoneIntervals">Набор существующих интервалов</param>
        public static void RemoveAircraftInterval(this IMassServiceZone zone, int aircraftId, Dictionary<int, Interval> zoneIntervals)
        {
            zoneIntervals.Remove(aircraftId);
        }

        /// <summary>
        /// Проверяет пересечение переданного интервала с набором существующих интервалов
        /// </summary>
        /// <param name="interval">Интервал для проверки</param>
        /// <param name="zoneIntervals">Набор существующих интервалов</param>
        /// <returns></returns>
        private static bool CheckIntervalsIntersection(Interval interval, Dictionary<int, Interval> zoneIntervals)
        {
            foreach (var occupiedInterval in zoneIntervals.Values)
            {
                if (occupiedInterval.IsIntervalsIntersects(interval))
                    return true;
            }

            return false;
        }
    }
}
