namespace demo.DemoApi.Domain.Entities.SmsMetadata
{
    /// <summary>
    /// Координаты местоположения
    /// </summary>
    public class SmsPositionData
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
