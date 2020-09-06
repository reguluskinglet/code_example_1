using System;

namespace demo.DemoGateway.Client.Dto
{
    /// <summary>
    /// Dto для установки статуса изоляции.
    /// </summary>
    public class SetIsolationStatusClientDto
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