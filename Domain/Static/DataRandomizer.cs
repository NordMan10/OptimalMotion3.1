using System;
using System.Collections.Generic;
using System.Globalization;

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

        public static int GetRandomizedValue(int value, int dispersion, int step)
        {
            if (dispersion == 0 || step == 0)
                return value;
            var minValue = value - (value * (double)dispersion / 100);
            var maxValue = value + (value * (double)dispersion / 100);

            var possibleValues = new List<int>();

            var possibleValue = minValue;
            while ((int)possibleValue <= maxValue)
            {
                possibleValues.Add((int)possibleValue);
                possibleValue += step;
            }

            var valueIndex = random.Next(0, possibleValues.Count);

            return possibleValues[valueIndex];
        }
    }
}
