using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptimalMotion3._1.Domain.Enums;
using OptimalMotion3._1.Interfaces;

namespace OptimalMotion3._1.Domain
{
    public class TakingOffAircraft : IAircraft
    {
        public TakingOffAircraft(int id, AircraftTypes type, AircraftPriorities priority, TakingOffAircraftCreationMoments creationMoments, 
            TakingOffAircraftCreationIntervals creationIntervals, bool processingIsNeeded, int runwayId, int specialPlaceId)
        {
            Id = id;
            Type = type;
            Priority = priority;
            CreationMoments = creationMoments;
            CreationIntervals = creationIntervals;
            ProcessingIsNeeded = processingIsNeeded;
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
        }

        public int Id { get; }

        public AircraftTypes Type { get; }

        public AircraftPriorities Priority { get; }

        public TakingOffAircraftCreationMoments CreationMoments { get; }
        public TakingOffAircraftCalculatingMoments CalculatingMoments { get; } = new TakingOffAircraftCalculatingMoments();

        public TakingOffAircraftCreationIntervals CreationIntervals { get; }
        public TakingOffAircraftCalculatingIntervals CalculatingIntervals { get; } = new TakingOffAircraftCalculatingIntervals();

        public bool ProcessingIsNeeded { get; }

        public bool IsReserve { get; set; }

        public int RunwayId { get; }

        public int SpecialPlaceId { get; }
    }
}
