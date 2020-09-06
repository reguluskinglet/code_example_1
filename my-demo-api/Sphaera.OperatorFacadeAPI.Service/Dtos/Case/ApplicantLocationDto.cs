using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Координаты местоположения абонента.
    /// </summary>
    public class ApplicantLocationDto
    {
        /// <summary>
        /// Создание объекта с информацией местоположения абонента.
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="locationAttributes"></param>
        public ApplicantLocationDto(Guid caseFolderId,
            double? latitude,
            double? longitude,
            ApplicantLocationInfoDto locationAttributes)
        {
            CaseFolderId = caseFolderId;
            Latitude = latitude;
            Longitude = longitude;
            LocationInfo = locationAttributes;
        }

        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Широта
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Долгота
        /// </summary>
        public double? Longitude { get; set; }
        
        /// <summary>
        /// Дополнительная информация о местоположении абонента.
        /// </summary>
        public ApplicantLocationInfoDto LocationInfo { get; set; }
    }
}