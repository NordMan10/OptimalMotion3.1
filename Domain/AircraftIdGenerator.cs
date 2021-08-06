

namespace OptimalMotion3._1.Domain
{
    /// <summary>
    /// Класс для генерации уникальных Id для ВС. Синглтон
    /// </summary>
    public class AircraftIdGenerator
    {
        protected AircraftIdGenerator(int initIdValue)
        {
            id = initIdValue;
            this.initIdValue = initIdValue;
        }

        private static AircraftIdGenerator instance;
        private static object syncRoot = new object();
        private int initIdValue;
        private int id;

        /// <summary>
        /// Возвращает экземпляр класса. Если экземпляр уже был создан, возвращает ссылку на него
        /// </summary>
        /// <param name="startIdValue"></param>
        /// <returns></returns>
        public static AircraftIdGenerator GetInstance(int initIdValue)
        {
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
