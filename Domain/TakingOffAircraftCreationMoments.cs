

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Моменты, получаемые во входных данных для взлетающего ВС
    /// </summary>
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
