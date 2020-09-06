using System;

namespace demo.DemoGateway.Dtos
{
    /// <summary>
    /// Dto для установки статуса изоляции.
    /// </summary>
    public class IsolationStatusDto
    {
        /// <summary>
        /// Id звонка.
        /// </summary>
        public Guid CallId { get; set; }
        
        /// <summary>
        /// Статус изоляции.
        /// </summary>
        public bool Isolated { get; set; }
    }
}