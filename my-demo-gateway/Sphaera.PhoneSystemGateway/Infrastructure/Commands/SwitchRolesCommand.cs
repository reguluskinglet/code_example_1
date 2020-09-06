using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Команда для смены ролей между главным в разговоре и ассистентом/частичным ассистентом
    /// </summary>
    public class SwitchRolesCommand : BaseAsteriskCommand
    {
        /// <inheritdoc />
        public SwitchRolesCommand(ILogger logger,
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
            var lineId = args.RouteData?.LineId;
            if (!lineId.HasValue)
            {
                Logger.Warning("SwitchRolesCommand. Не найден LineId.");
                throw new Exception("Не найден LineId");
            }

            var oldMainChannel = await ChannelRepository.GetChannelForMainUser(lineId.Value);
            if (oldMainChannel == null)
            {
                Logger.Warning("SwitchRolesCommand. Не найден канал главного в разговоре.");
                throw new Exception("Не найден канал главного в разговоре");
            }

            if (oldMainChannel.CallId != args.RouteData.FromCallId)
            {
                Logger.Warning($"SwitchRolesCommand. Пользователь звонка {args.RouteData.FromCallId} не является главным в разговоре. MainUserCallId: {oldMainChannel.CallId}");
                throw new Exception($"Пользователь звонка с Id {args.RouteData.FromCallId} не является главным в разговоре");
            }

            var assistantChannel = await ChannelRepository.GetChannelByCallId(args.RouteData.ToCallId);
            if (assistantChannel == null)
            {
                Logger.Warning($"SwitchRolesCommand. Канал с Id {args.RouteData.ToCallId} не найден.");
                throw new Exception($"Канал с Id {args.RouteData.ToCallId} не найден.");
            }

            await ChangeMainChannel(oldMainChannel, assistantChannel, lineId.Value, args);
        }

        private async Task ChangeMainChannel(DAL.Entities.Channel oldMainChannel, DAL.Entities.Channel assistantChannel, Guid lineId, StasisStartEventArgs args)
        {
            var channelsInLine = await ChannelRepository.GetChannelsByLineId(lineId);

            var assistantRole = assistantChannel.Role;
            var assistantBridgeId = assistantChannel.BridgeId;
            var mainBridgeId = oldMainChannel.BridgeId;
            var newMainChannelId = assistantChannel.ChannelId;
            var newMainChannelExtension = assistantChannel.Extension;

            await AriClient.AddChannelToBridge(mainBridgeId, newMainChannelId);
            await AriClient.AddChannelToBridge(assistantBridgeId, oldMainChannel.ChannelId);
            var assistantSpeakChannel = channelsInLine.SingleOrDefault(t => t.OriginalChannelId == assistantChannel.ChannelId && t.Role == ChannelRoleType.SpeakSnoopChannel);
            if (assistantSpeakChannel != null)
            {
                await InitializeSnoopChannel(oldMainChannel.ChannelId, oldMainChannel.Extension, assistantRole, oldMainChannel.CallId,
                    assistantSpeakChannel.BridgeId, args, SnoopBridgeType.Speak, true);
            }

            await SnoopChannelByAllAssistantsChannels(newMainChannelId, newMainChannelExtension, ChannelRoleType.MainUser, assistantChannel.CallId, args);

            if (assistantRole == ChannelRoleType.PartialAssistant)
            {
                await SnoopChannelByAllAssistantsChannels(oldMainChannel.ChannelId, oldMainChannel.Extension, assistantRole, oldMainChannel.CallId, args, assistantChannel.ChannelId);
            }

            oldMainChannel.Role = assistantRole;
            oldMainChannel.BridgeId = assistantBridgeId;
            assistantChannel.Role = ChannelRoleType.MainUser;
            assistantChannel.BridgeId = mainBridgeId;
            await ChannelRepository.UpdateChannel(oldMainChannel);
            await ChannelRepository.UpdateChannel(assistantChannel);

            await HangUpOldSnoopChannels(oldMainChannel.ChannelId, channelsInLine);
            await HangUpOldSnoopChannels(newMainChannelId, channelsInLine);
        }

        private async Task HangUpOldSnoopChannels(string originalChannelId, IList<DAL.Entities.Channel> channelsInLine)
        {
            var snoopChannels = channelsInLine
                .Where(t => (t.Role == ChannelRoleType.SnoopChannel || t.Role == ChannelRoleType.SpeakSnoopChannel) && t.OriginalChannelId == originalChannelId)
                .ToList();

            foreach (var snoopChannel in snoopChannels)
            {
                await AriClient.HangupChannel(snoopChannel.ChannelId);
            }
        }
    }
}