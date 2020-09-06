using System.Threading.Tasks;
using demo.MessageContracts.InboxDistribution;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Обработка интеграционного события обнволение очереди
    /// </summary>
    public class UpdateInboxIntegrationEventHandler : BaseMessageConsumer<InboxUpdateEvent>
    {
        private readonly PhoneHubMessageService _callManagementService;
        private readonly ILogger _logger;

        /// <summary>
        /// Создает новый экземпляр.
        /// </summary>
        public UpdateInboxIntegrationEventHandler(
            ILogger logger,
            PhoneHubMessageService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <inheritdoc />
        protected override async Task Consume(InboxUpdateEvent message)
        {
            _logger.Debug($"Start processing an integration event {nameof(UpdateInboxIntegrationEventHandler)}.");
            await _callManagementService.NotifyUsersAboutInboxUpdate(message.InboxId);
        }
    }
}
