using System.Collections.Generic;
using System.Threading.Tasks;
using demo.MessageContracts.InboxDistribution;
using demo.MessageContracts.InboxDistribution.Enums;
using demo.Monitoring.Logger;
using demo.Monitoring.Metrics;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Helpers;
using demo.Transit.Consumer;

namespace demo.DemoApi.Service.IntegrationEventHandlings
{
    /// <summary>
    /// Эвент для пришедшего в Inbox события
    /// </summary>
    public class IncomingInboxIntegrationEventHandler : BaseMessageConsumer<IncomingInboxIntegrationEvent>
    {
        private readonly CallManagementService _callManagementService;
        private readonly IMetric _metric;
        private readonly ILogger _logger;

        /// <summary>
        /// Создает новый экземпляр <see cref="IncomingInboxIntegrationEventHandler"/>.
        /// </summary>
        public IncomingInboxIntegrationEventHandler(
            ILogger logger,
            CallManagementService callManagementService,
            IMetric metric)
        {
            _logger = logger;
            _callManagementService = callManagementService;
            _metric = metric;
        }

        /// <inheritdoc/>
        protected override async Task Consume(IncomingInboxIntegrationEvent message)
        {
            if (message?.CallerExtension == null)
            {
                _logger.Warning($"Требуется {nameof(message.CallerExtension)} on IncomingInboxIntegrationEvent");
                return;
            }

            _logger.Debug($"Start processing an integration event {nameof(IncomingInboxIntegrationEvent)}.",
                new Dictionary<string, string>
                {
                    { nameof(message.ContractInboxItemType), $"{message.ContractInboxItemType}" },
                    { nameof(message.CallerExtension), $"{message.CallerExtension}" }
                });

            await _callManagementService.AddIncomingCall(message);

            if (message.ContractInboxItemType == ContractInboxItemType.Sms)
            {
                _metric.SmsAdded();
            }
            else
            {
                _metric.CallAdded();
            }
        }
    }
}
