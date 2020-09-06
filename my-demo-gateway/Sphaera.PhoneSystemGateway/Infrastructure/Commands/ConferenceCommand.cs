using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для добавления пользователей в режиме конференции
    /// </summary>
    public class ConferenceCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public ConferenceCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
        }

        /// <inheritdoc />
        protected override async Task InternalExecute(Channel channel, StasisStartEventArgs args)
        {
            var routeData = args.RouteData;

            if (!routeData.LineId.HasValue)
            {
                return;
            }

            await AriClient.Answer(channel.Id);

            await InitializeRecordingChannel(channel.Id, routeData.ToExtension, ChannelRoleType.Conference, args.BridgeId, routeData.ToCallId, routeData.LineId);

            var bridge = await AriClient.GetBridge(args.BridgeId);
            await AriClient.AddChannelToBridge(bridge.Id, channel.Id);
            await SnoopChannelByAllAssistantsChannels(channel.Id, routeData.ToExtension, ChannelRoleType.Conference, routeData.ToCallId, args);

            Logger.Information($"Channel added to call in conference mode. ChannelId: {channel.Id}. CallId: {routeData.ToCallId}");

            var channelForIncomingCall = await ChannelRepository.GetChannelForIncomingCall(routeData.LineId.Value);
            if (channelForIncomingCall != null && channelForIncomingCall.Interrupted)
            {
                channelForIncomingCall.Interrupted = false;
                await ChannelRepository.UpdateChannel(channelForIncomingCall);
            }

            var channelEntity = new DAL.Entities.Channel
            {
                ChannelId = channel.Id,
                BridgeId = args.BridgeId,
                CallId = routeData.ToCallId,
                Extension = routeData.ToExtension,
                Role = ChannelRoleType.Conference,
                LineId = routeData.LineId
            };
            await ChannelRepository.AddChannel(channelEntity);
        }
    }
}
