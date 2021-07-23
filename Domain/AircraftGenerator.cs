using OptimalMotion3._1.Domain.Static;
using System;

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
        public TakingOffAircraft GetTakingOffAircraft(AircraftInputData inputData)
        {
            var id = idGenerator.GetUniqueAircraftId();

            return new TakingOffAircraft(id, inputData.Type, inputData.Priority, inputData.CreationMoments, 
                inputData.CreationIntervals, inputData.ProcessingIsNeeded, inputData.RunwayId, inputData.SpecialPlaceId);
        }
    }
}
