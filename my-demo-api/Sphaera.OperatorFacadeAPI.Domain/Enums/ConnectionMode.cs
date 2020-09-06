namespace demo.DemoApi.Domain.Enums
{
    /// <summary>
    /// Режим подключения участника разговора
    /// </summary>
    public enum ConnectionMode
    {
        /// <summary>
        /// Неизвестный
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Главный в разговоре
        /// </summary>
        MainUser = 1,

        /// <summary>
        /// Ассистирование
        /// </summary>
        Assistance = 2,

        /// <summary>
        /// Частичное ассистирование
        /// </summary>
        PartialAssistance = 3,

        /// <summary>
        /// Прослушивание
        /// </summary>
        Listening = 4,

        /// <summary>
        /// Конференция
        /// Выставляется по умолчанию, когда пользователь принимает внешний вызов
        /// </summary>
        Conference = 5
    }
}
