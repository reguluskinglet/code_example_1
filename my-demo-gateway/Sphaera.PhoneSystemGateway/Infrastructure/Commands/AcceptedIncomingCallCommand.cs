using System;
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
    /// Команда для сохранения данных о каналах при ответе пользователя на входящий вызов
    /// </summary>
    public class AcceptedIncomingCallCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public AcceptedIncomingCallCommand(ILogger logger,
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
            Logger.Information($"AcceptIncomingCallCommand. UserChannelId: {userChannel.Id}, IncomingChannelId: {args.ChannelId}");

            try
            {
                var incomingCallChannel = await ChannelRepository.GetByChannelId(args.ChannelId);
                if (incomingCallChannel == null)
                {
                    Logger.Warning($"Канал входящего вызова не найден. UserChannelId: {userChannel.Id}");
                    return;
                }

                var routeData = args.RouteData;
                var lineId = routeData.LineId;

                await InitializeRecordingChannel(userChannel.Id, routeData.ToExtension, ChannelRoleType.Conference, incomingCallChannel.BridgeId, routeData.ToCallId, lineId);

                await AriClient.UnholdAsync(incomingCallChannel.ChannelId);
                await AriClient.StopMohInBridgeAsync(incomingCallChannel.BridgeId);
                await AriClient.AddChannelToBridge(incomingCallChannel.BridgeId, userChannel.Id);

                incomingCallChannel.Interrupted = false;
                incomingCallChannel.LineId = lineId;
                await ChannelRepository.UpdateChannel(incomingCallChannel);

                await UpdateAudioRecords(incomingCallChannel.BridgeId, incomingCallChannel.CallId, lineId);

                var userChannelEntity = new DAL.Entities.Channel
                {
                    ChannelId = userChannel.Id,
                    BridgeId = incomingCallChannel.BridgeId,
                    CallId = routeData.ToCallId,
                    Extension = routeData.ToExtension,
                    Role = ChannelRoleType.Conference,
                    LineId = lineId
                };
                await ChannelRepository.AddChannel(userChannelEntity);
            }
            catch (Exception ex)
            {
                Logger.Warning("AcceptIncomingCallCommand Error", ex);
            }
        }

        private async Task UpdateAudioRecords(string mainBridgeId, Guid initCallId, Guid? lineId)
        {
            var commonBridgeId = GetCommonRecordingBridgeId(mainBridgeId);

            var audioRecord = await AudioRecordRepository.GetRecordByName(commonBridgeId);
            if (audioRecord != null)
            {
                audioRecord.LineId = lineId;
                await AudioRecordRepository.UpdateRecord(audioRecord);
            }

            var initCallRecord = await AudioRecordRepository.GetRecordByCallId(initCallId);
            if (initCallRecord != null)
            {
                initCallRecord.LineId = lineId;
                await AudioRecordRepository.UpdateRecord(initCallRecord);
            }
        }
    }
}
