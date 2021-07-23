using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain
{
    public class AircraftInputDataGenerator
    {
        protected AircraftInputDataGenerator()
        {
        }

        private readonly Random random = new Random();
        private static AircraftInputDataGenerator _instance;
        private static readonly object SyncRoot = new object();


        public static AircraftInputDataGenerator GetInstance()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new AircraftInputDataGenerator();
                }
            }
            return _instance;
        }

        /// <summary>
        /// Возвращает набор входных данных для кокретного планового момента
        /// </summary>
        /// <returns></returns>
        public AircraftInputData GetAircraftInputData(int plannedTakingOffMoment)
        {
            var runwayId = random.Next(ProgramConstants.StartIdValue, CommonInputData.RunwayCount + 1).ToString();
            var specialPlaceId = DataRandomizer.GetRandomizedValue(ProgramConstants.StartIdValue, CommonInputData.SpecialPlaceCount + 1);

            var aircraftType = random.Next((int)AircraftTypes.Light, (int)AircraftTypes.Heavy + 1).ToString();

            var priority = GetAircraftPriority();

            var processingIsNeeded = GetProcessingNecessity();

            var creationMoments = new TakingOffAircraftCreationMoments(plannedTakingOffMoment);
            var creationIntervals = GetTakingOffAircraftCreationIntervals();

            return new AircraftInputData(runwayId, specialPlaceId, aircraftType, priority, processingIsNeeded, creationIntervals, creationMoments);
        }


        private int GetAircraftPriority()
        {
            int priority;

            var priorityValue = random.Next(1, 11);
            if (priorityValue == 1)
                priority = (int)AircraftPriorities.High;
            else if (priorityValue < 4)
                priority = (int)AircraftPriorities.Medium;
            else 
                priority = (int)AircraftPriorities.Default;

            return priority;
        }

        private bool GetProcessingNecessity()
        {
            var processingIsNeededVariants = new List<bool> { true, false };
            return processingIsNeededVariants[random.Next(0, processingIsNeededVariants.Count)];
        }

        private TakingOffAircraftCreationIntervals GetTakingOffAircraftCreationIntervals()
        {
            var motionFromParkingToPS = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.MotionFromParkingToPS, 25, 15);
            var motionFromPSToES = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.MotionFromPSToES, 25, 5);
            var takingOffInterval = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.TakingOffInterval, 0, 5);
            var motionFromParkingToSP = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.MotionFromParkingToSP, 25, 15);
            var motionFromSPToPS = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.MotionFromSPToPS, 25, 15);
            var processingTime = DataRandomizer.GetRandomizedValue(ConstantTakingOffCreationIntervals.ProcessingTime, 25, 30);

            return new TakingOffAircraftCreationIntervals(motionFromParkingToPS, motionFromPSToES, takingOffInterval, motionFromParkingToSP,
                motionFromSPToPS, processingTime);
        }
    }
}
