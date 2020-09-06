using System;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Дто подключенной линии
    /// </summary>
    public class ConnectedLineDto
    {
        /// <summary>
        /// Id линии
        /// </summary>
        public Guid LineId { get; set; }
        /// <summary>
        /// Режим подключения
        /// </summary>
        public ConnectionMode Mode { get; set; }
    }
}
