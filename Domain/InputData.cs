using OptimalMotion3._1.Domain.Enums;

namespace OptimalMotion3._1.Domain
{
    public class InputData
    {
        public InputData(int runwayId, int plannedTakingOffMoment, AircraftTypes aircraftType, AircraftPriorities priority)
        {
            RunwayId = runwayId;
            PlannedTakingOffMoment = plannedTakingOffMoment;
            AircraftType = aircraftType;
            Priority = priority;
        }

        /// <summary>
        /// Количество ВПП
        /// </summary>
        public int RunwayId { get; }

        /// <summary>
        /// Плановый момент вылета
        /// </summary>
        public int PlannedTakingOffMoment { get; }

        /// <summary>
        /// Тип ВС
        /// </summary>
        public AircraftTypes AircraftType { get; }

        /// <summary>
        /// Приоритет ВС
        /// </summary>
        public AircraftPriorities Priority { get; }
    }
}
