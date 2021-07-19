using OptimalMotion3._1.Domain.Enums;
using OptimalMotion3._1.Domain.Static;
using System;
using System.Collections.Generic;

namespace OptimalMotion3._1.Domain
{
    public class AircraftGenerator
    {
        protected AircraftGenerator(AircraftIdGenerator idGenerator)
        {
            this.idGenerator = idGenerator;
        }

        private readonly AircraftIdGenerator idGenerator;
        private readonly Random random = new Random();
        private static AircraftGenerator _instance;
        private static readonly object SyncRoot = new object();


        public static AircraftGenerator GetInstance(AircraftIdGenerator idGenerator)
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new AircraftGenerator(idGenerator);
                }
            }
            return _instance;
        }


        /// <summary>
        /// Возвращает экземпляр взлетающего ВС
        /// </summary>
        /// <param name="inputData">Экземпляр входных данных</param>
        /// <param name="type">Тип ВС</param>
        /// <returns></returns>
        public TakingOffAircraft GetTakingOffAircraft(InputData inputData)
        {
            var id = idGenerator.GetUniqueAircraftId();

            var moments = new TakingOffAircraftMoments(inputData.PlannedTakingOffMoment);
            var intervals = GetTakingOffAircraftIntervals();

            var processingIsNeededVariants = new List<bool> { true };
            var processingIsNeeded = processingIsNeededVariants[random.Next(0, processingIsNeededVariants.Count)];

            var specialPlaceId = DataRandomizer.GetRandomizedValue(ModellingParameters.StartIdValue, ModellingParameters.SpecialPlaceCount + 1);

            return new TakingOffAircraft(id, inputData.AircraftType, moments, intervals, processingIsNeeded, inputData.RunwayId, specialPlaceId);
        }

        /// <summary>
        /// Возвращает набор заданных интервалов
        /// </summary>
        /// <returns></returns>
        private TakingOffAircraftIntervals GetTakingOffAircraftIntervals()
        {
            var motionFromParkingToPS = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.MotionFromParkingToPS, 25, 15);
            var motionFromPSToES = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.MotionFromPSToES, 25, 5);
            var takingOffInterval = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.TakingOffInterval, 20, 5);
            var motionFromParkingToSP = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.MotionFromParkingToSP, 25, 15);
            var motionFromSPToPS = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.MotionFromSPToPS, 25, 15);
            var processingTime = DataRandomizer.GetRandomizedValue(TakingOffAircraftData.ProcessingTime, 25, 30);

            return new TakingOffAircraftIntervals(motionFromParkingToPS, motionFromPSToES, takingOffInterval, motionFromParkingToSP, 
                motionFromSPToPS, processingTime);
        }
    }
}
