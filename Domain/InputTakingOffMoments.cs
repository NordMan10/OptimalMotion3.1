using OptimalMotion3._1.Domain.Static;
using System.Collections.Generic;
using System.Linq;

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Содержит набор плановых и разрешенных моментов взлета и методы для работы с ними
    /// </summary>
    public class InputTakingOffMoments
    {
        public InputTakingOffMoments(List<int> plannedMoments, List<int> permittedMoments)
        {
            PlannedMoments = plannedMoments;
            PermittedMoments = permittedMoments;
        }

        /// <summary>
        /// Индекс последнего использованного планового момента
        /// </summary>
        private int lastPlannedTakingOffMomentIndex = -1;
        /// <summary>
        /// Индекс последнего использованного разрешенного момента
        /// </summary>
        private int lastPermittedMomentIndex = -1;

        /// <summary>
        /// Плановые моменты взлета
        /// </summary>
        public List<int> PlannedMoments { get; }

        /// <summary>
        /// Разрешенные моменты взлета
        /// </summary>
        public List<int> PermittedMoments { get; }

        /// <summary>
        /// Возвращает самый первый неиспользованный разрешенный момент взлета
        /// </summary>
        /// <returns></returns>
        public int GetNextPermittedMoment()
        {
            return PermittedMoments[++lastPermittedMomentIndex];
        }

        /// <summary>
        /// Возвращаем ближайший разрешенный момент для переданного возможного момента, если его возможно установить. 
        /// Если невозможно, возвращает null
        /// </summary>
        /// <param name="possibleMoment"></param>
        /// <returns></returns>
        public int? GetNearestPermittedMoment(int possibleMoment)
        {
            // Упорядочиваем разрешенные моменты
            var orderedPermittedMoments = PermittedMoments.OrderBy(m => m).ToList();
            // Выбираем только те, что еще не были использованы
            var permittedMoments = orderedPermittedMoments.Skip(lastPermittedMomentIndex + 1).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент больше или равен возможному + резервное время прибытия => возвращаем его
                if (permittedMoment - CommonInputData.SpareArrivalTimeInterval.StartMoment >= possibleMoment)
                {
                    lastPermittedMomentIndex = orderedPermittedMoments.IndexOf(permittedMoment);
                    return permittedMoment;
                }
            }

            return null;
        }

        /// <summary>
        /// Возвращает список неиспользованных плановых моментов
        /// </summary>
        /// <returns></returns>
        public List<int> GetUnusedPlannedMoments()
        {
            // Отбираем еще не использованные плановые моменты
            var unusedMoments = PlannedMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();

            // Увеличиваем индекс последнего использованного планового момента на количество
            // неиспользованных (потому что подразумевается, что раз их взяли, то они уже использованы)
            lastPlannedTakingOffMomentIndex += unusedMoments.Count;

            return unusedMoments;
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
