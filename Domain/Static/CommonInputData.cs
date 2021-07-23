using System.Collections.Generic;

namespace OptimalMotion3._1.Domain.Static
{
    /// <summary>
    /// Класс для общих входных данных
    /// </summary>
    public static class CommonInputData
    {
        /// <summary>
        /// Количество ВПП
        /// </summary>
        public static int RunwayCount { get; set; } = 2;

        /// <summary>
        /// Количество спец. площадок
        /// </summary>
        public static int SpecialPlaceCount { get; set; } = 2;

        /// <summary>
        /// Наборы плановых и разрешенных моментов
        /// </summary>
        public static InputTakingOffMoments InputTakingOffMoments { get; } =
            new InputTakingOffMoments(
                // Плановые моменты
                new List<int>
                {
                    600, 630, 680, 700, 750, 1040, 1290, 1310, 1500, 1580
                },
                // Разрешенные моменты
                new List<int>
                {
                    660, 750, 790, 850, 880, 940, 1060, 1120, 1200, 1280, 1670, 
                    1700, 1760, 1800, 1900, 2000, 2090, 2150, 2240, 2390, 2500
                }
            );

        /// <summary>
        ///  Интервал запасного времени прибытия
        /// </summary>
        public static Interval SpareArrivalTimeInterval { get; set; } = new Interval(20, 50);

        /// <summary>
        /// Допустимое количество резервных ВС в зависимости от времени простоя
        /// </summary>
        public static Dictionary<int, int> PermissibleReserveAircraftCount { get; } = new Dictionary<int, int>
        {
            { 0, int.MaxValue },
            { 1, 300 },
            { 2, 240 },
            { 3, 180 },
            { 4, 120 },
            { 5, 10 },
            { int.MaxValue, 0 },
        };
    }
}
