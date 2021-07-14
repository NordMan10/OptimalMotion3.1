using OptimalMotion3._1.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimalMotion3._1.Interfaces
{
    public interface IAircraft
    {
        int Id { get; }
        AircraftTypes Type { get; }
        int RunwayId { get; }
    }
}
