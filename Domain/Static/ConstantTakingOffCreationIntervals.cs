

namespace OptimalMotion3._1.Domain.Static
{
    /// <summary>
    /// Заданные средние значения интервалов для ВС (нужны временно, пока данные не будут передавать напрямую для каждого ВС)
    /// </summary>
    public static class ConstantTakingOffCreationIntervals
    {
        /// <summary>
        /// Время руления от парковки до ПРДВ
        /// </summary>
        public static int MotionFromParkingToPS = 240;

        /// <summary>
        /// Время руления от ПРДВ до ИСП
        /// </summary>
        public static int MotionFromPSToES = 40;

        /// <summary>
        /// Время руления от парковки до Спец. площадки
        /// </summary>
        public static int MotionFromParkingToSP = 120;

        /// <summary>
        /// Время руления от Спец. площадки до ПРДВ
        /// </summary>
        public static int MotionFromSPToPS = 120;

        /// <summary>
        /// Время взлета
        /// </summary>
        public static int TakingOffInterval = 30;

        /// <summary>
        /// Время обработки
        /// </summary>
        public static int ProcessingTime { get; set; } = 240;

    }
}
