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
            OrderedPlannedMoments = plannedMoments.OrderBy(m => m).ToList();
            OrderedPermittedMoments = permittedMoments.OrderBy(m => m).ToList();
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
        public List<int> OrderedPlannedMoments { get; }

        /// <summary>
        /// Разрешенные моменты взлета
        /// </summary>
        public List<int> OrderedPermittedMoments { get; }

        /// <summary>
        /// Возвращает самый первый неиспользованный разрешенный момент взлета
        /// </summary>
        /// <returns></returns>
        public int GetNextPermittedMoment()
        {
            return OrderedPermittedMoments[++lastPermittedMomentIndex];
        }

        /// <summary>
        /// Возвращаем ближайший разрешенный момент для переданного возможного момента, если его возможно установить. 
        /// Если невозможно, возвращает null
        /// </summary>
        /// <param name="possibleMoment"></param>
        /// <returns></returns>
        public int? GetNearestPermittedMoment(int possibleMoment)
        {
            // Выбираем только те, что еще не были использованы
            var permittedMoments = OrderedPermittedMoments.Skip(lastPermittedMomentIndex + 1).ToList();

            // Проверяем каждый разрешенный момент
            foreach (var permittedMoment in permittedMoments)
            {
                // Если разрешенный момент - страховочное время прибытия больше или равен возможному => возвращаем его
                if (permittedMoment - CommonInputData.SpareArrivalTimeInterval.StartMoment >= possibleMoment)
                {
                    lastPermittedMomentIndex = OrderedPermittedMoments.IndexOf(permittedMoment);
                    return permittedMoment;
                }
            }

            // Если такого момента не нашлось => возвращаем null
            return null;
        }

        /// <summary>
        /// Возвращает список неиспользованных плановых моментов
        /// </summary>
        /// <returns></returns>
        public List<int> GetUnusedPlannedMoments()
        {
            // Упорядочиваем плановые моменты
            var orderedPlannedMoments = OrderedPlannedMoments.OrderBy(m => m).ToList();

            // Отбираем еще не использованные плановые моменты
            var unusedMoments = orderedPlannedMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();

            // Увеличиваем индекс последнего использованного планового момента на количество
            // неиспользованных (потому что подразумевается, что раз их взяли, то они уже использованы)
            lastPlannedTakingOffMomentIndex += unusedMoments.Count;

            return unusedMoments;
        }


        public void ResetLastPlannedTakingOffMomentIndex()
        {
            lastPlannedTakingOffMomentIndex = -1;
        }

        public void ResetLastPermittedTakingOffMomentIndex()
        {
            lastPermittedMomentIndex = -1;
        }
    }
}
