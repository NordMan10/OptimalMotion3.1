using OptimalMotion3._1.Domain;
using System;

namespace OptimalMotion3._1.Interfaces
{
    public interface IMassServiceZone
    { 
        //IComparable Id { get; }
        
        Interval GetFreeInterval(Interval newInterval);
        
        void AddAircraftInterval(int aircraftId, Interval freeInterval);
        
        void RemoveAircraftInterval(int aircraftId);

        void Reset();
    }
}
