namespace demo.DemoApi.Service.Dtos.Enums
{
    /// <summary>
    /// Тип звонка
    /// </summary>
    public enum CallType
    {
        /// <summary>
        /// Неизвестный
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Входящий
        /// </summary>
        Incoming = 1,

        /// <summary>
        /// Исходящий
        /// </summary>
        Outgoing = 2,

        /// <summary>
        /// Ответ не получен 
        /// </summary>
        Unanswered = 3,

        /// <summary>
        /// Пропущенный
        /// </summary>
        Missed = 4
    }
}
