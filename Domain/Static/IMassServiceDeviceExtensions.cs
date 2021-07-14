using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain.Static
{
    public static class IMassServiceDeviceExtensions
    {
        public static void AddAircraftInterval(this IMassServiceZone zone,  TakingOffAircraft aircraft, Dictionary<int, Interval> zoneIntervals)
        {
            var firstOccupiedMoment = aircraft.Moments.PlannedTakingOff - aircraft.Intervals.TakingOff - aircraft.Intervals.MotionFromPSToES;
            var secondOccupiedMoment = firstOccupiedMoment + aircraft.Intervals.PSDelay;
            var occupiedInterval = new Interval(firstOccupiedMoment, secondOccupiedMoment);

            if (CheckIntervalsIntersection(occupiedInterval, zoneIntervals))
                throw new ArgumentException("Интервалы пересекаются! Передайте проверенный интервал");

            zoneIntervals.Add(aircraft.Id, occupiedInterval);
        }

        public static Interval GetFreeInterval(this IMassServiceZone zone, Interval newInterval, Dictionary<int, Interval> zoneIntervals)
        {
            var delay = 0;
            foreach (var occupiedInterval in zoneIntervals.Values)
            {
                if (newInterval.LastMoment >= occupiedInterval.FirstMoment && newInterval.FirstMoment <= occupiedInterval.LastMoment)
                {
                    delay = occupiedInterval.LastMoment - newInterval.FirstMoment;
                }
            }

            return new Interval(newInterval.FirstMoment + delay, newInterval.LastMoment + delay);
        }

        public static void RemoveAircraftInterval(this IMassServiceZone zone, TakingOffAircraft aircraft, Dictionary<int, Interval> zoneIntervals)
        {
            zoneIntervals.Remove(aircraft.Id);
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
