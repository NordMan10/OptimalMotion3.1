using System.Collections.Generic;
using OptimalMotion3._1.Interfaces;
using OptimalMotion3._1.Domain.Static;

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Представление предварительного старта
    /// </summary>
    public class PreliminaryStart : IMassServiceZone
    {
        public PreliminaryStart(int id)
        {
            Id = id;
        }

        public int Id { get; }

        private readonly Dictionary<int, Interval> OccupiedIntervals = new Dictionary<int, Interval>();

        public void AddAircraftInterval(int aircraftId, Interval freeInterval)
        {
            IMassServiceDeviceExtensions.AddAircraftInterval(this, aircraftId, freeInterval, OccupiedIntervals);    
        }

        public Interval GetFreeInterval(Interval newInterval)
        {
            return IMassServiceDeviceExtensions.GetFreeInterval(this, newInterval, OccupiedIntervals);
        }

        public void RemoveAircraftInterval(int aircraftId)
        {
            IMassServiceDeviceExtensions.RemoveAircraftInterval(this, aircraftId, OccupiedIntervals);
        }

        public void Reset()
        {
            OccupiedIntervals.Clear();
        }
    }
}
