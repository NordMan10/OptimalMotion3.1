using System.Collections.Generic;
using OptimalMotion3._1.Interfaces;
using OptimalMotion3._1.Domain.Static;

namespace OptimalMotion3._1.Domain
{
    public class SpecialPlace : IMassServiceZone
    {
        public SpecialPlace(int id)
        {
            Id = id;
        }

        public int Id { get; }

        private readonly Dictionary<int, Interval> ProcessingIntervals = new Dictionary<int, Interval>();

        public void AddAircraftInterval(int aircraftId, Interval freeInterval)
        {
            IMassServiceDeviceExtensions.AddAircraftInterval(this, aircraftId, freeInterval, ProcessingIntervals);
        }

        public Interval GetFreeInterval(Interval newInterval)
        {
            return IMassServiceDeviceExtensions.GetFreeInterval(this, newInterval, ProcessingIntervals);
        }

        public void RemoveAircraftInterval(int aircraftId)
        {
            IMassServiceDeviceExtensions.RemoveAircraftInterval(this, aircraftId, ProcessingIntervals);
        }

        public void Reset()
        {
            ProcessingIntervals.Clear();
        }
    }
}
