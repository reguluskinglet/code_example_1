using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для установки статуса удержания.
    /// </summary>
    public class HoldStatusDto
    {
        /// <summary>
        /// Id вызова.
        /// </summary>
        public Guid CallId { get; set; }
        
        /// <summary>
        /// Статус удержания
        /// </summary>
        public bool OnHold { get; set; }
    }
}
