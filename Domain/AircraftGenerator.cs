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

<<<<<<< HEAD
        public TakingOffAircraft GetTakingOffAircraft(InputData inputData, AircraftTypes type)
=======
        /// <summary>
        /// Возвращает экземпляр взлетающего ВС
        /// </summary>
        /// <param name="inputData">Экземпляр входных данных</param>
        /// <param name="type">Тип ВС</param>
        /// <returns></returns>
        public TakingOffAircraft GetTakingOffAircraft(InputData inputData)
>>>>>>> fbc4839 (GetActualTakingOffMoments method is realized)
        {
            var id = idGenerator.GetUniqueAircraftId();

            var moments = new TakingOffAircraftMoments(inputData.PlannedTakingOffMoment);
            var intervals = GetTakingOffAircraftCreationIntervals();

            var processingIsNeededVariants = new List<bool> { true, false };
            var processingIsNeeded = processingIsNeededVariants[random.Next(0, processingIsNeededVariants.Count)];

            var specialPlaceId = DataRandomizer.GetRandomizedValue(ModellingParameters.StartIdValue, ModellingParameters.SpecialPlaceCount + 1);

            return new TakingOffAircraft(id, inputData.AircraftType, moments, intervals, processingIsNeeded, inputData.RunwayId, specialPlaceId);
        }

        /// <summary>
        /// Sets intervals, which aircraft should have if intervals will differs from each other depending on aircraft
        /// </summary>
        /// <returns>Intervals in seconds</returns>
        private TakingOffAircraftIntervals GetTakingOffAircraftCreationIntervals()
        {
            return new TakingOffAircraftIntervals(TakingOffAircraftData.MotionFromParkingToPS, TakingOffAircraftData.MotionFromPSToES, 
                TakingOffAircraftData.TakingOffInterval, TakingOffAircraftData.MotionFromParkingToSP, TakingOffAircraftData.MotionFromSPToPS, 
                TakingOffAircraftData.ProcessingTime);
        }
    }
}
