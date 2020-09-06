using System.Threading.Tasks;
using demo.MessageContracts.PhoneSystemGateway;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработка интеграционного события, возникающего когда заявитель не ответил или отклонил вызов оператора при перезвоне
    /// </summary>
    public class RejectCallIntegrationEventHandler : BaseMessageConsumer<RejectCallIntegrationEvent>
    {
        private readonly CallManagementService _callManagementService;
        private readonly ILogger _logger;

        /// <summary>
        /// Создает новый экземпляр <see cref="RejectCallIntegrationEventHandler"/>.
        /// </summary>
        public RejectCallIntegrationEventHandler(
            ILogger logger,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <summary>
        /// Событие о том, что заявитель отклонил или не ответил на вызов оператора.
        /// </summary>
        protected override async Task Consume(RejectCallIntegrationEvent message)
        {
            _logger.Debug($"Start processing an integration event {nameof(RejectCallIntegrationEvent)}.");
            await _callManagementService.RejectCallByExternalCaller(message.CallId);
        }
    }
}
