using System;
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
    /// Команда для изоляции канала
    /// </summary>
    public class IsolationCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public IsolationCommand(ILogger logger,
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
            if (!(args is IsolationStasisStartEventArgs model))
            {
                return;
            }
            
            var isolationData = model.IsolationStatusData;
            
            try
            {
                var isolationChannel = await ChannelRepository.GetChannelByCallId(isolationData.CallId);

                if (isolationData.Isolated)
                {
                    await AriClient.MuteChannel(isolationChannel.ChannelId);
                }
                else
                {
                    await AriClient.UnmuteChannel(isolationChannel.ChannelId);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
                throw new Exception($"Не удалось установить параметр изоляции со значением {isolationData.Isolated}");
            }
        }
    }
}
