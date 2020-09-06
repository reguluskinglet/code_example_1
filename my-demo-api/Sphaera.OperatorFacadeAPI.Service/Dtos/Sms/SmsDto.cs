namespace demo.DemoApi.Service.Dtos.Sms
{
    public class SmsDto
    {
        /// <summary>
        /// Текстовое сообщение входящего смс-сообщения.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Информация о местоположении абонента.
        /// </summary>
        public SmsLocationDto Location { get; set; }

        /// <summary>
        /// Дата и время полученное из метаданных смс-сообщения.
        /// </summary>
        public string Timestamp { get; set; }
    }
}
