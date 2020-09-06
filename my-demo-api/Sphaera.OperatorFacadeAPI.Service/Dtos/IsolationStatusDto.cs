using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для установки статуса изоляции.
    /// </summary>
    public class IsolationStatusDto
    {
        /// <summary>
        /// Id вызова.
        /// </summary>
        public Guid CallId { get; set; }
        
        /// <summary>
        /// Статус изоляции
        /// </summary>
        public bool Isolated { get; set; }
    }
}
