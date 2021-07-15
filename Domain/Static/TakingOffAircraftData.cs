using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Domain.Static
{
    public static class TakingOffAircraftData
    {
        public static int MotionFromParkingToPS = 240;
        public static int MotionFromPSToES = 40;
        public static int MotionFromParkingToSP = 120;
        public static int MotionFromSPToPS = 120;
        public static int TakingOffInterval = 30;
        public static int ProcessingTime { get; set; } = 240;

    }
}
