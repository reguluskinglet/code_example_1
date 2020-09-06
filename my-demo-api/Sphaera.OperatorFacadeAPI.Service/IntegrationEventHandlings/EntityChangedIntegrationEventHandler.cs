using System.Linq;
using System.Threading.Tasks;
using demo.DDD;
using demo.MessageContracts.TrackingEvent;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработка интеграционного события изменения сущности
    /// </summary>
    public class EntityChangedIntegrationEventHandler : BaseMessageConsumer<EntityChangedIntegrationEvent>
    {
        private readonly ILogger _logger;
        private readonly CallManagementService _callManagementService;

        /// <inheritdoc />
        public EntityChangedIntegrationEventHandler(
            ILogger logger,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <inheritdoc />
        protected override async Task Consume(EntityChangedIntegrationEvent message)
        {
            _logger.Debug("Start processing an integration event EntityChangedIntegrationEvent.");

            var trackingEvents = message.TrackingEvents.Select(x => new TrackingEvent
            {
                AggreagateId = x.AggregateId,
                EntityId = x.EntityId,
                TransactionDate = x.TransactionDate,
                EntityName = x.EntityName,
                OperationType = (EventType)x.OperationType,
                UserId = x.UserId
            });

            await _callManagementService.NotifyAboutEntityChanged(trackingEvents);
        }
    }
}
