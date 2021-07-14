using System.Collections.Generic;
using OptimalMotion3._1.Interfaces;
using OptimalMotion3._1.Domain.Static;

namespace OptimalMotion3._1.Domain
{
    public class PreliminaryStart : IMassServiceZone
    {
        public PreliminaryStart(int id)
        {
            Id = id;
        }

        public int Id { get; }

        private readonly Dictionary<int, Interval> OccupiedIntervals = new Dictionary<int, Interval>();

        public void AddAircraftInterval(TakingOffAircraft aircraft)
        {
            IMassServiceDeviceExtensions.AddAircraftInterval(this, aircraft, OccupiedIntervals);
        }

        public Interval GetFreeInterval(Interval newInterval)
        {
            return IMassServiceDeviceExtensions.GetFreeInterval(this, newInterval, OccupiedIntervals);
        }

        public void RemoveAircraftInterval(TakingOffAircraft aircraft)
        {
            IMassServiceDeviceExtensions.RemoveAircraftInterval(this, aircraft, OccupiedIntervals);
        }
    }
}
