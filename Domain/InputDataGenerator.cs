using OptimalMotion3._1.Domain.Static;
using System;

namespace OptimalMotion3._1.Domain
{
    public class InputDataGenerator
    {
        public InputDataGenerator(int initTakingOffMoment)
        {
            currentTakingOffMoment = initTakingOffMoment;
        }

        private int currentTakingOffMoment;

        private Random random = new Random();

        public InputData GetInputData()
        {
            var runwayId = random.Next(ModellingParameters.StartIdValue, ModellingParameters.RunwayCount + 1);
            var takingOffMoment = currentTakingOffMoment;
            currentTakingOffMoment += ModellingParameters.TakingOffMomentStep;

            return new InputData(runwayId, takingOffMoment);
        }
    }
}
