namespace demo.DemoGateway.Dtos.Sms
{
    /// <summary>
    /// Данные, полученные из xml из sms body
    /// </summary>
    public class SmsMetadata
    {
        /// <summary>
        /// Координаты места нахождения
        /// </summary>
        public SmsPosition Position { get; set; }
        
        /// <summary>
        /// Время определения места нахождения
        /// </summary>
        public string Timestamp { get; set; }
        
        /// <summary>
        /// Радиус круга в метрах
        /// </summary>
        public int? Radius { get; set; } 
        
        /// <summary>
        /// Внутренний радиус круга в метрах.
        /// </summary>
        public int? InnerRadius { get; set; }

        /// <summary>
        /// Внешний радиус круга в метрах.
        /// </summary>
        public int? OuterRadius { get; set; }

        /// <summary>
        /// Начальный угол сектора в градусах.
        /// </summary>
        public int? StartAngle { get; set; }

        /// <summary>
        /// Угол раскрытия сектора в градусах.
        /// </summary>
        public int? OpeningAngle { get; set; }

    }
}