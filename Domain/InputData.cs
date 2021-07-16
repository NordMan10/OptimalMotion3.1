using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Domain
{
    public class InputData
    {
        public InputData(int runwayId, int plannedTakingOffMoment)
        {
            RunwayId = runwayId;
            PlannedTakingOffMoment = plannedTakingOffMoment;
        }

        /// <summary>
        /// Количество ВПП
        /// </summary>
        public int RunwayId { get; }

        /// <summary>
        /// Плановый момент вылета
        /// </summary>
        public int PlannedTakingOffMoment { get; }
    }
}
