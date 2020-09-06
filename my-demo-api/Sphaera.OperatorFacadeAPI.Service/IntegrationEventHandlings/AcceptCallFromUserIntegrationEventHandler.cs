using System.Threading.Tasks;
using demo.MessageContracts.PhoneSystemGateway;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработка интеграционного события, возникающего когда заявитель принял вызов при перезвоне пользователю
    /// </summary>
    public class AcceptCallFromUserIntegrationEventHandler : BaseMessageConsumer<AcceptCallFromUserIntegrationEvent>
    {
        private readonly CallManagementService _callManagementService;
        private readonly ILogger _logger;

        /// <summary>
        /// Создает новый экземпляр <see cref="AcceptCallFromUserIntegrationEventHandler"/>.
        /// </summary>
        public AcceptCallFromUserIntegrationEventHandler(
            ILogger logger,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <summary>
        /// Событие о том, что заявитель принял вызов от пользователя.
        /// </summary>
        protected override async Task Consume(AcceptCallFromUserIntegrationEvent message)
        {
            _logger.Debug($"Start processing an integration event {nameof(AcceptCallFromUserIntegrationEvent)}.");
            await _callManagementService.ProcessAcceptedCallFromUser(message.CallId);
        }
    }
}
