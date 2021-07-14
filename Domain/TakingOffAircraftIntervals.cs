using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Domain
{
    public class TakingOffAircraftIntervals
    {
        public TakingOffAircraftIntervals(
            int motionFromParkingToPS, int motionFromPSToES, int takeoff,
            int motionFromParkingToSP, int motionFromSPToPS, int processing, int psDelay)
        {
            MotionFromParkingToPS = motionFromParkingToPS;
            MotionFromPSToES = motionFromPSToES;
            TakingOff = takeoff;
            MotionFromParkingToSP = motionFromParkingToSP;
            MotionFromSPToPS = motionFromSPToPS;
            Processing = processing;
            PSDelay = psDelay;
        }

        public int MotionFromParkingToPS { get; }
        public int MotionFromPSToES { get; }
        public int TakingOff { get; }
        public int Processing { get; }
        public int MotionFromParkingToSP { get; }
        public int MotionFromSPToPS { get; }
        public int PSDelay { get; }
    }
}
