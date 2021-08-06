

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Интерваоы, получаемые во входных данных для взлетающего ВС
    /// </summary>
    public class TakingOffAircraftCreationIntervals
    {
        public TakingOffAircraftCreationIntervals(
            int motionFromParkingToPS, int motionFromPSToES, int takingOff,
            int motionFromParkingToSP, int motionFromSPToPS, int processing)
        {
            MotionFromParkingToPS = motionFromParkingToPS;
            MotionFromPSToES = motionFromPSToES;
            TakingOff = takingOff;
            MotionFromParkingToSP = motionFromParkingToSP;
            MotionFromSPToPS = motionFromSPToPS;
            Processing = processing;
        }

        /// <summary>
        /// Время руления от парковки до ПРДВ
        /// </summary>
        public int MotionFromParkingToPS { get; }

        /// <summary>
        /// Время руления от ПРДВ до ИСП
        /// </summary>
        public int MotionFromPSToES { get; }

        /// <summary>
        /// Время руления от парковки до Спец. площадки
        /// </summary>
        public int MotionFromParkingToSP { get; }

        /// <summary>
        /// Время руления от Спец. площадки до ПРДВ
        /// </summary>
        public int MotionFromSPToPS { get; }

        /// <summary>
        /// Время взлета
        /// </summary>
        public int TakingOff { get; }

        /// <summary>
        /// Время обработки
        /// </summary>
        public int Processing { get; set; }
    }
}
