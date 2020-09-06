namespace demo.DemoGateway.Enums
{
    /// <summary>
    /// Тип бриджа для прослушивания канала
    /// </summary>
    public enum SnoopBridgeType
    {
        /// <summary>
        /// Тип бриджа не определен
        /// </summary>
        None = 0,

        /// <summary>
        /// Канал, который который инициировал создание бриджа, может только говорить в бридж
        /// </summary>
        Speak = 1,

        /// <summary>
        /// Канал, который который инициировал создание бриджа, может только слышать звук из бриджа
        /// </summary>
        Listen = 2
    }
}
