using System;

namespace demo.DemoApi.Service.Infrastructure.Options
{
    /// <summary>
    /// Настройки зон регламентного вуремени
    /// </summary>
    public class RegulatoryTimeZones
    {
        /// <summary>
        /// Начало желтой зоны
        /// </summary>
        public TimeSpan Yellow { get; set; }
        /// <summary>
        /// Начало красной зоны
        /// </summary>
        public TimeSpan Red { get; set; }
    }
}
