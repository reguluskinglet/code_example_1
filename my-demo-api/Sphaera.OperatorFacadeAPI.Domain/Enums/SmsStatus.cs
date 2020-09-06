namespace demo.DemoApi.Domain.Enums
{
    /// <summary>
    /// Статус смс.
    /// </summary>
    public enum SmsStatus
    {
        /// <summary>
        /// Смс без статуса.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Новая смс.
        /// </summary>
        New = 1,
        
        /// <summary>
        /// Смс принята.
        /// </summary>
        Accepted = 2,
    }
}