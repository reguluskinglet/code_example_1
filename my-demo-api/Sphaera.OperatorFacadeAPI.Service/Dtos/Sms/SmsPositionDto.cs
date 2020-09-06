namespace demo.DemoApi.Service.Dtos.Sms
{
    /// <summary>
    /// Координаты местоположения заявителя.
    /// </summary>
    public class SmsPositionDto
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
