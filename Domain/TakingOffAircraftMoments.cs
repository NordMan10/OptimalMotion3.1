﻿

namespace OptimalMotion3._1.Domain
{
    public class TakingOffAircraftMoments
    {
        public TakingOffAircraftMoments(int plannedTakingOff)
        {
            PlannedTakingOff = plannedTakingOff;
        }

        /// <summary>
        /// Плановый момент вылета
        /// </summary>
        public int PlannedTakingOff { get; }

        public int TakingOff { get; }
        public int Start { get; set; }
    }
}
