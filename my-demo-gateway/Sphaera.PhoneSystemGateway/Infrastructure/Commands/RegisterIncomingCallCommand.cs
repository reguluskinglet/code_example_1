using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.MessageContracts.DemoGateway;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using demo.Transit.Publisher;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для сохранения и передачи информации о входящем вызове от заявителя
    /// </summary>
    public class RegisterIncomingCallCommand : BaseAsteriskCommand
    {
        private readonly IQueueSender _queueSender;

        /// <inheritdoc />
        public RegisterIncomingCallCommand(
            ILogger logger,
            IChannelRepository channelRepository,
            AsteriskAriClient ariClient,
            IQueueSender queueSender,
            IAudioRecordRepository audioRecordRepository,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
            _queueSender = queueSender;
        }

        /// <inheritdoc />
        protected override async Task InternalExecute(Channel incomingCallChannel, StasisStartEventArgs args)
        {
            var callerExtension = incomingCallChannel.Caller.Number;
            var callId = Guid.NewGuid();

            var mainBridge = await InitializeMainBridge(incomingCallChannel.Id, callerExtension, callId);
            if (mainBridge == null)
            {
                return;
            }

            var channel = new DAL.Entities.Channel
            {
                ChannelId = incomingCallChannel.Id,
                Extension = callerExtension,
                CallId = callId,
                BridgeId = mainBridge,
                Role = ChannelRoleType.ExternalChannel
            };

            var bNumber = incomingCallChannel.Dialplan.Exten;
            await _queueSender.Publish(new IncomingCallIntegrationEvent
            {
                CallId = callId,
                CallerExtension = callerExtension,
                BNumber = bNumber,
            });

            Logger.Information($"Sent message to queue about incoming call. Caller: {callerExtension}; ChannelId: {incomingCallChannel.Id}");

            await ChannelRepository.AddChannel(channel);
        }

        /// <summary>
        /// Создать главный бридж, добавить туда канал, включить приветствие ожидания и начать запись
        /// </summary>
        private async Task<string> InitializeMainBridge(string channelId, string callerExtension, Guid initialCallId)
        {
            try
            {
                await AriClient.Answer(channelId);

                var mainBridge = await AriClient.CreateBridge();
                await AriClient.AddChannelToBridge(mainBridge.Id, channelId);

                await StartCallRecording(channelId, initialCallId, callerExtension, ChannelRoleType.ExternalChannel, mainBridge.Id);

                await AriClient.StartMohInBridgeAsync(mainBridge.Id);

                Logger.Information($"InitializeMainBridge. Channel {channelId} added to bridge {mainBridge.Id}");

                return mainBridge.Id;
            }
            catch (Exception ex)
            {
                Logger.Warning($"InitializeMainBridge Error. {ex.Message}.", ex);
                return null;
            }
        }
    }
}
