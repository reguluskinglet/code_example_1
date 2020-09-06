using System;
using Newtonsoft.Json;

namespace demo.DemoApi.Domain.Entities.SmsMetadata
{
    /// <summary>
    /// Информация о местоположении, пришедшая из СМС
    /// </summary>
    public class SmsLocationData
    {
        /// <summary>
        /// Координаты местоположения
        /// </summary>
        public SmsPositionData Position { get; set; }

        /// <summary>
        /// Радиус круга в метрах.
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

        /// <summary>
        /// Форматированная информация о местоположении заявителя.
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public virtual string GetFormattedLocationInfoString =>
            string.Format("<tr><td>R:</td><td>{0}</td></tr><tr><td>iR:</td><td>{1}</td></tr><tr><td>oR:</td><td>{2}</td></tr><tr><td>sA:</td><td>{3}</td></tr><tr><td>oA:</td><td>{4}</td></tr>",
                Radius.HasValue ? Convert.ToString(Radius) : "н/д",
                InnerRadius.HasValue ? Convert.ToString(InnerRadius) : "н/д",
                OuterRadius.HasValue ? Convert.ToString(OuterRadius) : "н/д",
                StartAngle.HasValue ? Convert.ToString(StartAngle) : "н/д",
                OpeningAngle.HasValue ? Convert.ToString(OpeningAngle) : "н/д");

        /// <summary>
        /// Создать экземпляр текущей сущности с пустыми полями
        /// </summary>
        /// <returns></returns>
        public static SmsLocationData CreateEmpty()
        {
            return new SmsLocationData
            {
                Position = new SmsPositionData()
            };
        }
        
        /// <summary>
        /// Проверка на наличие широты и долготы местоположения заявителя
        /// </summary>
        public virtual bool NotEmpty() => Position.Latitude != default && Position.Longitude != default;
    }
}
