using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Inbox
{
    /// <summary>
    /// Dto с ответом на запрос с очередями.
    /// </summary>
    public class InboxesResponseDto
    {
        /// <summary>
        /// Коллекция очередей.
        /// </summary>
        public List<InboxDto> Inboxes { get; set; }
    }
}