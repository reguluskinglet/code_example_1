namespace demo.DemoApi.Domain.Entities.SmsMetadata
{
    /// <summary>
    /// Данные смс-сообщения.
    /// </summary>
    public class SmsMessageData
    {
        /// <summary>
        /// Текстовое сообщение входящего смс-сообщения.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Координаты местоположения
        /// </summary>
        public SmsLocationData Location { get; set; }
        
        /// <summary>
        /// Дата и время полученное из метаданных смс-сообщения.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
