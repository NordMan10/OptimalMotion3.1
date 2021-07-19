using System.ComponentModel;

namespace OptimalMotion3._1.Domain
{
    public class TableRow
    {
        public TableRow(string aircraftId, string startMoment, string plannedTakingOffMoment, string actualTakingOffMoment, 
            string permittedTakingOffMoment, bool needProcessing, bool isReserved, string runwayId, string specialPlaceId)
        {
            AircraftId = aircraftId;
            StartMoment = startMoment;
            ActualTakingOffMoment = actualTakingOffMoment;
            PlannedTakingOffMoment = plannedTakingOffMoment;
            PermittedTakingOffMoment = permittedTakingOffMoment;
            NeedProcessing = needProcessing;
            IsReserved = isReserved;
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
        }

        [DisplayName("Id ВС")]
        public string AircraftId { get; }

        [DisplayName("Tстарт.")]
        public string StartMoment { get; }

        [DisplayName("Тплан. взлет.")]
        public string PlannedTakingOffMoment { get; }

        [DisplayName("Тфакт. взлет.")]
        public string ActualTakingOffMoment { get; }

        [DisplayName("Tразр. взлет.")]
        public string PermittedTakingOffMoment { get; }

        [DisplayName("Обработка")]
        public bool NeedProcessing { get; }

        [DisplayName("Резерв")]
        public bool IsReserved { get; }

        [DisplayName("Id ВПП")]
        public string RunwayId { get; }

        [DisplayName("Id Спец. площадки")]
        public string SpecialPlaceId { get; }
    }
}
