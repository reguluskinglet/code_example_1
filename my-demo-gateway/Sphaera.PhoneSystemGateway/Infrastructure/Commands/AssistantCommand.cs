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
    /// Команда для добавления пользователей в режиме ассистирования
    /// </summary>
    public class AssistantCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public AssistantCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
        }

        /// <inheritdoc />
        protected override async Task InternalExecute(Channel assistantChannel, StasisStartEventArgs args)
        {
            await CreateNewAssistantBridgesAndSnoop(assistantChannel, args, ChannelRoleType.Assistant);
        }
    }
}
