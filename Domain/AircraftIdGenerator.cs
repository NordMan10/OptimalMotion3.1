

namespace OptimalMotion3._1.Domain
{
    public class AircraftIdGenerator
    {
        protected AircraftIdGenerator(int id)
        {
            this.id = id;
        }

        private static AircraftIdGenerator instance;
        private static object syncRoot = new object();
        private static int initIdValue;
        private int id;

        public static AircraftIdGenerator GetInstance(int startIdValue)
        {
            initIdValue = startIdValue;
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new AircraftIdGenerator(initIdValue);
                }
            }
            return instance;
        }

        /// <summary>
        /// Сбрасывает значение Id до значения, переданного при создании
        /// </summary>
        public void Reset()
        {
            id = initIdValue;
        }

        /// <summary>
        /// Возвращает уникальный Id для ВС
        /// </summary>
        /// <returns></returns>
        public int GetUniqueAircraftId()
        {
            return id++;
        }
    }
}
