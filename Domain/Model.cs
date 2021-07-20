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
        /// Возвращает список ВС с рассчитанными возможным моментом взлета и моментом старта, на основе переданного списка 
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
                // Рассчитываем и задаем возможный момент взлета
                takingOffAircraft.Moments.PossibleTakingOff = takingOffAircraft.Moments.PlannedTakingOff + startDelay;

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
            var possibleTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.PossibleTakingOff).OrderBy(i => i).ToList();

            for (var i = 0; i < possibleTakingOffMoments.Count; i++)
            {
                var possibleMoment = possibleTakingOffMoments[i];
                var nearestPermittedMoment = GetNearestPermittedMoment(possibleMoment);
                if (nearestPermittedMoment == null)
                {
                    takingOffAircrafts[i].Moments.Start = -1;
                    takingOffAircrafts[i].Moments.PermittedTakingOffMoment = -1;
                    continue;
                }
                    //throw new ArgumentException("Для данного ВС не удалось найти разрешенный момент");

                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                var startDelay = verifiedPermittedMoment - possibleMoment;
                var currentAircraftStartMoment = takingOffAircrafts[i].Moments.Start + startDelay;

                var reserveAircraftStartMoments = GetReserveAircraftStartMoments(verifiedPermittedMoment, i, takingOffAircrafts);

                takingOffAircrafts[i].Moments.Start = currentAircraftStartMoment;
                takingOffAircrafts[i].Moments.PermittedTakingOffMoment = verifiedPermittedMoment;

                for (var j = 1; j <= reserveAircraftStartMoments.Count; j++)
                {
                    takingOffAircrafts[i + j].Moments.Start = reserveAircraftStartMoments[j - 1];
                    takingOffAircrafts[i + j].Moments.PermittedTakingOffMoment = verifiedPermittedMoment;
                    takingOffAircrafts[i + j].IsReserve = true;
                }

                i += reserveAircraftStartMoments.Count;
            }

            return takingOffAircrafts;
        }

        /// <summary>
        /// Возвращаем ближайший разрешенный момент для переданного возможного момента, если его возможно установить. 
        /// Если невозможно, возвращает null
        /// </summary>
        /// <param name="possibleMoment"></param>
        /// <returns></returns>
        private int? GetNearestPermittedMoment(int possibleMoment)
        {
            // Упорядочиваем разрешенные моменты
            var orderedPermittedMoments = InputTakingOffMoments.PermittedMoments.OrderBy(m => m).ToList();
            // Выбираем только те, что еще не были использованы
            var permittedMoments = orderedPermittedMoments.Skip(lastPermittedMomentIndex + 1).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент больше или равен возможному + резервное время прибытия => возвращаем его
                if (permittedMoment >= possibleMoment + ModellingParameters.ArrivalReserveTime)
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
        /// <param name="possibleMomentIndex"></param>
        /// <param name="takingOffAircrafts"></param>
        /// <returns></returns>
        private List<int> GetReserveAircraftStartMoments(int permittedMoment, int possibleMomentIndex, List<TakingOffAircraft> takingOffAircrafts)
        {
            var reserveStartMoments = new List<int>();

            // Получаем список возможных моментов взлета
            var possibleTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.PossibleTakingOff).OrderBy(i => i).ToList();

            // Проверяем, есть ли еще возможные моменты
            if (possibleMomentIndex < possibleTakingOffMoments.Count - 1)
            {
                // Определяем необходимое количество резервных ВС
                var reserveAircraftCount = GetReserveAircraftCount(permittedMoment, possibleMomentIndex, possibleTakingOffMoments);

                for (var i = 1; i <= reserveAircraftCount; i++)
                {
                    // Проверяем, есть ли еще возможные моменты
                    if (possibleMomentIndex + i < possibleTakingOffMoments.Count - 1)
                    {
                        // Берем возможный момент для резервного ВС;
                        var reserveAircraftPossibleMoment = possibleTakingOffMoments[possibleMomentIndex + i];

                        // Рассчитываем задержку для момента старта резервного ВС
                        var startDelay = permittedMoment - reserveAircraftPossibleMoment;
                        // Задаем момент старта для резервного ВС
                        var reserveAircraftStartMoment = takingOffAircrafts[possibleMomentIndex + i].Moments.Start + startDelay;

                        // Добавляем момент старта
                        reserveStartMoments.Add(reserveAircraftStartMoment);
                    }
                }
            }
            
            // Возаращаем либо пустой список, либо заполненный старовыми моментами
            return reserveStartMoments;
        }

        private int GetReserveAircraftCount(int permittedMoment, int possibleMomentIndex, List<int> possibleTakingOffMoments)
        {
            var reserveAircraftCount = 0;

            var index = 1;
            while (possibleMomentIndex + index < possibleTakingOffMoments.Count - 1 && 
                permittedMoment >= possibleTakingOffMoments[possibleMomentIndex + index] + ModellingParameters.ArrivalReserveTime)
            {
                reserveAircraftCount++;
                if (reserveAircraftCount > 1)
                {
                    var t = 1;
                }
                index++;
            }

            int timeToLastTakeOffMoment;
            int permittedTime;
            do
            {
                timeToLastTakeOffMoment = possibleTakingOffMoments[possibleMomentIndex + reserveAircraftCount] - possibleTakingOffMoments[possibleMomentIndex];
                permittedTime = ModellingParameters.ReserveAircraftCount.
                        Where(item => reserveAircraftCount <= item.Value).OrderBy(i => i.Value).First().Key;

                if (timeToLastTakeOffMoment > permittedTime)
                    reserveAircraftCount--;
            }
            while (timeToLastTakeOffMoment > permittedTime);

            return reserveAircraftCount;
        }


        private List<TableRow> GetOutputData(List<int> plannedTakingOffMoments)
        {
            var tableRows = new List<TableRow>();

            var rawPlannedTakingOffMoments = plannedTakingOffMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();
            lastPlannedTakingOffMomentIndex += rawPlannedTakingOffMoments.Count;

            var configuredTakingOffAircrafts = GetConfiguredTakingOffAircrafts(rawPlannedTakingOffMoments);
            var aircraftsWithReserve = GetReconfiguredAircraftsWithReserve(configuredTakingOffAircrafts);

            var aircraftsOrderedByPermittedMoments = aircraftsWithReserve.OrderBy(a => a.Moments.PermittedTakingOffMoment).ToList();
            foreach(var aircraft in aircraftsOrderedByPermittedMoments)
            {
                tableRows.Add(GetTableRow(aircraft));
            }

            return tableRows;
        }

        private TableRow GetTableRow(TakingOffAircraft aircraft)
        {
            var aircraftTotalMotionTime = aircraft.Intervals.TakingOff + aircraft.Intervals.MotionFromPSToES;
            if (aircraft.ProcessingIsNeeded)
                aircraftTotalMotionTime += aircraft.Intervals.MotionFromSPToPS + aircraft.Intervals.MotionFromParkingToSP;
            else
                aircraftTotalMotionTime += aircraft.Intervals.MotionFromParkingToPS;

            var permittedMoment = aircraft.Moments.PermittedTakingOffMoment != -1 ? aircraft.Moments.PermittedTakingOffMoment.ToString() : "Не найден";
            var processingTime = aircraft.ProcessingIsNeeded ? aircraft.Intervals.Processing.ToString() : "-";
            var specialPlaceId = aircraft.ProcessingIsNeeded ? aircraft.SpecialPlaceId.ToString() : "-";

            return new TableRow(aircraft.Id.ToString(), aircraft.Moments.PlannedTakingOff.ToString(), aircraft.Moments.PossibleTakingOff.ToString(),
                    permittedMoment, aircraft.Moments.Start.ToString(), aircraftTotalMotionTime.ToString(), processingTime,
                    aircraft.ProcessingIsNeeded, aircraft.IsReserve, aircraft.RunwayId.ToString(), specialPlaceId);
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
