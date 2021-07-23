using OptimalMotion3._1.Domain;
using System;

namespace OptimalMotion3._1.Interfaces
{
    /// <summary>
    /// Интерфейс зоны массового обслуживания, то есть зоны, через которую последовательно проходит
    /// множество клиентов (в нашем случае ВС) и имеющая определенную емкость, что подразумевает необходимость учитывать 
    /// ее занятость 
    /// </summary>
    public interface IMassServiceZone
    { 
        //IComparable Id { get; }
        
        Interval GetFreeInterval(Interval newInterval);
        
        void AddAircraftInterval(int aircraftId, Interval freeInterval);
        
        void RemoveAircraftInterval(int aircraftId);

        void Reset();
    }
}
