using System.ComponentModel;

namespace OptimalMotion3._1.Domain
{
    public class TableRow
    {
        public TableRow(string aircraftId, string actualTakingOffMoment, string plannedTakingOffMoment, bool needProcessing,
            string runwayId, string specialPlaceId)
        {
            AircraftId = aircraftId;
            ActualTakingOffMoment = actualTakingOffMoment;
            PlannedTakingOffMoment = plannedTakingOffMoment;
            NeedProcessing = needProcessing;
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
        }

        [DisplayName("Id ВС")]
        public string AircraftId { get; }

        [DisplayName("Тплан. взлет")]
        public string PlannedTakingOffMoment { get; }

        [DisplayName("Тфакт. взлет")]
        public string ActualTakingOffMoment { get; }

        [DisplayName("Обработка")]
        public bool NeedProcessing { get; }

        [DisplayName("Id ВПП")]
        public string RunwayId { get; }

        [DisplayName("Id Спец. площадки")]
        public string SpecialPlaceId { get; }
    }
}
