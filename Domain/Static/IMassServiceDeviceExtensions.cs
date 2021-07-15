using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain.Static
{
    public static class IMassServiceDeviceExtensions
    {
        public static void AddAircraftInterval(this IMassServiceZone zone, int aircraftId, Interval freeInterval, Dictionary<int, Interval> zoneIntervals)
        {
            if (CheckIntervalsIntersection(freeInterval, zoneIntervals))
                throw new ArgumentException("Интервалы пересекаются! Передайте проверенный интервал");

            zoneIntervals.Add(aircraftId, freeInterval);
        }

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

        public static void RemoveAircraftInterval(this IMassServiceZone zone, int aircraftId, Dictionary<int, Interval> zoneIntervals)
        {
            zoneIntervals.Remove(aircraftId);
        }


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
