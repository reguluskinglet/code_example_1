using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Статус одного Case
    /// </summary>
    public class CaseStatusDto
    {
        /// <summary>
        /// Идентификатор Case
        /// </summary>
        public Guid CaseCardId { get; set; }
        
        /// <summary>
        /// Case статус
        /// </summary>
        public string Status { get; set; }
    }
}