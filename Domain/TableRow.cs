using System.ComponentModel;

namespace OptimalMotion3._1.Domain
{
    public class TableRow
    {
        public TableRow(string aircraftId, string plannedTakingOffMoment, string actualTakingOffMoment, 
            string permittedTakingOffMoment, string startMoment, string totalMotionTime, string processingTime, bool 
            needProcessing, bool isReserved, string runwayId, string specialPlaceId)
        {
            AircraftId = aircraftId;
            ActualTakingOffMoment = actualTakingOffMoment;
            PlannedTakingOffMoment = plannedTakingOffMoment;
            PermittedTakingOffMoment = permittedTakingOffMoment;
            StartMoment = startMoment;
            TotalMotionTime = totalMotionTime;
            ProcessingTime = processingTime;
            NeedProcessing = needProcessing;
            IsReserved = isReserved;
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
        }

        [DisplayName("Id ВС")]
        public string AircraftId { get; }

        [DisplayName("Тплан. взлет.")]
        public string PlannedTakingOffMoment { get; }

        [DisplayName("Тфакт. взлет.")]
        public string ActualTakingOffMoment { get; }

        [DisplayName("Tразр. взлет.")]
        public string PermittedTakingOffMoment { get; }

        [DisplayName("Tстарт.")]
        public string StartMoment { get; }

        [DisplayName("Суммарное время движения")]
        public string TotalMotionTime { get; }

        [DisplayName("Время обработки")]
        public string ProcessingTime { get; }

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
