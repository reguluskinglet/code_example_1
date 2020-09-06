using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для начала записи канала
    /// </summary>
    public class RecordingStartedCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public RecordingStartedCommand(
            ILogger logger,
            IChannelRepository channelRepository,
            AsteriskAriClient ariClient,
            IAudioRecordRepository audioRecordRepository,
            IOptions<AsteriskOptions> options)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        protected override async Task InternalExecute(Channel channel, StasisStartEventArgs args)
        {
            if (!(args is RecordingEventArgs eventArgs))
            {
                throw new ArgumentException($"RecordingStartedCommand. Incorrect argument type {nameof(args)}");
            }

            try
            {
                var audioRecord = await AudioRecordRepository.GetRecordByName(eventArgs.RecordName);
                if (audioRecord != null)
                {
                    audioRecord.RecordingStartTime = eventArgs.EventTime;
                    await AudioRecordRepository.UpdateRecord(audioRecord);
                }
                else
                {
                    Logger.Warning($"RecordingStartedCommand. AudioRecord not found. RecordName: {eventArgs.RecordName}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("RecordingStartedCommand.Error.", ex);
            }
        }
    }
}
