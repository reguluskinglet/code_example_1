using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.MessageContracts.MediaRecording;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using demo.Transit.Publisher;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands
{
    /// <summary>
    /// Команда для начала записи канала
    /// </summary>
    public class RecordingEndedCommand : BaseAsteriskCommand
    {
        private readonly IQueueSender _queueSender;

        /// <inheritdoc />
        public RecordingEndedCommand(
            ILogger logger,
            IChannelRepository channelRepository,
            AsteriskAriClient ariClient,
            IAudioRecordRepository audioRecordRepository,
            IOptions<AsteriskOptions> options,
            IQueueSender queueSender)
            : base(logger, channelRepository, audioRecordRepository, ariClient, options)
        {
            _queueSender = queueSender;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        protected override async Task InternalExecute(Channel channel, StasisStartEventArgs args)
        {
            if (!(args is RecordingEventArgs eventArgs))
            {
                throw new ArgumentException($"RecordingEndedCommand. Incorrect argument type {nameof(args)}");
            }

            try
            {
                var audioRecord = await AudioRecordRepository.GetRecordByName(eventArgs.RecordName);
                if (audioRecord == null || audioRecord.RecordingStartTime.HasValue == false)
                {
                    Logger.Warning($"Record with name {eventArgs.RecordName} not found");
                    return;
                }

                Logger.Information($"Audio record created. RecordName: {eventArgs.RecordName};");
                await _queueSender.Publish(new AudioRecordedIntegrationEvent
                {
                    LineId = audioRecord.LineId,
                    CallId = audioRecord.CallId,
                    FileName = $"{eventArgs.RecordName}.{AsteriskAriClient.RecordingFormat}",
                    RecordingStartTime = audioRecord.RecordingStartTime.Value,
                    RecordingEndTime = eventArgs.EventTime
                });

                audioRecord.RecordingEndTime = eventArgs.EventTime;
                await AudioRecordRepository.UpdateRecord(audioRecord);
            }
            catch (Exception ex)
            {
                Logger.Warning("RecordingEndedCommand.Error.", ex);
            }
        }
    }
}
