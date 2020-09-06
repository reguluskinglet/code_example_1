using System;
using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Inbox
{
    /// <summary>
    /// Dto с данными об очереди
    /// </summary>
    public class InboxDto
    {
        /// <summary>
        /// Id очереди.
        /// </summary>
        public Guid InboxId { get; set; }

        /// <summary>
        /// Имя очереди.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Статус регламентного времени.
        /// </summary>
        public string RegulatoryTimeStatus { get; set; }

        /// <summary>
        /// Количество элементов в очереди.
        /// </summary>
        public int ItemsCount { get; set; }

        /// <summary>
        /// Информация о самом старом звонке
        /// </summary>
        public InboxItemDto OldestInboxItem { get; set; }

        /// <summary>
        /// Информация о звонках
        /// </summary>
        public List<InboxItemDto> InboxItems { get; set; }
    }
}