using System.Collections.Generic;
using System.Threading.Tasks;
using demo.DDD;
using demo.DDD.Events;
using demo.DemoApi.Service.ApplicationServices;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработчик событий изменения сущностей
    /// </summary>
    public class TrackingEventHandler : ITrackingEventHandler
    {
        private readonly PhoneHubMessageService _phoneHubMessageService;

        /// <summary>
        /// Создает новый экземпляр <see cref="TrackingEventHandler"/>.
        /// </summary>
        public TrackingEventHandler(PhoneHubMessageService phoneHubMessageService)
        {
            _phoneHubMessageService = phoneHubMessageService;
        }

        /// <summary>
        /// Оповещение об изменении сущностей.
        /// </summary>
        public async Task Handle(List<TrackingEvent> trackingEvents)
        {
            await _phoneHubMessageService.NotifyUsersAboutEntityChanged(trackingEvents);
        }
    }
}
