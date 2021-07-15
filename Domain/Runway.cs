using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Interfaces;

namespace OptimalMotion3._1.Domain
{
    public class Runway : IMassServiceZone
    {
        public Runway(int id)
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
    }
}
