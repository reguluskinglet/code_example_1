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
    /// Команда для сохранения данных о канале участника разговора, который принял вызов от пользователя
    /// </summary>
    public class AcceptedCallFromUserCommand : BaseAsteriskCommand
    {
        private readonly IQueueSender _queueSender;

        /// <inheritdoc />
        public AcceptedCallFromUserCommand(ILogger logger,
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
        protected override async Task InternalExecute(Channel destinationChannel, StasisStartEventArgs args)
        {
            var routeData = args.RouteData;

            Logger.Information($"AcceptedCallFromUserCommand. DestinationChannelId: {destinationChannel.Id}, UserChannelId: {args.ChannelId}");

            try
            {
                var destinationExtension = routeData.ToExtension;
                var destinationChannelEntity = await ChannelRepository.GetByChannelId(destinationChannel.Id);
                if (destinationChannelEntity == null)
                {
                    Logger.Warning($"Канал вызываемого участника не найден. ChannelId: {destinationChannel.Id}");
                    return;
                }

                var channelsInBridge = await ChannelRepository.GetByBridgeId(destinationChannelEntity.BridgeId);
                var userChannel = channelsInBridge.SingleOrDefault(x => x.Role == ChannelRoleType.Conference);
                if (userChannel == null)
                {
                    Logger.Warning($"Канал пользователя не найден. LineId: {routeData.LineId}");
                    return;
                }

                var callId = destinationChannelEntity.CallId;

                await StartCallRecording(userChannel.ChannelId, userChannel.CallId, userChannel.Extension, userChannel.Role, userChannel.BridgeId, routeData.LineId);
                await InitializeRecordingChannel(destinationChannel.Id, destinationExtension, destinationChannelEntity.Role, userChannel.BridgeId, callId, routeData.LineId);

                destinationChannelEntity.Role = ChannelRoleType.ExternalChannel;
                await ChannelRepository.UpdateChannel(destinationChannelEntity);

                await AriClient.StopBeeps(args.PlaybackId);
                await AriClient.AddChannelToBridge(userChannel.BridgeId, destinationChannel.Id);

                Logger.Information($"Участник разговора {destinationExtension} принял вызов от пользователя {userChannel.Extension}");
                await _queueSender.Publish(new AcceptCallFromUserIntegrationEvent
                {
                    CallId = callId
                });
            }
            catch (Exception ex)
            {
                Logger.Warning("AcceptedCallFromUserCommand Error", ex);
            }
        }
    }
}
