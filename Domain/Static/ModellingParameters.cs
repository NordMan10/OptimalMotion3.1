

namespace OptimalMotion3._1.Domain.Static
{
    public static class ModellingParameters
    {
        /// <summary>
        /// Первый момент взлета
        /// </summary>
        public static int FirstTakingOffMoment { get; set; } = 600;

        /// <summary>
        /// Интервал моментов взлета
        /// </summary>
        public static int TakingOffMomentStep { get; set; } = 180;

        /// <summary>
        /// Начальное значение Id для зон массового обслуживания
        /// </summary>
        public static int StartIdValue = 1;

        /// <summary>
        /// Количество ВПП
        /// </summary>
        public static int RunwayCount { get; set; } = 2;

        /// <summary>
        /// Количество спец. площадок
        /// </summary>
        public static int SpecialPlaceCount { get; set; } = 2;

        /// <summary>
        ///  Резервное время прибытия
        /// </summary>
        public static int ArrivalReserveTime { get; set; } = 30;
    }
}
