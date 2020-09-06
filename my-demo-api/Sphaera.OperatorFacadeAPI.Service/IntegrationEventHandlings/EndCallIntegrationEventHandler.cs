using System.Threading.Tasks;
using demo.MessageContracts.PhoneSystemGateway;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработка интеграционного события завершения звонка
    /// </summary>
    public class EndCallIntegrationEventHandler : BaseMessageConsumer<EndCallIntegrationEvent>
    {
        private readonly CallManagementService _callManagementService;
        private readonly ILogger _logger;

        /// <summary>
        /// Создает новый экземпляр <see cref="EndCallIntegrationEventHandler"/>.
        /// </summary>
        public EndCallIntegrationEventHandler(
            ILogger logger,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <summary>
        /// Событие о том, что заявитель завершил звонок.
        /// </summary>
        protected override async Task Consume(EndCallIntegrationEvent message)
        {
            _logger.Debug($"Start processing an integration event {nameof(EndCallIntegrationEvent)}.");
            await _callManagementService.EndCallByExternalCaller(message.CallId);
        }
    }
}
