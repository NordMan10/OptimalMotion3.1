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

        public Dictionary<int, Runway> Runways { get; } = new Dictionary<int, Runway>();
        public Dictionary<int, SpecialPlace> SpecialPlaces { get; } = new Dictionary<int, SpecialPlace>();
        private readonly ITable table;

        private static AircraftIdGenerator idGenerator = AircraftIdGenerator.GetInstance(ModellingParameters.StartIdValue);

        private AircraftGenerator aircraftGenerator = AircraftGenerator.GetInstance(idGenerator);

        private readonly InputDataGenerator inputDataGenerator = new InputDataGenerator(ModellingParameters.FirstTakingOffMoment);

        public event Func<List<TableRow>> AddTakingOffAircrafts;

        private int lastPlannedTakingOffMomentIndex = -1;
        private int lastPermittedMomentIndex = -1;

        
        /// <summary>
        /// Вызывает обработчик события добавления взлетающих ВС. Добавляет выходные данные в таблицу
        /// </summary>
        public void InvokeAddTakingOffAircrafts()
        {
            // Вызываем обработчик и получаем данные для заполнения таблицы
            var tableRows = AddTakingOffAircrafts?.Invoke();

            // Заполняем таблицу
            foreach(var row in tableRows)
                table.AddRow(row);
        }

        /// <summary>
        /// Обработчик события добавления взлетающих ВС
        /// </summary>
        /// <returns></returns>
        private List<TableRow> AddTakingOffAircraftsHandler()
        {
            // Получаем копию списка плановых моментов
            var plannedAircraftTakingOffMoments = InputTakingOffMoments.PlannedMoments.ToList();
            return GetOutputData(plannedAircraftTakingOffMoments);
        }



        /// <summary>
        /// Возвращает список ВС с рассчитанными фактическим моментом взлета и моментом старта, на основе переданного списка 
        /// запланированных моментов вылета (моменты по расписанию)
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        private List<TakingOffAircraft> GetConfiguredTakingOffAircrafts(List<int> plannedTakingOffMoments)
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
            var thisRunway = Runways[takingOffAircraft.RunwayId];

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
            var thisSpecialPlace = SpecialPlaces[takingOffAircraft.SpecialPlaceId];

            var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.Intervals.MotionFromPSToES -
                takingOffAircraft.Intervals.MotionFromSPToPS - takingOffAircraft.Intervals.Processing;

            var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.Intervals.Processing);
            var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

            thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

            return freeSPInterval.FirstMoment - processingInterval.FirstMoment;
        }

        private List<TakingOffAircraft> GetReconfiguredAircraftsWithReserve(List<TakingOffAircraft> takingOffAircrafts)
        {
            // Получаем список фактических моментов взлета
            var actualTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.ActualTakingOff).ToList();

            for (var i = 0; i < actualTakingOffMoments.Count; i++)
            {
                var actualMoment = actualTakingOffMoments[i];
                var nearestPermittedMoment = GetNearestPermittedMoment(actualMoment);
                if (nearestPermittedMoment == null)
                {
                    takingOffAircrafts[i].Moments.Start = -1;
                    takingOffAircrafts[i].Moments.PermittedTakingOffMoment = -1;
                    continue;
                }
                    //throw new ArgumentException("Для данного ВС не удалось найти разрешенный момент");

                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                var startDelay = verifiedPermittedMoment - actualMoment - ModellingParameters.ArrivalReserveTime;
                var currentAircraftStartMoment = takingOffAircrafts[i].Moments.Start + startDelay;

                var reserveAircraftStartMoment = GetReserveAircraftStartMoment(verifiedPermittedMoment, i, takingOffAircrafts);

                takingOffAircrafts[i].Moments.Start = currentAircraftStartMoment;
                takingOffAircrafts[i].Moments.PermittedTakingOffMoment = verifiedPermittedMoment;

                if (reserveAircraftStartMoment != null)
                {
                    takingOffAircrafts[i + 1].Moments.Start = (int)reserveAircraftStartMoment;
                    takingOffAircrafts[i + 1].Moments.PermittedTakingOffMoment = (int)verifiedPermittedMoment;
                    takingOffAircrafts[i + 1].IsReserve = true;
                    i++;
                }
            }

            return takingOffAircrafts;
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
            var orderedPermittedMoments = InputTakingOffMoments.PermittedMoments.OrderBy(m => m).ToList();
            var permittedMoments = orderedPermittedMoments.Skip(lastPermittedMomentIndex + 1).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент больше или равен фактического + резервное время прибытия => возвращаем его
                if (permittedMoment >= actualMoment + ModellingParameters.ArrivalReserveTime)
                {
                    lastPermittedMomentIndex = orderedPermittedMoments.IndexOf(permittedMoment);
                    return permittedMoment;
                }
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
                // Берем фактический момент для резервного ВС;
                var reserveAircraftActualMoment = actualTakingOffMoments[actualMomentIndex + 1];

                // Проверяем, возможно ли назначить запасной ВС
                if (permittedMoment >= reserveAircraftActualMoment + ModellingParameters.ArrivalReserveTime)
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


        private List<TableRow> GetOutputData(List<int> plannedTakingOffMoments)
        {
            var tableRows = new List<TableRow>();

            var rawPlannedTakingOffMoments = plannedTakingOffMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();
            lastPlannedTakingOffMomentIndex += rawPlannedTakingOffMoments.Count;

            var configuredTakingOffAircrafts = GetConfiguredTakingOffAircrafts(rawPlannedTakingOffMoments);
            var aircraftsWithReserve = GetReconfiguredAircraftsWithReserve(configuredTakingOffAircrafts);

            foreach(var aircraft in aircraftsWithReserve)
            {
                var aircraftTotalMotionTime = aircraft.Intervals.TakingOff + aircraft.Intervals.MotionFromPSToES;
                if (aircraft.ProcessingIsNeeded)
                    aircraftTotalMotionTime += aircraft.Intervals.MotionFromSPToPS + aircraft.Intervals.MotionFromParkingToSP;
                else
                    aircraftTotalMotionTime += aircraft.Intervals.MotionFromParkingToPS;

                tableRows.Add(new TableRow(aircraft.Id.ToString(), aircraft.Moments.PlannedTakingOff.ToString(),
                    aircraft.Moments.ActualTakingOff.ToString(), 
                    aircraft.Moments.PermittedTakingOffMoment != -1 ? aircraft.Moments.PermittedTakingOffMoment.ToString() : "Не найден",
                    aircraft.Moments.Start.ToString(), aircraftTotalMotionTime.ToString(), aircraft.Intervals.Processing.ToString(), 
                    aircraft.ProcessingIsNeeded, aircraft.IsReserve, aircraft.RunwayId.ToString(), aircraft.SpecialPlaceId.ToString()));
            }

            return tableRows;
        }

        private void InitRunways(int runwayCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < runwayCount + ModellingParameters.StartIdValue; i++)
            {
                var runway = new Runway(i);
                Runways.Add(i, runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = ModellingParameters.StartIdValue; i < specPlatformCount + ModellingParameters.StartIdValue; i++)
            {
                var specialPlace = new SpecialPlace(i);
                SpecialPlaces.Add(i, specialPlace);
            }
        }

        public void UpdateModel(int runwayCount, int specialPlaceCount)
        {
            UpdateRunways(runwayCount);
            UpdateSpecialPlaces(specialPlaceCount);
        }

        private void UpdateRunways(int runwayCount)
        {
            for (var i = Runways.Count; i < runwayCount; i++)
            {
                var runway = new Runway(i + ModellingParameters.StartIdValue);
                Runways.Add(i + ModellingParameters.StartIdValue, runway);
            }
        }

        private void UpdateSpecialPlaces(int specialPlaceCount)
        {
            for (var i = SpecialPlaces.Count; i < specialPlaceCount; i++)
            {
                var specialPlace = new SpecialPlace(i + ModellingParameters.StartIdValue);
                SpecialPlaces.Add(i + ModellingParameters.StartIdValue, specialPlace);
            }
        }

        public void ResetLastPlannedTakingOffMomentIndex()
        {
            lastPlannedTakingOffMomentIndex = -1;
        }

        public void ResetLastPermittedMomentIndex()
        {
            lastPermittedMomentIndex = -1;
        }

        //public void ResetInputMoments()
        //{
        //    var baseCount = 10;
        //    InputTakingOffMoments.PlannedMoments.RemoveRange(baseCount, InputTakingOffMoments.PlannedMoments.Count - baseCount);
        //    InputTakingOffMoments.PermittedMoments.RemoveRange(baseCount, InputTakingOffMoments.PermittedMoments.Count - baseCount);
        //}
    }
}
