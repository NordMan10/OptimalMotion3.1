

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Набор входных данных, необходимый для каждого ВС
    /// </summary>
    public class AircraftInputData
    {
        public AircraftInputData(string runwayId, int specialPlaceId, string type, int priority,
            bool processingIsNeeded, TakingOffAircraftCreationIntervals creationIntervals, TakingOffAircraftCreationMoments creationMoments)
        {
            RunwayId = runwayId;
            SpecialPlaceId = specialPlaceId;
            Type = type;
            Priority = priority;
            ProcessingIsNeeded = processingIsNeeded;
            CreationIntervals = creationIntervals;
            CreationMoments = creationMoments;
        }

        public string RunwayId { get; }

        public int SpecialPlaceId { get; }
        
        public string Type { get; }
        
        public int Priority { get; }
        
        public bool ProcessingIsNeeded { get; }
        
        public TakingOffAircraftCreationIntervals CreationIntervals { get; }
        
        public TakingOffAircraftCreationMoments CreationMoments { get; }
    }
}
