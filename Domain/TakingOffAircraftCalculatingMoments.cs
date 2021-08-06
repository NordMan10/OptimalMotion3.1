

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
        /// Разрешенный момент взлета для резервного ВС (представляет разрешенный момент взлета, который использует резервное ВС, если
        /// все пройдет в штатном режиме и оно останется резервным (ему не придется заменять основное ВС)
        /// </summary>
        public int ReservePermittedTakingOff { get; set; }

        /// <summary>
        /// Момент старта 
        /// </summary>
        public int Start { get; set; }
    }
}
