namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Дополнительная информация о местоположении абонента.
    /// </summary>
    public class ApplicantLocationInfoDto
    {
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
    }
}