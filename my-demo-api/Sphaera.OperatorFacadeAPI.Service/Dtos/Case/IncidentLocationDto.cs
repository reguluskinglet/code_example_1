using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Координаты места происшествия
    /// </summary>
    public class IncidentLocationDto
    {
        /// <inheritdoc />
        public IncidentLocationDto(Guid caseFolderId, double? latitude, double? longitude)
        {
            CaseFolderId = caseFolderId;
            Latitude = latitude;
            Longitude = longitude;
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
    }
}
