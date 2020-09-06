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
    /// Команда для обработки ситуации, когда предполагаемый участник разговора не ответил или отклонил вызов пользователя
    /// </summary>
    public class RejectedCallFromUserCommand : BaseAsteriskCommand
    {
        private readonly IQueueSender _queueSender;

        /// <inheritdoc />
        public RejectedCallFromUserCommand(ILogger logger,
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
            var channelId = channel.Id;
            Logger.Information($"RejectedCallFromUserCommand. DestinationChannelId: {channel.Id}");

            try
            {
                var destinationChannel = await ChannelRepository.GetByChannelId(channelId);
                if (destinationChannel == null || destinationChannel.Role != ChannelRoleType.RingingFromUser)
                {
                    Logger.Debug($"RejectedCallFromUserCommand. Канал участника уничтожен. CallId: {destinationChannel?.CallId}. {destinationChannel?.Role}");
                    return;
                }

                await ChannelRepository.DeleteChannel(destinationChannel.ChannelId);

                var channelsInBridge = await ChannelRepository.GetByBridgeId(destinationChannel.BridgeId);
                var userChannel = channelsInBridge.SingleOrDefault(x => x.Role == ChannelRoleType.Conference);
                if (userChannel == null)
                {
                    Logger.Warning("RejectedCallFromUserCommand. Канал пользователя не найден.");
                    return;
                }

                await AriClient.HangupChannel(userChannel.ChannelId);

                Logger.Information($"Отправка информации о том, что участник не принял или отклонил вызов от пользователя. Destination: {destinationChannel.Extension}");
                await _queueSender.Publish(new RejectCallIntegrationEvent
                {
                    CallId = destinationChannel.CallId
                });
            }
            catch (Exception ex)
            {
                Logger.Warning("RejectedCallFromUserCommand Error", ex);
            }
        }
    }
}
