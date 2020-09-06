namespace demo.DemoGateway.DAL.Entities
{
    /// <summary>
    /// Тип роли канала
    /// </summary>
    public enum ChannelRoleType
    {
        /// <summary>
        /// Роль не определена
        /// </summary>
        None = 0,

        /// <summary>
        /// Участник в режиме конференции
        /// </summary>
        Conference = 1,

        /// <summary>
        /// Главный в разговоре
        /// </summary>
        MainUser = 2,

        /// <summary>
        /// Ассистент
        /// </summary>
        Assistant = 3,

        /// <summary>
        /// Участник в режиме частичного ассистирования
        /// </summary>
        PartialAssistant = 4,

        /// <summary>
        /// Канал заявителя или другого участника, совершившего входящий вызов в службу или ответивший на вызов от пользователя
        /// </summary>
        ExternalChannel = 5,

        /// <summary>
        /// Snoop-канал
        /// </summary>
        SnoopChannel = 6,

        /// <summary>
        /// Snoop-канал с возможностью говорить в бридж
        /// </summary>
        SpeakSnoopChannel = 7,

        /// <summary>
        /// Канал предполагаемого участника разговора, который еще не принял вызов от пользователя.
        /// Канал с такой ролью существует только пока пользователь ожидает ответа от участника
        /// </summary>
        RingingFromUser = 8
    }
}
