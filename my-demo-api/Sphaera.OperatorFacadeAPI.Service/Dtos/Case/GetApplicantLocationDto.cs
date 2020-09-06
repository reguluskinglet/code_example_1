namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Информация местоположения в формате ESPG-3857.
    /// </summary>
    public class GetApplicantLocationDto
    {
        /// <summary>
        /// Широта
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Долгота
        /// </summary>
        public double? Longitude { get; set; }
    }
}
