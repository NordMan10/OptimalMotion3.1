using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Класс для генерации входных данных для каждого ВС. Синглтон
    /// </summary>
    public class AircraftInputDataGenerator
    {
        protected AircraftInputDataGenerator() {}

        private readonly Random random = new Random();
        private static AircraftInputDataGenerator _instance;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Возвращает экземпляр класса. Если экземпляр уже был создан, возвращает ссылку на него
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает значение приритета для ВС с неравномерным распределением
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает флаг необходимости обработки, генерируемый случайным образом
        /// </summary>
        /// <returns></returns>
        private bool GetProcessingNecessity()
        {
            var processingIsNeededVariants = new List<bool> { true, false };
            return processingIsNeededVariants[random.Next(0, processingIsNeededVariants.Count)];
        }

        /// <summary>
        /// Возвращает экземпляр класса заданных интервалов для ВС (нужен, пока данные для каждого ВС не будут передаваться напрямую).
        /// Значения интервалов задаются с некоторым разбросом от среднего значения
        /// </summary>
        /// <returns></returns>
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
