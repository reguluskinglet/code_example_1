using demo.DemoGateway.Dtos;
using demo.DemoGateway.Enums;

namespace demo.DemoGateway.Infrastructure.HostedServices.Dtos
{
    /// <summary>
    /// Аргументы события StasisStartEvent, передаваемые при вызове
    /// </summary>
    public class StasisStartEventArgs
    {
        /// <summary>
        /// Идентификатор канала
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Идентификатор моста
        /// </summary>
        public string BridgeId { get; set; }

        /// <summary>
        /// Тип соединения с участником разговора
        /// </summary>
        public StasisStartEventType EventType { get; set; }

        /// <summary>
        /// Данные вызова, переданные с CTI сервиса
        /// </summary>
        public RouteCallDto RouteData { get; set; }
        
        /// <summary>
        /// Исходный идентификатор канала, на основе которого сделана snoop-копия
        /// </summary>
        public string OriginalChannelId { get; set; }

        /// <summary>
        /// Идентификатор проигрывания гудков ожидания ответа
        /// </summary>
        public string PlaybackId { get; set; }
    }
}
