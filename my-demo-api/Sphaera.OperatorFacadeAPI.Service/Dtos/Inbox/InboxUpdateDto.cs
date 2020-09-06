using System;

namespace demo.DemoApi.Service.Dtos.Inbox
{
    /// <summary>
    /// Тело оповещения об обновленной очереди
    /// </summary>
    public class InboxUpdateDto
    {
        /// <summary>
        /// Id очереди
        /// </summary>
        public Guid InboxId { get; set; }
    }
}
