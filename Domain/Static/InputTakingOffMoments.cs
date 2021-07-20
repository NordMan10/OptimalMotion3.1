using System.Collections.Generic;

namespace OptimalMotion3._1.Domain.Static
{
    public static class InputTakingOffMoments
    {
        public static int PlannedMomentsStep { get; } = 90;
        public static int PermittedMomentsStep { get; } = 90;

        public static List<int> PlannedMoments { get; } = new List<int>
        {
            600, 630, 680, 700, 750, 1040, 1290, 1310, 1500, 1580
        };

        // Когда будешь работать с ними, просто упорядоч их
        public static List<int> PermittedMoments { get; } = new List<int>
        {
            660, 750, 790, 850, 880, 940, 1060, 1120, 1200, 1280, 1670, 1700, 1760, 1800, 1900, 2000, 2090, 2150, 2240, 2390, 2500
        };

        public static void AddTakingOffMoments(int plannedMoment, int permittedMoment)
        {
            PlannedMoments.Add(plannedMoment);
            PermittedMoments.Add(permittedMoment);
        }
    }
}
