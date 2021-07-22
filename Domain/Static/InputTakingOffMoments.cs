using System.Collections.Generic;
using System.Linq;

namespace OptimalMotion3._1.Domain.Static
{
    public static class InputTakingOffMoments
    {
        public static int PlannedMomentsStep { get; } = 90;
        public static int PermittedMomentsStep { get; } = 90;

        private static int lastPlannedTakingOffMomentIndex = -1;

        private static int lastPermittedMomentIndex = -1;


        public static List<int> PlannedMoments { get; } = new List<int>
        {
            600, 630, 680, 700, 750, 1040, 1290, 1310, 1500, 1580
        };

        // Когда будешь работать с ними, просто упорядоч их
        public static List<int> PermittedMoments { get; } = new List<int>
        {
            660, 750, 790, 850, 880, 940, 1060, 1120, 1200, 1280, 1670, 1700, 1760, 1800, 1900, 2000, 2090, 2150, 2240, 2390, 2500
        };

        public static void AddTakingOffMoments(int plannedMoment, int permittedMoment)
        {
            PlannedMoments.Add(plannedMoment);
            PermittedMoments.Add(permittedMoment);
        }

        public static int GetNextPermittedMoment()
        {
            return PermittedMoments[++lastPermittedMomentIndex];
        }

        /// <summary>
        /// Возвращаем ближайший разрешенный момент для переданного возможного момента, если его возможно установить. 
        /// Если невозможно, возвращает null
        /// </summary>
        /// <param name="possibleMoment"></param>
        /// <returns></returns>
        public static int? GetNearestPermittedMoment(int possibleMoment)
        {
            // Упорядочиваем разрешенные моменты
            var orderedPermittedMoments = InputTakingOffMoments.PermittedMoments.OrderBy(m => m).ToList();
            // Выбираем только те, что еще не были использованы
            var permittedMoments = orderedPermittedMoments.Skip(lastPermittedMomentIndex + 1).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент больше или равен возможному + резервное время прибытия => возвращаем его
                if (permittedMoment - ModellingParameters.ArrivalReserveTime.FirstMoment >= possibleMoment)
                {
                    lastPermittedMomentIndex = orderedPermittedMoments.IndexOf(permittedMoment);
                    return permittedMoment;
                }
            }

            return null;
        }

        public static List<int> GetUnusedPlannedMoments()
        {
            // Отбираем еще не использованные плановые моменты
            var unusedMoments = PlannedMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();

            // Увеличиваем индекс последнего использованного планового момента
            lastPlannedTakingOffMomentIndex += unusedMoments.Count;

            return unusedMoments;
        }

        public static void ResetLastPlannedTakingOffMomentIndex()
        {
            lastPlannedTakingOffMomentIndex = -1;
        }

        public static void ResetLastPermittedMomentIndex()
        {
            lastPermittedMomentIndex = -1;
        }

        //private void SetStartMomentsByTakingOffMoments(Dictionary<TakingOffAircraft, int> aircraftsAndTakingOffMoments, int totalDelay)
        //{
        //    foreach (var item in aircraftsAndTakingOffMoments)
        //    {
        //        var aircraft = item.Key;
        //        var takingOffMoment = item.Value;

        //        aircraft.CalculatingMoments.Start = takingOffMoment - aircraft.CreationIntervals.TakingOff -
        //            aircraft.CreationIntervals.MotionFromPSToES + totalDelay;

        //        if (aircraft.ProcessingIsNeeded)
        //            aircraft.CalculatingMoments.Start -= aircraft.CreationIntervals.MotionFromSPToPS - aircraft.CreationIntervals.Processing -
        //                aircraft.CreationIntervals.MotionFromParkingToSP;
        //        else
        //            aircraft.CalculatingMoments.Start -= aircraft.CreationIntervals.MotionFromParkingToPS;
        //    }
        //}
    }
}
