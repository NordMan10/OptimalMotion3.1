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

            AddTakingOffAircraftsData += AddTakingOffAircraftsDataHandler;
        }

        public List<Runway> Runways { get; } = new List<Runway>();
        public List<SpecialPlace> SpecialPlaces { get; } = new List<SpecialPlace>();

        private readonly ITable table;

        private static readonly AircraftIdGenerator idGenerator = AircraftIdGenerator.GetInstance(ProgramConstants.StartIdValue);
        private readonly AircraftGenerator aircraftGenerator = AircraftGenerator.GetInstance(idGenerator);
        private readonly AircraftInputDataGenerator aircraftInputDataGenerator = AircraftInputDataGenerator.GetInstance();

        public event Func<List<TableRow>> AddTakingOffAircraftsData;

        
        /// <summary>
        /// Вызывает обработчик события добавления взлетающих ВС. Добавляет выходные данные в таблицу
        /// </summary>
        public void FillTableByTakingOffAircraftsData()
        {
            // Вызываем обработчик и получаем данные для заполнения таблицы
            var tableRows = AddTakingOffAircraftsData?.Invoke();

            // Заполняем таблицу
            foreach(var row in tableRows)
                table.AddRow(row);
        }

        /// <summary>
        /// Обработчик события добавления взлетающих ВС
        /// </summary>
        /// <returns></returns>
        private List<TableRow> AddTakingOffAircraftsDataHandler()
        {
            // Получаем еще не использованные плановые моменты
            var unusedPlannedTakingOffMoments = CommonInputData.InputTakingOffMoments.GetUnusedPlannedMoments();
            return GetOutputData(unusedPlannedTakingOffMoments);
        }

        /// <summary>
        /// Возвращает список ВС с рассчитанными возможным моментом взлета и моментом старта и упорядоченный по возможным моментам
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        private List<TakingOffAircraft> GetOrderedConfiguredTakingOffAircrafts(List<int> plannedTakingOffMoments)
        {
            // Создаем список ВС
            var takingOffAircrafts= new List<TakingOffAircraft>();
            // Упорядочиваем переданный список плановых моментов
            var orderedPlannedTakingOffMoments = plannedTakingOffMoments.OrderBy(m => m).ToList();

            var startDelay = 0;

            // Берем каждый плановый момент
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
                    var SPArriveMoment = takingOffInterval.StartMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES -
                        takingOffAircraft.CreationIntervals.MotionFromSPToPS - takingOffAircraft.CreationIntervals.Processing;

                    startMoment = SPArriveMoment - takingOffAircraft.CreationIntervals.MotionFromParkingToSP + startDelay - CommonInputData.SpareArrivalTimeInterval.EndMoment;
                }
                else
                {
                    // Рассчитываем и задаем момент старта при отсутствии необходимости обработки
                    startMoment = takingOffInterval.StartMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES - takingOffAircraft.CreationIntervals.MotionFromParkingToPS +
                        startDelay - CommonInputData.SpareArrivalTimeInterval.EndMoment;
                }

                // Задаем рассчитанный момент старта текущему ВС
                takingOffAircraft.CalculatingMoments.Start = startMoment;
                // Рассчитываем и задаем возможный момент взлета
                takingOffAircraft.CalculatingMoments.PossibleTakingOff = takingOffAircraft.CreationMoments.PlannedTakingOff + startDelay;

                takingOffAircrafts.Add(takingOffAircraft);
            }

            // Упорядочиваем ВС по возможному моменту
            var orderedTakingOffAircrafts = takingOffAircrafts.OrderBy(a => a.CalculatingMoments.PossibleTakingOff).ToList();

            // Возвращаем упорядоченный список ВС
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
            var thisRunway = Runways.Find(r => r.Id == takingOffAircraft.RunwayId);

            // Получаем свободный интервал от ВПП
            var freeRunwayInterval = thisRunway.GetFreeInterval(takingOffInterval);
            // Добавляем полученный новый интервал в ВПП
            thisRunway.AddAircraftInterval(takingOffAircraft.Id, freeRunwayInterval);

            // Рассчитываем и возвращаем задержку
            return freeRunwayInterval.StartMoment - takingOffInterval.StartMoment;
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

            var SPArriveMoment = takingOffInterval.StartMoment - takingOffAircraft.CreationIntervals.MotionFromPSToES -
                takingOffAircraft.CreationIntervals.MotionFromSPToPS - takingOffAircraft.CreationIntervals.Processing;

            var processingInterval = new Interval(SPArriveMoment, SPArriveMoment + takingOffAircraft.CreationIntervals.Processing);
            var freeSPInterval = thisSpecialPlace.GetFreeInterval(processingInterval);

            thisSpecialPlace.AddAircraftInterval(takingOffAircraft.Id, freeSPInterval);

            return freeSPInterval.StartMoment - processingInterval.StartMoment;
        }

        /// <summary>
        /// Возвращает список ВС с заданными резервными ВС
        /// </summary>
        /// <param name="orderedTakingOffAircrafts"></param>
        /// <returns></returns>
        private List<TakingOffAircraft> GetReconfiguredAircraftsWithReserve(List<TakingOffAircraft> orderedTakingOffAircrafts)
        {
            // Создаем список использованных индексов
            var usedIndexes = new List<int>();

            // Берем каждый ВС
            for (var i = 0; i < orderedTakingOffAircrafts.Count; i++)
            {
                // Проверяем, использовался ли уже этот индекс ВС
                if (usedIndexes.Contains(i))
                    // Если да, то пропускаем его
                    continue;

                // Если нет, то:

                // Получаем возможный момент ВС
                var possibleMoment = orderedTakingOffAircrafts[i].CalculatingMoments.PossibleTakingOff;

                // Пытаемся получить ближайший к возможному моменту разрешенный момент
                var nearestPermittedMoment = CommonInputData.InputTakingOffMoments.GetNearestPermittedMoment(possibleMoment);
                // Проверяем 
                if (nearestPermittedMoment == null)
                {
                    // Если получили null, значит разрешенный момент не найден
                    // Отмечаем это соответствующим значением
                    orderedTakingOffAircrafts[i].CalculatingMoments.Start = -1;
                    orderedTakingOffAircrafts[i].CalculatingMoments.PermittedTakingOff = -1;
                    // И пропускаем это  ВС
                    continue;
                }

                // Если же получили не null, то отмечаем, что это проверенный разрешенный момент
                var verifiedPermittedMoment = (int)nearestPermittedMoment;

                // Рассчитываем задержку для текущего ВС, возможный момент которого мы рассматриваем
                var startDelay = verifiedPermittedMoment - possibleMoment;
                // Рассчитываем момент старта для этого же ВС
                var currentAircraftStartMoment = orderedTakingOffAircrafts[i].CalculatingMoments.Start + startDelay;

                // Получаем список стартовых моментов для резервных ВС
                var reserveAircraftStartMoments = GetReserveAircraftsStartMoments(verifiedPermittedMoment, i, orderedTakingOffAircrafts);

                // Создаем один общий список пар значений <индекс ВС : момент старта> для текущего и резервных ВС
                var allAircraftsStartMomentData = new Dictionary<int, int> { { i, currentAircraftStartMoment } };
                foreach (var item in reserveAircraftStartMoments)
                    allAircraftsStartMomentData.Add(item.Key, item.Value);

                // Задаем моменты старта для текущего и резервных ВС
                SetSPreparedStartMoments(allAircraftsStartMomentData, orderedTakingOffAircrafts);

                // Получаем индекс ВС, имеющего наибольший приоритет (среди текущего и резервных ВС)
                var mostPriorityAircraftIndex = GetMostPriorityAircraftIndex(allAircraftsStartMomentData, orderedTakingOffAircrafts);

                // Берем каждую пару значений из созданного общего списка ВС
                foreach (var dataItem in allAircraftsStartMomentData)
                {
                    // Задаем разрешенный момент
                    orderedTakingOffAircrafts[dataItem.Key].CalculatingMoments.PermittedTakingOff = verifiedPermittedMoment;
                    // Сравниваем индекс ВС и индекс наиболее приритетного ВС
                    if (dataItem.Key != mostPriorityAircraftIndex)
                    {
                        // Если данное ВС не является наиболее приоритетным => помечаем его как резервное
                        orderedTakingOffAircrafts[dataItem.Key].IsReserve = true;
                        // Задаем резервный разрешенный момент (момент взлета, если это ВС останется резервным и не заменит главное ВС)
                        orderedTakingOffAircrafts[dataItem.Key].CalculatingMoments.ReservePermittedTakingOff = CommonInputData.InputTakingOffMoments.GetNextPermittedMoment();
                    }

                    // Добавляем индекс текущего ВС в список использованных
                    usedIndexes.Add(dataItem.Key);
                }
            }

            // Возвращаем список ВС с назначенными резервными ВС
            return orderedTakingOffAircrafts;
        }

        /// <summary>
        /// Задает подготовленные стартовые моменты переданным ВС
        /// </summary>
        /// <param name="aircraftsStartMomentData"></param>
        /// <param name="takingOffAircrafts"></param>
        private void SetSPreparedStartMoments(Dictionary<int, int> aircraftsStartMomentData, List<TakingOffAircraft> takingOffAircrafts)
        {
            foreach (var momentItem in aircraftsStartMomentData)
            {
                takingOffAircrafts[momentItem.Key].CalculatingMoments.Start = momentItem.Value;
            }
        }

        /// <summary>
        /// Возвращает индекс ВС с наибольшим приоритетом из всех переданных ВС
        /// </summary>
        /// <param name="aircraftsStartMomentData"></param>
        /// <param name="takingOffAircrafts"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает время простоя ВС на ПРДВ (учитывает заданный интервал запасного времени прибытия и задержку у резервных ВС)
        /// </summary>
        /// <param name="takingOffAircrafts"></param>
        private void SetPSWaitingTime(List<TakingOffAircraft> takingOffAircrafts)
        {
            foreach (var aircraft in takingOffAircrafts)
            {
                // Рассчитываем момент прибытия на ПРДВ
                int arrivalToPSMoment;
                if (aircraft.ProcessingIsNeeded)
                {
                    arrivalToPSMoment = aircraft.CalculatingMoments.Start + aircraft.CreationIntervals.MotionFromParkingToSP + aircraft.CreationIntervals.Processing +
                    aircraft.CreationIntervals.MotionFromSPToPS;
                }
                else
                    arrivalToPSMoment = aircraft.CalculatingMoments.Start + aircraft.CreationIntervals.MotionFromParkingToPS;

                // Рассчитываем время простоя
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
        private Dictionary<int, int> GetReserveAircraftsStartMoments(int permittedMoment, int aircraftIndex, List<TakingOffAircraft> takingOffAircrafts)
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

            var index = 1;
            // Определяем максимально возможное количество резервных ВС.
            // Пока имеются возможные моменты и разрешенный момент входит в разрешенный страховочный интервал
            while (aircraftIndex + index < possibleTakingOffMoments.Count - 1 &&
                permittedMoment - CommonInputData.SpareArrivalTimeInterval.StartMoment >= possibleTakingOffMoments[aircraftIndex + index])
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

            // Возвращаем количество резервных ВС
            return reserveAircraftCount;
        }

        /// <summary>
        /// Возвращает данные в виде списка рядов для заполнения выходной таблицы
        /// </summary>
        /// <param name="plannedTakingOffMoments"></param>
        /// <returns></returns>
        private List<TableRow> GetOutputData(List<int> unusedPlannedTakingOffMoments)
        {
            var tableRows = new List<TableRow>();

            // Получаем список ВС с заданными возможными и стартовыми моментами, упорядоченный по возможным моментам
            var orderedConfiguredTakingOffAircrafts = GetOrderedConfiguredTakingOffAircrafts(unusedPlannedTakingOffMoments);
            // Получаем список ВС с заданными резервными ВС
            var aircraftsWithReserve = GetReconfiguredAircraftsWithReserve(orderedConfiguredTakingOffAircrafts);

            // Для всех ВС задаем время простоя на ПРДВ
            SetPSWaitingTime(aircraftsWithReserve);

            // Упорядочиваем список ВС по разрешенным моментам
            var aircraftsOrderedByPermittedMoments = aircraftsWithReserve.OrderBy(a => a.CalculatingMoments.PermittedTakingOff).ToList();
            // Добавляем данные о каждом ВС в таблицу
            foreach(var aircraft in aircraftsOrderedByPermittedMoments)
            {
                var tableRow = GetTableRow(aircraft);
                tableRows.Add(tableRow);
            }

            // Возвращаем строки данных для таблицы
            return tableRows;
        }

        /// <summary>
        /// Возвращает строку данных для таблицы, заполняя рассчитанными данными
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
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
                    aircraft.ProcessingIsNeeded, (aircraft.Priority).ToString(), aircraft.IsReserve, aircraft.CalculatingIntervals.PSDelay.ToString(), 
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
