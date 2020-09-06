namespace demo.DemoGateway.Infrastructure.Options
{
    /// <summary>
    /// Настройки подключения к серверу Asterisk.
    /// </summary>
    public class AsteriskOptions
    {
        /// <summary>
        /// Ip сервера.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Имя пользователя для подключения к Asterisk ARI
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Пароль для подключения к Asterisk ARI
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Включена ли запись разговоров
        /// </summary>
        public bool RecordingEnabled { get; set; }
    }
}