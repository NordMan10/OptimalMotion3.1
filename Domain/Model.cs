using OptimalMotion3._1.Domain.Enums;
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

        public List<Runway> Runways { get; } = new List<Runway>();
        public List<SpecialPlace> SpecialPlaces { get; } = new List<SpecialPlace>();

        private readonly ITable table;

        private static readonly AircraftIdGenerator idGenerator = AircraftIdGenerator.GetInstance(ProgramConstants.StartIdValue);
        private readonly AircraftGenerator aircraftGenerator = AircraftGenerator.GetInstance(idGenerator);
        private readonly AircraftInputDataGenerator aircraftInputDataGenerator = AircraftInputDataGenerator.GetInstance();

        public event Func<List<TableRow>> AddTakingOffAircrafts;

        
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
            var plannedAircraftTakingOffMoments = CommonInputData.InputTakingOffMoments.PlannedMoments.ToList();
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

            var startDelay = 0;
            //var startMomentsData = new Dictionary<TakingOffAircraft, int>();

            for (var i = 0; i < orderedPlannedTakingOffMoments.Count; i++)
            {
                // Генерируем входные данные для нового ВС
                var inputData = aircraftInputDataGenerator.GetAircraftInputData(orderedPlannedTakingOffMoments[i]);
                // Создаем ВС
                var takingOffAircraft = aircraftGenerator.GetTakingOffAircraft(inputData);
                int startMoment;

                // Рассчитываем интервал взлета
                var takingOffInterval = new Interval(inputData.CreationMoments.PlannedTakingOff - takingOffAircraft.CreationIntervals.TakingOff, 
                    inputData.CreationMoments.PlannedTakingOff);

                // Получаем задержку
                startDelay = GetRunwayStartDelay(takingOffAircraft, takingOffInterval);

                // Если нужна обработка
                if (takingOffAircraft.ProcessingIsNeeded)
                {
                    // Получаем задержку
                    startDelay += GetSpecialPlaceStartDelay(takingOffAircraft, takingOffInterval);

                    // Рассчитываем и задаем момент старта при необходимости обработки
                    var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES -
                        takingOffAircraft.CreationIntervals.MotionFromSPToPS - takingOffAircraft.CreationIntervals.Processing;

                    startMoment = SPArriveMoment - takingOffAircraft.CreationIntervals.MotionFromParkingToSP + startDelay - CommonInputData.SpareArrivalTimeInterval.LastMoment;
                }
                else
                {
                    // Рассчитываем и задаем момент старта при отсутствии необходимости обработки
                    startMoment = takingOffInterval.FirstMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES - takingOffAircraft.CreationIntervals.MotionFromParkingToPS +
                        startDelay - CommonInputData.SpareArrivalTimeInterval.LastMoment;
                }

                // Задаем рассчитанный момент старта текущему ВС
                takingOffAircraft.CalculatingMoments.Start = startMoment;
                // Рассчитываем и задаем возможный момент взлета
                takingOffAircraft.CalculatingMoments.PossibleTakingOff = takingOffAircraft.CreationMoments.PlannedTakingOff + startDelay;

                //startMomentsData.Add(takingOffAircraft, takingOffAircraft.CreationMoments.PlannedTakingOff);

                takingOffAircrafts.Add(takingOffAircraft);
            }


            var orderedTakingOffAircrafts = takingOffAircrafts.OrderBy(a => a.CalculatingMoments.PossibleTakingOff).ToList();
            return orderedTakingOffAircrafts;
        }

        //private void SetStartMomentsByTakingOffMoments(Dictionary<TakingOffAircraft, int> aircraftsAndTakingOffMoments, int totalDelay)
        //{
        //    foreach (var item in aircraftsAndTakingOffMoments)
        //    {
        //        var aircraft = item.Key;
        //        var takingOffMoment = item.Value;

        //        //var temp = takingOffMoment - aircraft.CreationIntervals.TakingOff -
        //        //    aircraft.CreationIntervals.MotionFromPSToES + totalDelay;

        //        if (aircraft.ProcessingIsNeeded)
        //            aircraft.CalculatingMoments.Start = takingOffMoment - aircraft.CreationIntervals.TakingOff -
        //            aircraft.CreationIntervals.MotionFromPSToES - aircraft.CreationIntervals.MotionFromSPToPS - aircraft.CreationIntervals.Processing -
        //                aircraft.CreationIntervals.MotionFromParkingToSP + totalDelay;
        //        else
        //            aircraft.CalculatingMoments.Start = takingOffMoment - aircraft.CreationIntervals.TakingOff -
        //            aircraft.CreationIntervals.MotionFromPSToES - aircraft.CreationIntervals.MotionFromParkingToPS + totalDelay;
        //    }
        //}

        /// <summary>
        /// Возвращает задержку момента старта от ВПП
        /// </summary>
        /// <param name="takingOffAircraft"></param>
        /// <param name="takingOffInterval"></param>
        /// <returns></returns>
        private int GetRunwayStartDelay(TakingOffAircraft takingOffAircraft, Interval takingOffInterval)
        {
            var thisRunway = Runways.Find(r => r.Id == takingOffAircraft.RunwayId);

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
            var thisSpecialPlace = SpecialPlaces.Find(sp => sp.Id == takingOffAircraft.SpecialPlaceId);

            var SPArriveMoment = takingOffInterval.FirstMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES -
                takingOffAircraft.CreationIntervals.MotionFromSPToPS - takingOffAircraft.CreationIntervals.Processing;

            var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.CreationIntervals.Processing);
            var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

            thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

            return freeSPInterval.FirstMoment - processingInterval.FirstMoment;
        }


        private List<TakingOffAircraft> GetReconfiguredAircraftsWithReserve(List<TakingOffAircraft> takingOffAircrafts)
        {
            var usedIndexes = new List<int>();

            for (var i = 0; i < takingOffAircrafts.Count; i++)
            {
                // Проверяем, использовался ли уже этот индекс ВС
                if (usedIndexes.Contains(i))
                    continue;

                // Получаем возможный момент ВС
                var possibleMoment = takingOffAircrafts[i].CalculatingMoments.PossibleTakingOff;

                // Получаем ближайший к возможному разрешенный момент
                var nearestPermittedMoment = CommonInputData.InputTakingOffMoments.GetNearestPermittedMoment(possibleMoment);
                if (nearestPermittedMoment == null)
                {
                    takingOffAircrafts[i].CalculatingMoments.Start = -1;
                    takingOffAircrafts[i].CalculatingMoments.PermittedTakingOff = -1;
                    continue;
                }

                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                var startDelay = verifiedPermittedMoment - possibleMoment;
                var currentAircraftStartMoment = takingOffAircrafts[i].CalculatingMoments.Start + startDelay;

                var reserveAircraftStartMoments = GetReserveAircraftStartMoments(verifiedPermittedMoment, i, takingOffAircrafts);

                // Создаем список моментов старта текущего и резервных ВС, связанных по индексу с конкретным ВС
                var allAircraftsStartMomentData = new Dictionary<int, int> { { i, currentAircraftStartMoment } };
                foreach (var item in reserveAircraftStartMoments)
                    allAircraftsStartMomentData.Add(item.Key, item.Value);

                // Задаем моменты старта для текущего и резервных ВС
                SetSPreparedStartMoments(allAircraftsStartMomentData, takingOffAircrafts);

                var mostPriorityAircraftIndex = GetMostPriorityAircraftIndex(allAircraftsStartMomentData, takingOffAircrafts);

                foreach (var dataItem in allAircraftsStartMomentData)
                {
                    takingOffAircrafts[dataItem.Key].CalculatingMoments.PermittedTakingOff = verifiedPermittedMoment;
                    if (dataItem.Key != mostPriorityAircraftIndex)
                    {
                        takingOffAircrafts[dataItem.Key].IsReserve = true;
                        takingOffAircrafts[dataItem.Key].CalculatingMoments.ReservePermittedTakingOff = CommonInputData.InputTakingOffMoments.GetNextPermittedMoment();
                    }

                    usedIndexes.Add(dataItem.Key);
                }
            }

            return takingOffAircrafts;
        }

        private void SetSPreparedStartMoments(Dictionary<int, int> aircraftsStartMomentData, List<TakingOffAircraft> takingOffAircrafts)
        {
            foreach (var momentItem in aircraftsStartMomentData)
            {
                takingOffAircrafts[momentItem.Key].CalculatingMoments.Start = momentItem.Value;
            }
        }

        private int GetMostPriorityAircraftIndex(Dictionary<int, int> aircraftsStartMomentData, List<TakingOffAircraft> takingOffAircrafts)
        {
            var mostPriorityAircraftIndex = aircraftsStartMomentData.First().Key;
            foreach (var dataItem in aircraftsStartMomentData)
            {
                if (takingOffAircrafts[dataItem.Key].Priority > takingOffAircrafts[mostPriorityAircraftIndex].Priority)
                    mostPriorityAircraftIndex = dataItem.Key;
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
                    arrivalToPSMoment = aircraft.CalculatingMoments.Start + aircraft.CreationIntervals.MotionFromParkingToSP + aircraft.CreationIntervals.Processing +
                    aircraft.CreationIntervals.MotionFromSPToPS;
                }
                else
                    arrivalToPSMoment = aircraft.CalculatingMoments.Start + aircraft.CreationIntervals.MotionFromParkingToPS;
                
                

                if (aircraft.IsReserve)
                    aircraft.CalculatingIntervals.PSDelay = aircraft.CalculatingMoments.ReservePermittedTakingOff - arrivalToPSMoment - aircraft.CreationIntervals.MotionFromPSToES - 
                        aircraft.CreationIntervals.TakingOff;
                else
                    aircraft.CalculatingIntervals.PSDelay = aircraft.CalculatingMoments.PermittedTakingOff - arrivalToPSMoment - aircraft.CreationIntervals.MotionFromPSToES -
                        aircraft.CreationIntervals.TakingOff;
            }
        }

        /// <summary>
        /// Возвращает список моментов старта для резервных ВС, если их возможно задать. Если невозможно, возвращает пустой список
        /// </summary>
        /// <param name="permittedMoment"></param>
        /// <param name="aircraftIndex"></param>
        /// <param name="takingOffAircrafts"></param>
        /// <returns></returns>
        private Dictionary<int, int> GetReserveAircraftStartMoments(int permittedMoment, int aircraftIndex, List<TakingOffAircraft> takingOffAircrafts)
        {
            var reserveStartMoments = new Dictionary<int, int>();

            // Получаем список возможных моментов взлета
            var possibleTakingOffMoments = takingOffAircrafts.Select(a => a.CalculatingMoments.PossibleTakingOff).ToList();

            // Проверяем, есть ли еще возможные моменты
            if (aircraftIndex < possibleTakingOffMoments.Count - 1)
            {
                // Определяем допустимое количество резервных ВС
                var reserveAircraftCount = GetReserveAircraftCount(permittedMoment, aircraftIndex, possibleTakingOffMoments);

                for (var i = 1; i < reserveAircraftCount + 1; i++)
                {
                    // Проверяем, есть ли еще возможные моменты и совпадают ли Id ВПП у ВС, которым принадлежат эти моменты
                    if (aircraftIndex + i < possibleTakingOffMoments.Count && 
                        takingOffAircrafts[aircraftIndex].RunwayId == takingOffAircrafts[aircraftIndex + i].RunwayId)
                    {
                        // Берем возможный момент для резервного ВС;
                        var reserveAircraftPossibleMoment = possibleTakingOffMoments[aircraftIndex + i];

                        // Рассчитываем задержку для момента старта резервного ВС
                        var startDelay = permittedMoment - reserveAircraftPossibleMoment;
                        // Задаем момент старта для резервного ВС
                        var reserveAircraftStartMoment = takingOffAircrafts[aircraftIndex + i].CalculatingMoments.Start + startDelay;

                        // Добавляем момент старта
                        reserveStartMoments.Add(aircraftIndex + i, reserveAircraftStartMoment);
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
        /// <param name="aircraftIndex"></param>
        /// <param name="possibleTakingOffMoments"></param>
        /// <returns></returns>
        private int GetReserveAircraftCount(int permittedMoment, int aircraftIndex, List<int> possibleTakingOffMoments)
        {
            var reserveAircraftCount = 0;
            var permittedInterval = new Interval(permittedMoment - CommonInputData.SpareArrivalTimeInterval.FirstMoment,
                    permittedMoment - CommonInputData.SpareArrivalTimeInterval.LastMoment);

            var index = 1;
            // Определяем максимально возможное количество резервных ВС.
            // Пока имеются возможные моменты и разрешенный момент входит в разрешенный страховочный интервал
            while (aircraftIndex + index < possibleTakingOffMoments.Count - 1 &&
                permittedMoment - CommonInputData.SpareArrivalTimeInterval.FirstMoment >= possibleTakingOffMoments[aircraftIndex + index])
            {
                // Увеличиваем количество резервных ВС
                reserveAircraftCount++;
                // Увеличиваем индекс
                index++;
            }

            // Проверяем полученное количество по заданному критерию
            int timeToLastTakingOffMoment;
            int permittedTime;
            do
            {
                // По заданному критерию, в зависимости от определенного количества резервных ВС, находим допустимое время простоя резервных ВС
                permittedTime = CommonInputData.PermissibleReserveAircraftCount.
                    Where(item => reserveAircraftCount <= item.Key).OrderBy(i => i.Key).First().Value;
                
                // Рассчитываем время простоя (время, которое пройдет с момента взлета первого (основного) ВС до момента взлета последнего резервного ВС)
                timeToLastTakingOffMoment = possibleTakingOffMoments[aircraftIndex + reserveAircraftCount] - possibleTakingOffMoments[aircraftIndex];
                
                // Если рассчитанное время простоя больше допустимого => уменьшаем количество резервных ВС
                if (timeToLastTakingOffMoment > permittedTime)
                    reserveAircraftCount--;

                // Повторяем, пока не удовлетровим заданному критерию
            }
            while (timeToLastTakingOffMoment > permittedTime);

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

            // Получаем еще не использованные плановые моменты
            var rawPlannedTakingOffMoments = CommonInputData.InputTakingOffMoments.GetUnusedPlannedMoments();

            // Получаем список ВС с заданными возможными и стартовыми моментами, упорядоченный по возможным моментам
            var orderedConfiguredTakingOffAircrafts = GetOrderedConfiguredTakingOffAircrafts(rawPlannedTakingOffMoments);
            // Получаем список ВС с заданными резервными ВС
            var aircraftsWithReserve = GetReconfiguredAircraftsWithReserve(orderedConfiguredTakingOffAircrafts);

            // Для всех ВС задаем время простоя на ПРДВ
            SetPSWaitingTime(aircraftsWithReserve);

            // Упорядочиваем список ВС по разрешенным моментам
            var aircraftsOrderedByPermittedMoments = aircraftsWithReserve.OrderBy(a => a.CalculatingMoments.PermittedTakingOff).ToList();
            // Добавляем данные о каждом ВС в таблицу
            foreach(var aircraft in aircraftsOrderedByPermittedMoments)
            {
                tableRows.Add(GetTableRow(aircraft));
            }

            return tableRows;
        }

        private TableRow GetTableRow(TakingOffAircraft aircraft)
        {
            var aircraftTotalMotionTime = aircraft.CreationIntervals.TakingOff + aircraft.CreationIntervals.MotionFromPSToES;
            if (aircraft.ProcessingIsNeeded)
                aircraftTotalMotionTime += aircraft.CreationIntervals.MotionFromSPToPS + aircraft.CreationIntervals.MotionFromParkingToSP;
            else
                aircraftTotalMotionTime += aircraft.CreationIntervals.MotionFromParkingToPS;

            var permittedMoment = aircraft.CalculatingMoments.PermittedTakingOff != -1 ? aircraft.CalculatingMoments.PermittedTakingOff.ToString() : "Не найден";
            var processingTime = aircraft.ProcessingIsNeeded ? aircraft.CreationIntervals.Processing.ToString() : "-";
            var specialPlaceId = aircraft.ProcessingIsNeeded ? aircraft.SpecialPlaceId.ToString() : "-";

            return new TableRow(aircraft.Id.ToString(), aircraft.CreationMoments.PlannedTakingOff.ToString(), aircraft.CalculatingMoments.PossibleTakingOff.ToString(),
                    permittedMoment, aircraft.CalculatingMoments.Start.ToString(), aircraftTotalMotionTime.ToString(), processingTime,
                    aircraft.ProcessingIsNeeded, ((int)aircraft.Priority).ToString(), aircraft.IsReserve, aircraft.CalculatingIntervals.PSDelay.ToString(), 
                    aircraft.RunwayId.ToString(), specialPlaceId);
        }

        #region Initializations

        private void InitRunways(int runwayCount)
        {
            for (var i = ProgramConstants.StartIdValue; i < runwayCount + ProgramConstants.StartIdValue; i++)
            {
                var runway = new Runway(i.ToString());
                Runways.Add(runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = ProgramConstants.StartIdValue; i < specPlatformCount + ProgramConstants.StartIdValue; i++)
            {
                var specialPlace = new SpecialPlace(i);
                SpecialPlaces.Add(specialPlace);
            }
        }

        #endregion


        #region UpdateAndReset

        public void UpdateModel(int runwayCount, int specialPlaceCount)
        {
            UpdateRunways(runwayCount);
            UpdateSpecialPlaces(specialPlaceCount);
        }

        private void UpdateRunways(int runwayCount)
        {
            for (var i = Runways.Count; i < runwayCount; i++)
            {
                var runway = new Runway((i + ProgramConstants.StartIdValue).ToString());
                Runways.Add(runway);
            }
        }

        private void UpdateSpecialPlaces(int specialPlaceCount)
        {
            for (var i = SpecialPlaces.Count; i < specialPlaceCount; i++)
            {
                var specialPlace = new SpecialPlace(i + ProgramConstants.StartIdValue);
                SpecialPlaces.Add(specialPlace);
            }
        }

        public void ResetRunways()
        {
            foreach (var runway in Runways)
                runway.Reset();
        }

        public void ResetSpecialPlaces()
        {
            foreach (var specialPlace in SpecialPlaces)
                specialPlace.Reset();
        }

        #endregion
    }
}
