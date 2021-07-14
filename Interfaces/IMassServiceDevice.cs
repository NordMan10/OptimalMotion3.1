using OptimalMotion3._1.Domain;

namespace OptimalMotion3._1.Interfaces
{
    public interface IMassServiceDevice
    {
        int Id { get; }
        Interval GetFreeInterval(Interval newInterval);
        void AddAircraftInterval(TakingOffAircraft aircraft);
        void RemoveAircraftInterval(TakingOffAircraft aircraft);
    }
}
