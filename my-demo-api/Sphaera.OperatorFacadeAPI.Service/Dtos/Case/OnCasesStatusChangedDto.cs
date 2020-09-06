using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Для отправки информации через SignalR об изменении статуса Case
    /// </summary>
    public class OnCasesStatusChangedDto
    {
        /// <summary>
        /// Идентификатор CaseFolder
        /// </summary>
        public Guid CaseFolderId { get; set; }
        
        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        public Guid UserId { get; set; }
    }
}