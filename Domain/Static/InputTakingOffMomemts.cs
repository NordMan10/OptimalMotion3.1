using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Domain.Static
{
    public static class InputTakingOffMomemts
    {
        public static List<int> PlannedMoments = new List<int>
        {
            600, 660, 750, 900, 950, 1040, 1290, 1310, 1500
        };

        // Когда будешь работать с ними, просто упорядач их
        public static List<int> PermittedMoments = new List<int>
        {
            660, 690, 850, 1020, 1050, 1040, 1260, 1320, 1500
        };
    }
}
