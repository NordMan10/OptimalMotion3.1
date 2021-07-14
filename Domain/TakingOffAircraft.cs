﻿using System;
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
        public TakingOffAircraft(int id, AircraftTypes type, TakingOffAircraftMoments moments, TakingOffAircraftIntervals intervals,
            bool processingIsNeeded, int runwayId, int specialPlaceId)
        {
            Id = id;
            Type = type;
            Moments = moments;
            Intervals = intervals;
            ProcessingIsNeeded = processingIsNeeded;
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
        }

        public int Id { get; }

        public AircraftTypes Type { get; }

        public TakingOffAircraftMoments Moments { get; }

        public TakingOffAircraftIntervals Intervals { get; }

        public bool ProcessingIsNeeded { get; }

        public int RunwayId { get; }

        public int SpecialPlaceId { get; }
    }
}
