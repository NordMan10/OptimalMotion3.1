using OptimalMotion3._1.Domain.Static;
using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly InputDataGenerator inputDataGenerator = new InputDataGenerator();

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
        /// Возвращает список ВС с рассчитанными возможным моментом взлета и моментом старта и упорядоченный по возможным моментам
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        private List<TakingOffAircraft> GetOrderedConfiguredTakingOffAircrafts(List<int> plannedTakingOffMoments)
        {
            var takingOffAircrafts= new List<TakingOffAircraft>();
            var orderedPlannedTakingOffMoments = plannedTakingOffMoments.OrderBy(m => m).ToList();

            for (var i = 0; i < orderedPlannedTakingOffMoments.Count; i++)
            {
                // Генерируем входные данные для нового ВС
                var inputData = inputDataGenerator.GetInputData(orderedPlannedTakingOffMoments[i]);
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

            var orderedTakingOffAircrafts = takingOffAircrafts.OrderBy(a => a.Moments.PossibleTakingOff).ToList();
            return orderedTakingOffAircrafts;
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
            var possibleTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.PossibleTakingOff).ToList();

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

                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                var startDelay = verifiedPermittedMoment - possibleMoment;
                var currentAircraftStartMoment = takingOffAircrafts[i].Moments.Start + startDelay;

                var reserveAircraftStartMoments = GetReserveAircraftStartMoments(verifiedPermittedMoment, i, takingOffAircrafts);

                // Создаем список моментов старта текущего и резервных ВС
                var allStartMoments = new List<int> { currentAircraftStartMoment };
                allStartMoments.AddRange(reserveAircraftStartMoments);
                // Задаем моменты старта для текущего и резервных ВС
                SetStartMoments(takingOffAircrafts, i, allStartMoments);

                var mostPriorityAircraftIndex = GetMostPriorityAircraftIndex(takingOffAircrafts, i, allStartMoments.Count);

                for (var j = 0; j < allStartMoments.Count; j++)
                {
                    if (takingOffAircrafts[i].RunwayId != takingOffAircrafts[i + j].RunwayId)
                    {
                        var t = 5;
                    }
                    takingOffAircrafts[i + j].Moments.PermittedTakingOffMoment = verifiedPermittedMoment;
                    if (i + j != mostPriorityAircraftIndex)
                        takingOffAircrafts[i + j].IsReserve = true;
                }

                i += reserveAircraftStartMoments.Count;
                lastPermittedMomentIndex += reserveAircraftStartMoments.Count;
            }

            return takingOffAircrafts;
        }

        private void SetStartMoments(List<TakingOffAircraft> takingOffAircrafts, int firstIndex, List<int> aircraftStartMoments)
        {
            for (var i = 0; i < aircraftStartMoments.Count; i++)
            {
                takingOffAircrafts[firstIndex + i].Moments.Start = aircraftStartMoments[i];
            }
        }

        private int GetMostPriorityAircraftIndex(List<TakingOffAircraft> takingOffAircrafts, int firstIndex, int aircraftCount)
        {
            var mostPriorityAircraftIndex = firstIndex;
            for (var i = 1; i < aircraftCount; i++)
            {
                if (takingOffAircrafts[firstIndex + i].Priority > takingOffAircrafts[mostPriorityAircraftIndex].Priority)
                    mostPriorityAircraftIndex = firstIndex + i;
            }

            return mostPriorityAircraftIndex;
        }

        private void SetPSWaitingTime(List<TakingOffAircraft> takingOffAircrafts)
        {
            foreach (var aircraft in takingOffAircrafts)
            {
                int arrivalToPSMoment;
                if (aircraft.ProcessingIsNeeded)
                {
                    arrivalToPSMoment = aircraft.Moments.Start + aircraft.Intervals.MotionFromParkingToSP + aircraft.Intervals.Processing +
                    aircraft.Intervals.MotionFromSPToPS;
                }
                else
                    arrivalToPSMoment = aircraft.Moments.Start + aircraft.Intervals.MotionFromParkingToPS;

                aircraft.PSWaitingTime = aircraft.Moments.PermittedTakingOffMoment - arrivalToPSMoment - aircraft.Intervals.MotionFromPSToES - 
                    aircraft.Intervals.TakingOff;

                if (aircraft.PSWaitingTime > 30)
                {
                    var t = 5;
                }
            }
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
        /// Возвращает список моментов старта для резервных ВС, если их возможно задать. Если невозможно, возвращает пустой список
        /// </summary>
        /// <param name="permittedMoment"></param>
        /// <param name="possibleMomentIndex"></param>
        /// <param name="takingOffAircrafts"></param>
        /// <returns></returns>
        private List<int> GetReserveAircraftStartMoments(int permittedMoment, int possibleMomentIndex, List<TakingOffAircraft> takingOffAircrafts)
        {
            var reserveStartMoments = new List<int>();

            // Получаем список возможных моментов взлета
            var possibleTakingOffMoments = takingOffAircrafts.Select(a => a.Moments.PossibleTakingOff).ToList();

            // Проверяем, есть ли еще возможные моменты
            if (possibleMomentIndex < possibleTakingOffMoments.Count - 1)
            {
                // Определяем допустимое количество резервных ВС
                var reserveAircraftCount = GetReserveAircraftCount(permittedMoment, possibleMomentIndex, possibleTakingOffMoments);

                for (var i = 1; i <= reserveAircraftCount; i++)
                {
                    // Проверяем, есть ли еще возможные моменты и совпадают ли Id ВПП у ВС, которым принадлежат эти моменты
                    if (possibleMomentIndex + i < possibleTakingOffMoments.Count - 1 && 
                        takingOffAircrafts[possibleMomentIndex].RunwayId == takingOffAircrafts[possibleMomentIndex + i].RunwayId)
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

        /// <summary>
        /// Возвращает допустимое количество резервных ВС для переданного разрешенного момента и возможного момента текущкго ВС. 
        /// Возможный момент определяется списку переданных моментов и индексу момента текущего ВС
        /// </summary>
        /// <param name="permittedMoment"></param>
        /// <param name="possibleMomentIndex"></param>
        /// <param name="possibleTakingOffMoments"></param>
        /// <returns></returns>
        private int GetReserveAircraftCount(int permittedMoment, int possibleMomentIndex, List<int> possibleTakingOffMoments)
        {
            var reserveAircraftCount = 0;

            // Определяем максимально возможное количество резервных ВС
            var index = 1;
            while (possibleMomentIndex + index < possibleTakingOffMoments.Count - 1 && 
                permittedMoment >= possibleTakingOffMoments[possibleMomentIndex + index] + ModellingParameters.ArrivalReserveTime)
            {
                reserveAircraftCount++;
                index++;
            }

            // Проверяем полученное количество по заданному критерию
            int timeToLastTakeOffMoment;
            int permittedTime;
            do
            {
                // По заданному критерию, в зависимости от определенного количества резервных ВС, находим допустимое время простоя резервных ВС
                permittedTime = ModellingParameters.ReserveAircraftCount.
                    Where(item => reserveAircraftCount <= item.Value).OrderBy(i => i.Value).First().Key;
                
                // Рассчитываем время простоя (время, которое пройдет с момента взлета первого (основного) ВС до момента взлета последнего резервного ВС)
                timeToLastTakeOffMoment = possibleTakingOffMoments[possibleMomentIndex + reserveAircraftCount] - possibleTakingOffMoments[possibleMomentIndex];
                
                // Если рассчитанное время простоя больше допустимого => уменьшаем количество резервных ВС
                if (timeToLastTakeOffMoment > permittedTime)
                    reserveAircraftCount--;

                // Повторяем, пока не удовлетровим заданному критерию
            }
            while (timeToLastTakeOffMoment > permittedTime);

            return reserveAircraftCount;
        }

        /// <summary>
        /// Возвращает данные для заполнения выходной таблицы в виде списка рядов
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        private List<TableRow> GetOutputData(List<int> plannedTakingOffMoments)
        {
            var tableRows = new List<TableRow>();

            // Отбираем еще не использованный плановые моменты
            var rawPlannedTakingOffMoments = plannedTakingOffMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();
            // Увеличиваем индекс последнего использованного планового момента
            lastPlannedTakingOffMomentIndex += rawPlannedTakingOffMoments.Count;

            // Получаем список ВС с заданными возможными и стартовыми моментами, упорядоченный по возможным моментам
            var orderedConfiguredTakingOffAircrafts = GetOrderedConfiguredTakingOffAircrafts(rawPlannedTakingOffMoments);
            // Получаем список ВС с заданными резервными ВС
            var aircraftsWithReserve = GetReconfiguredAircraftsWithReserve(orderedConfiguredTakingOffAircrafts);

            // Для всех ВС задаем время простоя на ПРДВ
            SetPSWaitingTime(aircraftsWithReserve);

            // Упорядочиваем список ВС по разрешенным моментам
            var aircraftsOrderedByPermittedMoments = aircraftsWithReserve.OrderBy(a => a.Moments.PermittedTakingOffMoment).ToList();
            // Добавляем данные о каждом ВС в таблицу
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
                    aircraft.ProcessingIsNeeded, ((int)aircraft.Priority).ToString(), aircraft.IsReserve, aircraft.PSWaitingTime.ToString(), 
                    aircraft.RunwayId.ToString(), specialPlaceId);
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
    }
}
