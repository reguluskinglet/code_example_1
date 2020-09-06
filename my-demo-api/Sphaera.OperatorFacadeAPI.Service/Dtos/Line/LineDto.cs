using System;
using demo.DemoApi.Service.Dtos.Calls;

namespace demo.DemoApi.Service.Dtos.Line
{
    /// <summary>
    /// Dto линии вызова.
    /// </summary>
    public class LineDto
    {
        /// <summary>
        /// Идентификатор линии.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Дата создания линии вызова.
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Идентификатор инцидента.
        /// </summary>
        public Guid? CaseFolderId { get; set; }

        /// <summary>
        /// Входящий вызов от заявителя или другого участника разговора, звонящего в службу
        /// </summary>
        public Guid? ExternalCallerId { get; set; }
    }
}
