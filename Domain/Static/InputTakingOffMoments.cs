using System.Collections.Generic;
using System.Linq;

namespace OptimalMotion3._1.Domain.Static
{
    public class InputTakingOffMoments
    {
        public InputTakingOffMoments(List<int> plannedMoments, List<int> permittedMoments)
        {
            PlannedMoments = plannedMoments;
            PermittedMoments = permittedMoments;
        }


        private int lastPlannedTakingOffMomentIndex = -1;

        private int lastPermittedMomentIndex = -1;

        public List<int> PlannedMoments { get; }

        // Когда будешь работать с ними, просто упорядоч их
        public List<int> PermittedMoments { get; }

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
                if (permittedMoment - CommonInputData.SpareArrivalTimeInterval.FirstMoment >= possibleMoment)
                {
                    lastPermittedMomentIndex = orderedPermittedMoments.IndexOf(permittedMoment);
                    return permittedMoment;
                }
            }

            return null;
        }

        public List<int> GetUnusedPlannedMoments()
        {
            // Отбираем еще не использованные плановые моменты
            var unusedMoments = PlannedMoments.Skip(lastPlannedTakingOffMomentIndex + 1).ToList();

            // Увеличиваем индекс последнего использованного планового момента
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
