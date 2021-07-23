

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Рассчитываемые моменты для взлетающего ВС
    /// </summary>
    public class TakingOffAircraftCalculatingMoments
    {
        public TakingOffAircraftCalculatingMoments() { }

        /// <summary>
        /// Возможный момент взлета
        /// </summary>
        public int PossibleTakingOff { get; set; }

        /// <summary>
        /// Разрешенный момент взлета
        /// </summary>
        public int PermittedTakingOff { get; set; }

        /// <summary>
        /// Разрешенный момент взлета для резервного ВС (представляет разрешенный момент взлета для резервного ВС, если он остается 
        /// резервным, а не заменяет основное ВС
        /// </summary>
        public int ReservePermittedTakingOff { get; set; }

        /// <summary>
        /// Момент старта 
        /// </summary>
        public int Start { get; set; }
    }
}
