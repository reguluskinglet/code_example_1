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
    public class MuteCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public MuteCommand(ILogger logger,
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
            if (!(args is MuteStasisEventArgs model))
            {
                throw new ArgumentException($"Не правильный тип аргумента {nameof(args)}");
            }
            
            var muteStatusData = model.MuteStatusData;
            
            try
            {
                var muteChannel = await ChannelRepository.GetChannelByCallId(muteStatusData.CallId);

                if (muteStatusData.Muted)
                {
                    await AriClient.MuteChannel(muteChannel.ChannelId, "in");
                }
                else
                {
                    await AriClient.UnmuteChannel(muteChannel.ChannelId, "in");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message);
                throw new Exception($"Не удалось установить параметр мюте со значением {model.MuteStatusData.Muted}");
            }
        }
    }
}
