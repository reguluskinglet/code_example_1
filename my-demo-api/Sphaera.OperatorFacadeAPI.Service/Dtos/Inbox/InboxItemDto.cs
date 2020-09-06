using System;

namespace demo.DemoApi.Service.Dtos.Inbox
{
    /// <summary>
    /// Информация об элементе очереди
    /// </summary>
    public class InboxItemDto
    {
        /// <summary>
        /// Id звонка
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Время поступления элемента в очередь
        /// </summary>
        public DateTime ArrivalTime { get; set; }

        /// <summary>
        /// Информация о звонящем участнике
        /// </summary>
        public ParticipantInfoDto Caller { get; set; }
    }
}
