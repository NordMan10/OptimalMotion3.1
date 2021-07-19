using OptimalMotion3._1.Domain;

namespace OptimalMotion3._1.Interfaces
{
    public interface IMassServiceZone
    { 
        int Id { get; }
        
        Interval GetFreeInterval(Interval newInterval);
        
        void AddAircraftInterval(int aircraftId, Interval freeInterval);
        
        void RemoveAircraftInterval(int aircraftId);

        void Reset();
    }
}
