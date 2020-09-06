using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Receiver
{
    /// <summary>
    /// Dto списка получателей, разбитого по типам
    /// </summary>
    public class ReceiversResponseDto
    {
        /// <summary>
        /// Список получателей для подключения пользователей
        /// </summary>
        public List<ReceiverDto> SingleReceivers { get; set; }

        /// <summary>
        /// Список получателей для подключения групп
        /// </summary>
        public List<ReceiverDto> GroupReceivers { get; set; }
    }
}
