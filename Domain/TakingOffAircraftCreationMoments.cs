

namespace OptimalMotion3._1.Domain
{
    public class TakingOffAircraftCreationMoments
    {
        public TakingOffAircraftCreationMoments(int plannedTakingOff)
        {
            PlannedTakingOff = plannedTakingOff;
        }

        /// <summary>
        /// Плановый момент вылета
        /// </summary>
        public int PlannedTakingOff { get; }
    }
}
