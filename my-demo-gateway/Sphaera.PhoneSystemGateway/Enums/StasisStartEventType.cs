namespace demo.DemoGateway.Enums
{
    /// <summary>
    /// Тип события, возникающего при звонке. Передается в аргументах событий. В БД не пишется
    /// </summary>
    public enum StasisStartEventType
    {
        /// <summary>
        /// Ответ на входящий вызов от заявителя или другого внешнего контакта
        /// </summary>
        AcceptIncomingCall,

        /// <summary>
        /// Добавление участника в звонок в режиме конференции
        /// </summary>
        Conference,

        /// <summary>
        /// Добавление участника в звонок в режиме ассистирования
        /// </summary>
        Assistant,

        /// <summary>
        /// Добавление участника в звонок в режиме частичного ассистирования
        /// </summary>
        PartialAssistant,

        /// <summary>
        /// Добавление канала в snoop-бридж
        /// </summary>
        AddToSnoopBridge,

        /// <summary>
        /// Добавление канала ассистента/частичного ассистента в snoop-бридж с возможностью говорить в бридж
        /// </summary>
        AddToSpeakSnoopBridge,

        /// <summary>
        /// Добавление нового входящего вызова.
        /// </summary>
        RegisterIncomingCallCommand,

        /// <summary>
        /// Удаление канала.
        /// </summary>
        DeleteChannelCommand,

        /// <summary>
        /// Закончить вызов.
        /// </summary>
        ForceHangUpCommand,

        /// <summary>
        /// Поменяться ролями.
        /// </summary>
        SwitchRolesCommand,

        /// <summary>
        /// Изоляции участника линии.
        /// </summary>
        IsolationCommand,

        /// <summary>
        /// Отключение микрофона участника линии.
        /// </summary>
        MuteCommand,

        /// <summary>
        /// Создать исходящий вызов от пользователя.
        /// </summary>
        CallFromUser,

        /// <summary>
        /// Звонок внешнего участника, которому звонит пользователь.
        /// </summary>
        CallToDestination,

        /// <summary>
        /// Заявитель или другой внешний участник отклонил или не ответил на вызов пользователя.
        /// </summary>
        RejectedCallFromUser,

        /// <summary>
        /// Stasis событие возникает, но его не нужно обрабатывать
        /// </summary>
        IgnoreStasisEvent,

        /// <summary>
        /// Событие начала записи
        /// </summary>
        RecordingStarted,

        /// <summary>
        /// Событие окончания записи
        /// </summary>
        RecordingEnded
    }
}
