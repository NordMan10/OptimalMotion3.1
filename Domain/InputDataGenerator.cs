using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Domain.Enums;
using System;

namespace OptimalMotion3._1.Domain
{
    public class InputDataGenerator
    {
        public InputDataGenerator() { }

        private Random random = new Random();

        /// <summary>
        /// Возвращает набор входных данных
        /// </summary>
        /// <returns></returns>
        public InputData GetInputData(int takingOffMoment)
        {
            var runwayId = random.Next(ModellingParameters.StartIdValue, ModellingParameters.RunwayCount + 1);
            var aircraftType = (AircraftTypes)random.Next((int)AircraftTypes.Light, (int)AircraftTypes.Heavy + 1);

            var priority = GetAircraftPriority();

            return new InputData(runwayId, takingOffMoment, aircraftType, priority);
        }

        private AircraftPriorities GetAircraftPriority()
        {
            AircraftPriorities priority;

            var priorityValue = random.Next(1, 11);
            if (priorityValue == 1)
                priority = AircraftPriorities.High;
            else if (priorityValue < 4)
                priority = AircraftPriorities.Medium;
            else 
                priority = AircraftPriorities.Default;

            return priority;
        }
    }
}
