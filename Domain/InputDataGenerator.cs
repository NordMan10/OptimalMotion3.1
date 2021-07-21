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
            var aircraftType = (AircraftTypes)random.Next((int)AircraftTypes.Light, (int)AircraftTypes.Heavy);

            return new InputData(runwayId, takingOffMoment, aircraftType);
        }
    }
}
