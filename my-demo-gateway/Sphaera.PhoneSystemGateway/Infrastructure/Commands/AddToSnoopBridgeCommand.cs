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
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для добавления snoop-копии канала пользователя в указанный бридж
    /// </summary>
    public class AddToSnoopBridgeCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public AddToSnoopBridgeCommand(ILogger logger,
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
            Logger.Information($"AddToSnoopBridge. Bridge: {args.BridgeId}. Channel: {args.ChannelId}");

            await AriClient.AddChannelToBridge(args.BridgeId, args.ChannelId);

            var channelEntity = new DAL.Entities.Channel
            {
                ChannelId = args.ChannelId,
                BridgeId = args.BridgeId,
                Role = args.EventType == StasisStartEventType.AddToSpeakSnoopBridge ? ChannelRoleType.SpeakSnoopChannel : ChannelRoleType.SnoopChannel,
                LineId = args.RouteData.LineId,
                CallId = args.RouteData.ToCallId,
                OriginalChannelId = args.OriginalChannelId,
                Extension = args.RouteData.ToExtension
            };

            await ChannelRepository.AddChannel(channelEntity);
        }
    }
}
