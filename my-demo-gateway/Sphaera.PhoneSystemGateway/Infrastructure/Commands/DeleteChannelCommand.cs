using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.MessageContracts.DemoGateway;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using demo.Transit.Publisher;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Удалить канал из Asterisk и из БД
    /// </summary>
    public class DeleteChannelCommand : BaseAsteriskCommand
    {
        private readonly IQueueSender _queueSender;

        /// <inheritdoc />
        public DeleteChannelCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> options,
            IQueueSender queueSender)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
            _queueSender = queueSender;
        }

        /// <inheritdoc />
        protected override async Task InternalExecute(Channel channel, StasisStartEventArgs args)
        {
            Logger.Information($"DeleteChannel. Id: {channel.Id}");

            var channelEntity = await ChannelRepository.GetByChannelId(channel.Id);
            if (channelEntity == null)
            {
                Logger.Warning($"Канал не найден или был удален ранее. ChannelId: {channel.Id}");
                return;
            }

            await NotifyIfIncomingCallEnded(channelEntity);

            await DeleteChannel(channelEntity);
            await DestroyBridge(channelEntity.BridgeId);
        }

        /// <summary>
        /// Отправить событие о том, что участник разговора, который звонил в службу завершил разговор
        /// </summary>
        private async Task NotifyIfIncomingCallEnded(DAL.Entities.Channel channelEntity)
        {
            if (channelEntity.Role == ChannelRoleType.ExternalChannel)
            {
                await _queueSender.Publish(new EndCallIntegrationEvent
                {
                    CallId = channelEntity.CallId
                });
            }
        }

        private async Task DeleteChannel(DAL.Entities.Channel channel)
        {
            if (channel.Role == ChannelRoleType.Assistant || channel.Role == ChannelRoleType.PartialAssistant || channel.Role == ChannelRoleType.SpeakSnoopChannel)
            {
                await ChannelRepository.DeleteChannel(channel.ChannelId);
                await HangUpAllChannelsInBridge(channel.BridgeId);

                return;
            }

            await ChannelRepository.DeleteChannel(channel.ChannelId);
        }

        private async Task HangUpAllChannelsInBridge(string bridgeId)
        {
            var allChannels = await ChannelRepository.GetByBridgeId(bridgeId);
            foreach (var channel in allChannels)
            {
                await AriClient.HangupChannel(channel.ChannelId);
            }
        }

        private async Task DestroyBridge(string bridgeId)
        {
            Logger.Information($"DestroyBridge: {bridgeId}");

            var allChannelsInBridge = await ChannelRepository.GetByBridgeId(bridgeId);
            if (allChannelsInBridge.Count > 1)
            {
                Logger.Debug("Bridge contains more than 1 channel");
                return;
            }

            if (allChannelsInBridge.Count == 1)
            {
                var lastChannelInBridge = allChannelsInBridge.First();
                if (lastChannelInBridge.Interrupted)
                {
                    Logger.Information($"Канал {lastChannelInBridge.ChannelId} {lastChannelInBridge.Role} ожидает ответа.");
                    return;
                }

                await AriClient.HangupChannel(lastChannelInBridge.ChannelId);
                return;
            }

            AriClient.DestroyBridge(bridgeId);
            await DestroyRecordingBridge(bridgeId);
        }

        private async Task DestroyRecordingBridge(string bridgeId)
        {
            var commonRecordsBridgeId = GetCommonRecordingBridgeId(bridgeId);
            var audioRecordCommon = await AudioRecordRepository.GetRecordByName(commonRecordsBridgeId);
            if (audioRecordCommon == null)
            {
                return;
            }

            var recordingBridgeId = GetCommonRecordingBridgeId(bridgeId);
            AriClient.DestroyBridge(recordingBridgeId);
        }
    }
}