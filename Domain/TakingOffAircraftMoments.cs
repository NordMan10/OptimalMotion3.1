

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

        public int PossibleTakingOff { get; set; }

        public int PermittedTakingOffMoment { get; set; }

        public int Start { get; set; }
    }
}
