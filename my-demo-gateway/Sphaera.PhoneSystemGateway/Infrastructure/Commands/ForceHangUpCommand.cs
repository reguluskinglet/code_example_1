using System.Threading.Tasks;
using AsterNET.ARI.Models;
using Microsoft.Extensions.Options;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для завершения звонка канала и установки признака, что входящий вызов от заявителя или внешнего участника разговора был прерван
    /// </summary>
    public class ForceHangUpCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public ForceHangUpCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        protected override async Task InternalExecute(Channel channel, StasisStartEventArgs args)
        {
            var routeData = args.RouteData;

            if (routeData.FromCallId.HasValue)
            {
                var incomingCallChannel = await ChannelRepository.GetChannelByCallId(routeData.FromCallId.Value);
                if (incomingCallChannel != null)
                {
                    incomingCallChannel.Interrupted = true;
                    await ChannelRepository.UpdateChannel(incomingCallChannel);
                }
            }

            var channelForDelete = await ChannelRepository.GetChannelByCallId(routeData.ToCallId);
            if (channelForDelete != null)
            {
                await AriClient.HangupChannel(channelForDelete.ChannelId);
            }
        }
    }
}
