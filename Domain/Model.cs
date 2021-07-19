using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptimalMotion3._1.Domain
{
    public class Model
    {
        public Model(int runwayCount, int specialPlaceCount, ITable table)
        {
            InitRunways(runwayCount);
            InitSpecialPlaces(specialPlaceCount);
            this.table = table;

            AddTakingOffAircrafts += AddTakingOffAircraftsHandler;
        }

        private Dictionary<int, Runway> runways = new Dictionary<int, Runway>();
        private Dictionary<int, SpecialPlace> specialPlaces = new Dictionary<int, SpecialPlace>();
        private readonly ITable table;

        private static AircraftIdGenerator idGenerator = AircraftIdGenerator.GetInstance(ModellingParameters.StartIdValue);

        private AircraftGenerator aircraftGenerator = AircraftGenerator.GetInstance(idGenerator);

        private readonly InputDataGenerator inputDataGenerator = new InputDataGenerator(ModellingParameters.FirstTakingOffMoment);

        public event Func<TableRow> AddTakingOffAircrafts;


        public void UpdateModel(int runwayCount, int specialPlaceCount)
        {
            UpdateRunways(runwayCount);
            UpdateSpecialPlaces(specialPlaceCount);
        }

        public void InvokeAddTakingOffAircraft()
        {
            var tableRow = AddTakingOffAircrafts?.Invoke();
            table.AddRow(tableRow);
        }

        private TableRow AddTakingOffAircraftsHandler()
        {
            return GetOutputData();
        }

        /// <summary>
        /// Возвращает список ВС с рассчитанными фактическим моментом взлета и моментом старта, на основе переданного списка 
        /// запланированных моментов вылета (моменты по расписанию)
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        public List<TakingOffAircraft> GetConfiguredTakingOffAircrafts(List<int> plannedTakingOffMoments)
        {
            var takingOffAircrafts= new List<TakingOffAircraft>();

            for (var i = 0; i < plannedTakingOffMoments.Count; i++)
            {
                // Генерируем входные данные для нового ВС
                var inputData = inputDataGenerator.GetInputData(plannedTakingOffMoments[i]);
                // Создаем ВС
                var takingOffAircraft = aircraftGenerator.GetTakingOffAircraft(inputData);
                int startMoment;

                // Рассчитываем интервал взлета
                var takingOffInterval = new Interval(inputData.PlannedTakingOffMoment - takingOffAircraft.Intervals.TakingOff, inputData.PlannedTakingOffMoment);

                // Получаем задержку
                var startDelay = GetRunwayStartDelay(takingOffAircraft, takingOffInterval);

                // Если нужна обработка
                if (takingOffAircraft.ProcessingIsNeeded)
                {
                    // Получаем задержку
                    startDelay += GetSpecialPlaceStartDelay(takingOffAircraft, takingOffInterval);

                    // Рассчитываем и задаем момент старта при необходимости обработки
                    var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
                        takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

                    startMoment = SPArriveMoment - takingOffAircraft.Intervals.MotionFromParkingToSP + startDelay - ModellingParameters.ArrivalReserveTime;
                }
                else
                {
                    // Рассчитываем и задаем момент старта при отсутствии необходимости обработки
                    startMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES - takingOffAircraft.Intervals.MotionFromParkingToPS +
                        startDelay - ModellingParameters.ArrivalReserveTime;
                }

                // Задаем рассчитанный момент старта текущему ВС
                takingOffAircraft.Moments.Start = startMoment;
                // Рассчитываем и задаем фактический момент взлета
                takingOffAircraft.Moments.ActualTakingOff = takingOffAircraft.Moments.PlannedTakingOff + startDelay;

                takingOffAircrafts.Add(takingOffAircraft);
            }

            return takingOffAircrafts;
        }

        /// <summary>
        /// Возвращает задержку момента старта от ВПП
        /// </summary>
        /// <param name="takingOffAircraft"></param>
        /// <param name="takingOffInterval"></param>
        /// <returns></returns>
        private int GetRunwayStartDelay(TakingOffAircraft takingOffAircraft, Interval takingOffInterval)
        {
            var thisRunway = runways[takingOffAircraft.RunwayId];

            // Получаем свободный интервал от ВПП
            var freeRunwayInterval = thisRunway.GetFreeInterval(takingOffInterval);
            // Добавляем полученный новый интервал в ВПП
            thisRunway.AddAircraftInterval(takingOffAircraft.Id, freeRunwayInterval);

            // Рассчитываем и возвращаем задержку
            return freeRunwayInterval.FirstMoment - takingOffInterval.FirstMoment;
        }

        /// <summary>
        /// Возвращает задержку момента старта от Спец. площадки
        /// </summary>
        /// <param name="takingOffAircraft"></param>
        /// <param name="takingOffInterval"></param>
        /// <returns></returns>
        private int GetSpecialPlaceStartDelay(TakingOffAircraft takingOffAircraft, Interval takingOffInterval)
        {
            var thisSpecialPlace = specialPlaces[takingOffAircraft.SpecialPlaceId];

            var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
                takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

            var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.Intervals.Processing);
            var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

            thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

            return freeSPInterval.FirstMoment - processingInterval.FirstMoment;
        }

        public void GetReconfiguredAircraftsWithReserve(List<TakingOffAircraft> takingOffAircrafts)
        {
            // Получаем список фактических моментов взлета
            var actualTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.ActualTakingOff).ToList();

            for (var i = 0; i < actualTakingOffMoments.Count; i++)
            {
                var actualMoment = actualTakingOffMoments[i];
                var nearestPermittedMoment = GetNearestPermittedMoment(actualMoment);
                if (nearestPermittedMoment == null)
                    throw new ArgumentException("Для данного ВС не удалось найти разрешенный момент");

                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                var startDelay = verifiedPermittedMoment - actualMoment - ModellingParameters.ArrivalReserveTime;
                var currentAircraftStartMoment = takingOffAircrafts[i].Moments.Start + startDelay;

                var reserveAircraftStartMoment = GetReserveAircraftStartMoment(verifiedPermittedMoment, i, takingOffAircrafts);

                takingOffAircrafts[i].Moments.Start = currentAircraftStartMoment;

                if (reserveAircraftStartMoment != null)
                {
                    takingOffAircrafts[i + 1].Moments.Start = (int)reserveAircraftStartMoment;
                    i++;
                }
            }
        }

        /// <summary>
        /// Возвращаем ближайший разрешенный момент для переданного фактического момента, если его возможно установить. 
        /// Если невозможно, возвращает null
        /// </summary>
        /// <param name="actualMoment"></param>
        /// <returns></returns>
        private int? GetNearestPermittedMoment(int actualMoment)
        {
            // Упорядочиваем разрешенные моменты
            var permittedMoments = InputTakingOffMomemts.PermittedMoments.OrderBy(m => m).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент больше или равен фактического + резервное время прибытия => возвращаем его
                if (permittedMoment >= actualMoment + ModellingParameters.ArrivalReserveTime)
                    return permittedMoment;
            }

            return null;
        }

        /// <summary>
        /// Возвращает момент старта для резервного ВС, если его возможно установить. Если невозможно, возвращает null
        /// </summary>
        /// <param name="permittedMoment"></param>
        /// <param name="actualMomentIndex"></param>
        /// <param name="takingOffAircrafts"></param>
        /// <returns></returns>
        private int? GetReserveAircraftStartMoment(int permittedMoment, int actualMomentIndex, List<TakingOffAircraft> takingOffAircrafts)
        {
            // Получаем список фактический моментов взлета
            var actualTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.ActualTakingOff).ToList();

            if (actualMomentIndex < actualTakingOffMoments.Count - 1)
            {
                // Берем фактический и разрешенный моменты;
                var actualMoment = actualTakingOffMoments[actualMomentIndex];
                var reserveAircraftActualMoment = actualTakingOffMoments[actualMomentIndex + 1];

                // Проверяем, возможно ли назначить запасной ВС
                if (permittedMoment >= actualMoment + ModellingParameters.ArrivalReserveTime)
                {
                    // Если возможно, то рассчитываем момент старта для следующего ВС с учетом запаса по времени
                    var startDelay = permittedMoment - reserveAircraftActualMoment - ModellingParameters.ArrivalReserveTime;
                    var reserveAircraftStartMoment = takingOffAircrafts[actualMomentIndex + 1].Moments.Start + startDelay;

                    // Возвращаем момент старта
                    return reserveAircraftStartMoment;
                }
            }

            // Если невозможно назначить запасной ВС, то возвращаем null
            return null;
        }


        //public TableRow GetOutputData()
        //{
        //    var inputData = inputDataGenerator.GetInputData(1);
        //    var takingOffAircraft = aircraftGenerator.GetTakingOffAircraft(inputData, Enums.AircraftTypes.Medium);
        //    var thisRunway = runways[takingOffAircraft.RunwayId];
        //    int startMoment;

        //    var takingOffInterval = new Interval(inputData.PlannedTakingOffMoment - takingOffAircraft.Intervals.TakingOff, inputData.PlannedTakingOffMoment);
        //    var freeRunwayInterval = thisRunway.GetFreeInterval(takingOffInterval);
            
        //    var delay = freeRunwayInterval.FirstMoment - takingOffInterval.FirstMoment;

        //    thisRunway.AddAircraftInterval(takingOffAircraft.Id, freeRunwayInterval);

        //    if (takingOffAircraft.ProcessingIsNeeded)
        //    {
        //        var thisSpecialPlace = specialPlaces[takingOffAircraft.SpecialPlaceId];

        //        var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
        //            takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

        //        var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.Intervals.Processing);
        //        var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

        //        delay += freeSPInterval.FirstMoment - processingInterval.FirstMoment;

        //        thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

        //        startMoment = SPArriveMoment - takingOffAircraft.Intervals.MotionFromParkingToSP + delay;
        //    }
        //    else
        //    {
        //        startMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES - takingOffAircraft.Intervals.MotionFromParkingToPS +
        //            delay;
        //    }

        //    takingOffAircraft.Moments.Start = startMoment;

        //    var actualTakingOffMoment = takingOffAircraft.Moments.PlannedTakingOff + delay;

        //    return new TableRow(takingOffAircraft.Id.ToString(), actualTakingOffMoment.ToString(), takingOffAircraft.Moments.PlannedTakingOff.ToString(), 
        //        takingOffAircraft.ProcessingIsNeeded, takingOffAircraft.RunwayId.ToString(), takingOffAircraft.SpecialPlaceId.ToString());
        //}

        private void InitRunways(int runwayCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < runwayCount + ModellingParameters.StartIdValue; i++)
            {
                var runway = new Runway(i);
                runways.Add(i, runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < specPlatformCount + ModellingParameters.StartIdValue; i++)
            {
                var specialPlace = new SpecialPlace(i);
                specialPlaces.Add(i, specialPlace);
            }
        }

        private void UpdateRunways(int runwayCount)
        {
            for (var i = runways.Count; i < runwayCount; i++)
            {
                var runway = new Runway(i + ModellingParameters.StartIdValue);
                runways.Add(i + ModellingParameters.StartIdValue, runway);
            }
        }

        private void UpdateSpecialPlaces(int specialPlaceCount)
        {
            for (var i = specialPlaces.Count; i < specialPlaceCount; i++)
            {
                var specialPlace = new SpecialPlace(i + ModellingParameters.StartIdValue);
                specialPlaces.Add(i + ModellingParameters.StartIdValue, specialPlace);
            }
        }
    }
}
