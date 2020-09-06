namespace demo.DemoGateway.Dtos.Sms
{
    /// <summary>
    /// Координаты местоположения
    /// </summary>
    public class SmsPosition
    {
        /// <summary>
        /// Данные о широте
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Данные о долготе
        /// </summary>
        public double Longitude { get; set; }
    }
}