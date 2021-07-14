using System.ComponentModel;

namespace OptimalMotion3._1.Domain
{
    public class TableRow
    {
        public TableRow(string aircraftId, string takingOffMoment, string planningTakingOffMoment, string actualTakingOffMoment, bool needProcessing)
        {
            AircraftId = aircraftId;
            PlannedTakingOffMoment = takingOffMoment;
            PlannedTakingOffMoment = planningTakingOffMoment;
            ActualTakingOffMoment = actualTakingOffMoment;
            NeedProcessing = needProcessing;
        }

        [DisplayName("Id ВС")]
        public string AircraftId { get; }

        [DisplayName("Тплан. взлет")]
        public string PlannedTakingOffMoment { get; }

        [DisplayName("Тфакт. взлет")]
        public string ActualTakingOffMoment { get; }

        [DisplayName("Обработка")]
        public bool NeedProcessing { get; }
    }
}
