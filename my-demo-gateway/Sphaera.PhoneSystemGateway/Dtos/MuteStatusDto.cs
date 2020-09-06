using System;

namespace demo.DemoGateway.Dtos
{
    /// <summary>
    /// Dto для установки статуса отключения звука.
    /// </summary>
    public class MuteStatusDto
    {
        /// <summary>
        /// Id звонка.
        /// </summary>
        public Guid CallId { get; set; }
        
        /// <summary>
        /// Статус отключения звука.
        /// </summary>
        public bool Muted { get; set; }
    }
}