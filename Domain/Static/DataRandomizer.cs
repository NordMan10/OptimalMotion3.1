using System;

namespace OptimalMotion3._1.Domain.Static
{
    public static class DataRandomizer
    {
        private static Random random = new Random();

        /// <summary>
        /// Возвращает значение в пределах переданного интервала значений
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="minValue"></param>
        /// <returns></returns>
        public static int GetRandomizedValue(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }
    }
}
