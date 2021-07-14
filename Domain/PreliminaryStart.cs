using System;
using System.Collections.Generic;
using OptimalMotion3._1.Interfaces;

namespace OptimalMotion3._1.Domain
{
    public class PreliminaryStart : IMassServiceDevice
    {
        public PreliminaryStart(int id)
        {
            Id = id;
        }

        public int Id { get; }

        private readonly Dictionary<int, Interval> OccupiedIntervals = new Dictionary<int, Interval>();

        public void AddAircraftInterval(TakingOffAircraft aircraft)
        {
            var firstOccupiedMoment = aircraft.Moments.PlannedTakingOff - aircraft.Intervals.TakingOff - aircraft.Intervals.MotionFromPSToES;
            var secondOccupiedMoment = firstOccupiedMoment + aircraft.Intervals.PSDelay;
            var occupiedInterval = new Interval(firstOccupiedMoment, secondOccupiedMoment);

            if (CheckIntervalsIntersection(occupiedInterval))
                throw new ArgumentException("Интервалы пересекаются! Передайте проверенный интервал");

            OccupiedIntervals.Add(aircraft.Id, occupiedInterval);
        }

        public Interval GetFreeInterval(Interval newInterval)
        {
            throw new NotImplementedException();
        }

        public void RemoveAircraftInterval(TakingOffAircraft aircraft)
        {
            OccupiedIntervals.Remove(aircraft.Id);
        }


        private bool CheckIntervalsIntersection(Interval interval)
        {
            foreach (var occupiedInterval in OccupiedIntervals.Values)
            {
                if (interval.LastMoment >= occupiedInterval.FirstMoment && interval.FirstMoment <= occupiedInterval.LastMoment)
                    return true;
            }

            return false;
        }
    }
}
