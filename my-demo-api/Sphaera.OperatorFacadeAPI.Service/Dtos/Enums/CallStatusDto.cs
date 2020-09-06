namespace demo.DemoApi.Service.Dtos.Enums
{
    /// <summary>
    /// Статус звонка для фронта.
    /// </summary>
    public enum CallStatusDto
    {
        /// <summary>
        /// Статус неизвестен.
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Звонок изолирован.
        /// </summary>
        IsIsolated = 1,
        
        /// <summary>
        /// Выключен ли микрофон.
        /// </summary>
        IsMutedMicrophone = 2,
        
        /// <summary>
        /// Звонок на удержании
        /// </summary>
        IsOnHold = 3,

        /// <summary>
        /// Ожидание
        /// </summary>
        CallPending = 4,

        /// <summary>
        /// Вызов Активен
        /// </summary>
        CallActive = 5,

        /// <summary>
        /// Вызов окончен
        /// </summary>
        CallEnd = 6
    }
}
