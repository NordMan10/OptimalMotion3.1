

namespace OptimalMotion3._1.Interfaces
{
    /// <summary>
    /// Интерфейс для ВС
    /// </summary>
    public interface IAircraft
    {
        int Id { get; }

        string Type { get; }
        
        string RunwayId { get; }
    }
}
