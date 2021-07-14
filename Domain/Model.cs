using OptimalMotion3._1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptimalMotion3._1.Domain
{
    public class Model
    {
        public Model(int runwayCount, int specialPlaceCount, ITable table)
        {
            InitRunways(runwayCount);
            InitSpecialPlaces(specialPlaceCount);
            this.table = table;
        }

        private Dictionary<int, Runway> runways = new Dictionary<int, Runway>();
        private Dictionary<int, SpecialPlace> specialPlaces = new Dictionary<int, SpecialPlace>();
        private readonly ITable table;

        private void InitRunways(int runwayCount)
        {
            for (var i = 0; i < runwayCount; i++)
            {
                var runway = new Runway(i);
                runways.Add(i, runway);
            }
        }

        private void InitSpecialPlaces(int specPlatformCount)
        {
            for (var i = 0; i < specPlatformCount; i++)
            {
                var specPlatform = new SpecialPlace(i);
                specialPlaces.Add(i, specPlatform);
            }
        }
    }
}
