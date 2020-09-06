using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using demo.DemoGateway.Serialization;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для сохранения информации о вызове от пользователя на указанный Extension.
    /// </summary>
    /// <remarks>Например, для перезвона заявителю или для вызова от пользователя к контакту из адресной книги</remarks>
    public class CallFromUserCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public CallFromUserCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
        }

        /// <inheritdoc />
        protected override async Task InternalExecute(Channel userChannel, StasisStartEventArgs args)
        {
            var routeData = args.RouteData;
            if (!routeData.FromCallId.HasValue)
            {
                Logger.Warning($"Id вызова пользователя не задан. ChannelId: {userChannel.Id}");
                return;
            }

            var userExtension = routeData.FromExtension;
            var destinationExtension = routeData.ToExtension;
            var destinationChannelId = AriClient.CreateChannelId(ChannelRoleType.ExternalChannel, destinationExtension);

            var bridge = await AriClient.CreateBridge();
            await AriClient.AddChannelToBridge(bridge.Id, userChannel.Id);

            var playBackId = await AriClient.PlayBeeps(userChannel.Id);

            var destinationCallArgs = new StasisStartEventArgs
            {
                BridgeId = bridge.Id,
                EventType = StasisStartEventType.CallToDestination,
                RouteData = routeData,
                PlaybackId = playBackId
            };

            var encodedArgs = JsonSerializer.EncodeData(destinationCallArgs);
            var originateResult = await AriClient.Originate(encodedArgs, "Служба 112", destinationExtension, destinationChannelId);
            if (!originateResult)
            {
                throw new Exception("Ошибка создания канала для участника разговора, которому звонит пользователь.");
            }

            var userChannelEntity = new DAL.Entities.Channel
            {
                ChannelId = userChannel.Id,
                Extension = userExtension,
                CallId = routeData.FromCallId.Value,
                BridgeId = bridge.Id,
                Role = ChannelRoleType.Conference,
                LineId = routeData.LineId
            };
            await ChannelRepository.AddChannel(userChannelEntity);

            var destinationChannelEntity = new DAL.Entities.Channel
            {
                ChannelId = destinationChannelId,
                Extension = destinationExtension,
                CallId = routeData.ToCallId,
                BridgeId = bridge.Id,
                Role = ChannelRoleType.RingingFromUser,
                LineId = routeData.LineId
            };
            await ChannelRepository.AddChannel(destinationChannelEntity);
        }
    }
}
