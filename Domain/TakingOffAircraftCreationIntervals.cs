

namespace OptimalMotion3._1.Domain
{
    public class TakingOffAircraftCreationIntervals
    {
        public TakingOffAircraftCreationIntervals(
            int motionFromParkingToPS, int motionFromPSToES, int takeoff,
            int motionFromParkingToSP, int motionFromSPToPS, int processing)
        {
            MotionFromParkingToPS = motionFromParkingToPS;
            MotionFromPSToES = motionFromPSToES;
            TakingOff = takeoff;
            MotionFromParkingToSP = motionFromParkingToSP;
            MotionFromSPToPS = motionFromSPToPS;
            Processing = processing;
        }

        public int MotionFromParkingToPS { get; }
        public int MotionFromPSToES { get; }
        public int TakingOff { get; }
        public int Processing { get; }
        public int MotionFromParkingToSP { get; }
        public int MotionFromSPToPS { get; }
    }
}
