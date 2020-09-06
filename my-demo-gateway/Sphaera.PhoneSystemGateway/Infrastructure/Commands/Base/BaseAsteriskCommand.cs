using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using demo.DemoGateway.Serialization;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.Commands.Base
{
    /// <summary>
    /// Базовая команда для исполнения сценариев телефонии
    /// </summary>
    public abstract class BaseAsteriskCommand
    {
        private const string SnoopChannelPrefix = "SNOOP";

        private readonly AsteriskOptions _asteriskOptions;

        /// <summary>
        /// Репозиторий для хранения информации о каналах
        /// </summary>
        protected IChannelRepository ChannelRepository { get; }

        /// <summary>
        /// Репозиторий для хранения информации о каналах
        /// </summary>
        protected IAudioRecordRepository AudioRecordRepository { get; }

        /// <summary>
        /// Клиент для взаимодействия с Asterisk ARI
        /// </summary>
        protected AsteriskAriClient AriClient { get; }

        /// <summary>
        /// Логгер
        /// </summary>
        protected ILogger Logger { get; }

        /// <inheritdoc />
        protected BaseAsteriskCommand(ILogger logger,
            IChannelRepository channelRepository,
            IAudioRecordRepository audioRecordRepository,
            AsteriskAriClient ariClient,
            IOptions<AsteriskOptions> asteriskOptions)
        {
            Logger = logger;
            ChannelRepository = channelRepository;
            AudioRecordRepository = audioRecordRepository;
            AriClient = ariClient;
            _asteriskOptions = asteriskOptions.Value;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        public async Task Execute(Channel channel, StasisStartEventArgs args)
        {
            await InternalExecute(channel, args);
        }

        /// <summary>
        /// Реализация команды
        /// </summary>
        protected abstract Task InternalExecute(Channel channel, StasisStartEventArgs args);

        /// <summary>
        /// Создать новую пару бриджей (LISTEN/SPEAK) для ассистента/частичного ассистента и добавить туда каналы для прослушивания/разговора
        /// </summary>
        protected async Task<DAL.Entities.Channel> CreateNewAssistantBridgesAndSnoop(Channel assistantChannel,
            StasisStartEventArgs args, ChannelRoleType assistantRole)
        {
            var lineId = args.RouteData?.LineId;
            if (!lineId.HasValue)
            {
                Logger.Warning($"CallTo{assistantRole}. LineId не найден AssistantChannelId: {assistantChannel.Id}.");
                return null;
            }

            var mainChannel = await ChannelRepository.GetChannelForMainUser(lineId.Value);
            if (mainChannel == null)
            {
                Logger.Warning($"CallTo{assistantRole}. MainChannel не найден.");
                return null;
            }

            var assistantExtension = args.RouteData.ToExtension;
            var assistantCallId = args.RouteData.ToCallId;
            var prefix = assistantRole == ChannelRoleType.PartialAssistant ? "PASSISTANT" : "ASSISTANT";
            var assistantBridgeId = $"{prefix}_{assistantExtension}_{assistantChannel.Id}";
            var speakBridgeId = $"{assistantBridgeId}_SPEAK";
            var listenBridgeId = $"{assistantBridgeId}_LISTEN";

            await InitializeRecordingChannel(assistantChannel.Id, args.RouteData.ToExtension, assistantRole, mainChannel.BridgeId, assistantCallId, lineId.Value);

            await AriClient.CreateBridge(speakBridgeId);
            await AriClient.CreateBridge(listenBridgeId);
            await AriClient.AddChannelToBridge(listenBridgeId, assistantChannel.Id);
            await InitializeSnoopChannel(assistantChannel.Id, assistantExtension, assistantRole, assistantCallId, speakBridgeId, args, SnoopBridgeType.Speak, true);

            var channelsInLine = await ChannelRepository.GetChannelsByLineId(lineId.Value);
            var channelsInMainBridge = channelsInLine.Where(t => t.BridgeId == mainChannel.BridgeId).ToList();
            var partialAssistantChannels = channelsInLine.Where(t => t.Role == ChannelRoleType.PartialAssistant).ToList();

            foreach (var channel in channelsInMainBridge)
            {
                await InitializeSnoopChannel(channel.ChannelId, channel.Extension, channel.Role, channel.CallId, listenBridgeId, args, SnoopBridgeType.Listen);
                if (NeedAddChannelToSpeakBridge(channel.Role, assistantRole))
                {
                    await InitializeSnoopChannel(channel.ChannelId, channel.Extension, channel.Role, channel.CallId, speakBridgeId, args, SnoopBridgeType.Speak);
                }
            }

            foreach (var channel in partialAssistantChannels)
            {
                await InitializeSnoopChannel(channel.ChannelId, channel.Extension, channel.Role, channel.CallId, listenBridgeId, args, SnoopBridgeType.Listen);
                if (assistantRole == ChannelRoleType.PartialAssistant)
                {
                    await InitializeSnoopChannel(channel.ChannelId, channel.Extension, channel.Role, channel.CallId, speakBridgeId, args, SnoopBridgeType.Speak);
                }
            }

            if (assistantRole == ChannelRoleType.PartialAssistant)
            {
                await SnoopChannelByAllAssistantsChannels(assistantChannel.Id, assistantExtension, assistantRole, assistantCallId, args);
            }

            var assistantChannelEntity = new DAL.Entities.Channel
            {
                ChannelId = assistantChannel.Id,
                Extension = assistantExtension,
                CallId = assistantCallId,
                BridgeId = listenBridgeId,
                Role = assistantRole,
                LineId = lineId.Value
            };
            await ChannelRepository.AddChannel(assistantChannelEntity);

            return assistantChannelEntity;
        }

        /// <summary>
        /// Добавить snoop-копию канала во все бриджи ассистентов и в бридж для частичных ассистентов
        /// </summary>
        protected async Task SnoopChannelByAllAssistantsChannels(
            string channelId,
            string channelExtension,
            ChannelRoleType channelRole,
            Guid callId,
            StasisStartEventArgs args,
            string excludedChannelId = null)
        {
            if (args.RouteData.LineId.HasValue)
            {
                var channelsInLine = (await ChannelRepository.GetChannelsByLineId(args.RouteData.LineId.Value))
                    .Where(t => excludedChannelId == null || t.ChannelId != excludedChannelId)
                    .ToList();

                await SnoopChannelByAssistantChannels(channelId, channelExtension, channelRole, callId, channelsInLine, args);

                var partialAssistantChannels = channelsInLine.Where(t => t.Role == ChannelRoleType.PartialAssistant);
                foreach (var channel in partialAssistantChannels)
                {
                    await InitializeSnoopChannel(channelId, channelExtension, channelRole, callId, channel.BridgeId, args, SnoopBridgeType.Listen);
                    var speakChannel = channelsInLine.SingleOrDefault(t => t.OriginalChannelId == channel.ChannelId && t.Role == ChannelRoleType.SpeakSnoopChannel);
                    if (speakChannel != null)
                    {
                        await InitializeSnoopChannel(channelId, channelExtension, channelRole, callId, speakChannel.BridgeId, args, SnoopBridgeType.Speak);
                    }
                }
            }
        }

        /// <summary>
        /// Добавить snoop-копию канала во все бриджи ассистентов
        /// </summary>
        protected async Task SnoopChannelByAssistantChannels(
            string agentChannelId,
            string agentExtension,
            ChannelRoleType agentRole,
            Guid agentCallId,
            IList<DAL.Entities.Channel> channelsInLine,
            StasisStartEventArgs args)
        {
            var assistanceChannels = channelsInLine.Where(t => t.Role == ChannelRoleType.Assistant);
            foreach (var channel in assistanceChannels)
            {
                await InitializeSnoopChannel(agentChannelId, agentExtension, agentRole, agentCallId, channel.BridgeId, args, SnoopBridgeType.Listen);
                if (NeedAddChannelToSpeakBridge(agentRole, ChannelRoleType.Assistant))
                {
                    var speakChannel = channelsInLine.SingleOrDefault(t => t.OriginalChannelId == channel.ChannelId && t.Role == ChannelRoleType.SpeakSnoopChannel);
                    if (speakChannel != null)
                    {
                        await InitializeSnoopChannel(agentChannelId, agentExtension, agentRole, agentCallId, speakChannel.BridgeId, args, SnoopBridgeType.Speak);
                    }
                }
            }
        }

        /// <summary>
        /// Создать snoop-копию канала
        /// </summary>
        protected async Task InitializeSnoopChannel(
            string agentChannelId,
            string agentExtension,
            ChannelRoleType agentRole,
            Guid agentCallId,
            string destinationBridgeId,
            StasisStartEventArgs startEventArgs,
            SnoopBridgeType bridgeType,
            bool channelForSpeak = false)
        {
            var whisper = GetWhisperForSnoop(bridgeType, channelForSpeak);
            var spy = GetSpyForSnoop(bridgeType, channelForSpeak);
            var snoopChannelId = GetSnoopChannelId(agentRole, agentExtension) + $"_spy-{spy}_whisper-{whisper}";

            startEventArgs.RouteData.ToExtension = agentExtension;
            startEventArgs.RouteData.ToCallId = agentCallId;

            var snoopArgs = new StasisStartEventArgs
            {
                EventType = channelForSpeak ? StasisStartEventType.AddToSpeakSnoopBridge : StasisStartEventType.AddToSnoopBridge,
                ChannelId = snoopChannelId,
                BridgeId = destinationBridgeId,
                RouteData = startEventArgs.RouteData,
                OriginalChannelId = agentChannelId
            };

            var encodedArgs = JsonSerializer.EncodeData(snoopArgs);
            await AriClient.SnoopChannel(agentChannelId, spy, whisper, encodedArgs, snoopChannelId);
        }

        /// <summary>
        /// Инициализировать запись канала
        /// </summary>
        protected async Task InitializeRecordingChannel(
            string channelId,
            string extension,
            ChannelRoleType role,
            string mainBridgeId,
            Guid callId,
            Guid? lineId)
        {
            await AddChannelToCommonRecordingBridge(channelId, extension, role, mainBridgeId, callId, lineId);
            await CreateSnoopChannelForRecording(channelId, extension, role, false, callId, lineId);
        }

        /// <summary>
        /// Начать запись разговора
        /// </summary>
        protected async Task StartCallRecording(string initialChannelId, Guid initialCallId, string extension, ChannelRoleType role, string mainBridgeId, Guid? lineId = null)
        {
            if (!_asteriskOptions.RecordingEnabled)
            {
                return;
            }

            var recordsBridgeId = GetCommonRecordingBridgeId(mainBridgeId);
            var fullAudioRecord = new AudioRecord
            {
                FileName = $"{recordsBridgeId}",
                LineId = lineId,
                CallId = null
            };
            await AudioRecordRepository.AddAudioRecord(fullAudioRecord);

            await AriClient.CreateBridge(recordsBridgeId);

            await AddChannelToCommonRecordingBridge(initialChannelId, extension, role, mainBridgeId, initialCallId, lineId);
            
            var result = await AriClient.StartRecordingBridge(recordsBridgeId);
            if (result.IsFailure)
            {
                Logger.Warning($"StartCallRecording. Failed to start call recording. MainBridgeId: {mainBridgeId}");
                return;
            }

            await CreateSnoopChannelForRecording(initialChannelId, extension, role, false, initialCallId, lineId);

            Logger.Information($"StartRecordingBridge. Bridge recording started. {mainBridgeId}");
        }

        /// <summary>
        /// Начать запись канала
        /// </summary>
        protected async Task<Result> StartRecordingChannel(string snoopChannelId, Guid callId, Guid? lineId)
        {
            if (!_asteriskOptions.RecordingEnabled)
            {
                return Result.Failure(ErrorCodes.RecordingError);
            }

            var audioRecord = new AudioRecord
            {
                CallId = callId,
                LineId = lineId,
                FileName = snoopChannelId
            };
            await AudioRecordRepository.AddAudioRecord(audioRecord);

            var result = await AriClient.StartRecordingChannel(snoopChannelId);
            if (result.IsFailure)
            {
                Logger.Warning($"StartRecordingChannel. Could not start recording channel. CallId: {callId}");
                return result;
            }

            Logger.Information($"StartRecordingChannel. Channel recording started. {snoopChannelId}");

            return Result.Success();
        }

        /// <summary>
        /// Получить Id бриджа для записи всех участников разговора
        /// </summary>
        protected static string GetCommonRecordingBridgeId(string bridgeId)
        {
            var recordChannelId = $"RECORDS_ALL_{bridgeId}";
            return recordChannelId;
        }

        /// <summary>
        /// Создать snoop-копию канала для записи
        /// </summary>
        /// <param name="channelId">Идентификатор исходного канала</param>
        /// <param name="extension">Номер</param>
        /// <param name="role">Роль канала</param>
        /// <param name="forCommonRecord">Признак того, что копия канала создана для записи разговора всех участников в один файл</param>
        /// <param name="callId">Идентификатор звонка</param>
        /// <param name="lineId">Идентификатор линии</param>
        /// <returns>SnoopChannelId</returns>
        private async Task<Result<string>> CreateSnoopChannelForRecording(
            string channelId,
            string extension,
            ChannelRoleType role,
            bool forCommonRecord,
            Guid callId,
            Guid? lineId)
        {
            if (!_asteriskOptions.RecordingEnabled)
            {
                return Result.Failure(ErrorCodes.RecordingError);
            }

            var routeData = new RouteCallDto
            {
                ToCallId = callId,
                LineId = lineId
            };

            var snoopChannelId = GetSnoopChannelIdForRecord(role, extension);
            var snoopArgs = new StasisStartEventArgs
            {
                EventType = StasisStartEventType.IgnoreStasisEvent,
                ChannelId = snoopChannelId,
                OriginalChannelId = channelId,
                RouteData = routeData
            };

            var encodedArgs = JsonSerializer.EncodeData(snoopArgs);
            await AriClient.SnoopChannel(channelId, "in", "none", encodedArgs, snoopChannelId);

            if (forCommonRecord)
            {
                return Result.Success(snoopChannelId);
            }
            
            var recordingResult = await StartRecordingChannel(snoopChannelId, callId, lineId);
            if (recordingResult.IsFailure)
            {
                Logger.Information($"CreateSnoopChannelForRecording. StartRecordingChannelError: {recordingResult.ErrorMessage}");
                return Result.Failure(ErrorCodes.RecordingError);
            }

            return Result.Success(snoopChannelId);
        }

        /// <summary>
        /// Добавить канал в бридж для записи всех участников
        /// </summary>
        private async Task AddChannelToCommonRecordingBridge(string channelId, string extension, ChannelRoleType role, string mainBridgeId, Guid callId, Guid? lineId)
        {
            var recordingChannelIdResult = await CreateSnoopChannelForRecording(channelId, extension, role, true, callId, lineId);
            if (recordingChannelIdResult.IsFailure)
            {
                Logger.Information($"Failed to create snoop channel for recording. RecordingEnabled: {_asteriskOptions.RecordingEnabled}");
                return;
            }

            var recordingChannelId = recordingChannelIdResult.Value;
            var recordingBridgeId = GetCommonRecordingBridgeId(mainBridgeId);
            await AriClient.AddChannelToBridge(recordingBridgeId, recordingChannelId);
        }

        /// <summary>
        /// Нужно ли добавлять snoop-копию канала в бридж, в котором инициатор создания бриджа может говорить
        /// </summary>
        private static bool NeedAddChannelToSpeakBridge(ChannelRoleType channelRole, ChannelRoleType assistantRole)
        {
            return channelRole == ChannelRoleType.MainUser && assistantRole == ChannelRoleType.Assistant ||
                   channelRole != ChannelRoleType.ExternalChannel && assistantRole == ChannelRoleType.PartialAssistant;
        }

        /// <summary>
        /// Получение атрибута, отвечающего за возможность snoop-канала слышать других участников в бридже
        /// </summary>
        /// <param name="bridgeType">Тип snoop-бриджа</param>
        /// <param name="channelForSpeak">Канал может только говорить в бридж</param>
        /// <returns>out - канал имеет возможность слышать, none - канал "глухой"</returns>
        private static string GetWhisperForSnoop(SnoopBridgeType bridgeType, bool channelForSpeak)
        {
            if (bridgeType == SnoopBridgeType.Listen || channelForSpeak)
            {
                return "none";
            }

            return "out";
        }

        /// <summary>
        /// Получение атрибута, отвечающего за возможность канала говорить в бридже
        /// </summary>
        /// <param name="bridgeType">Тип snoop-бриджа</param>
        /// <param name="channelForSpeak">Канал может только говорить в бридж</param>
        /// <returns>in - канал будут слышать другие участники бриджа, none - канал "немой"</returns>
        private static string GetSpyForSnoop(SnoopBridgeType bridgeType, bool channelForSpeak)
        {
            if (bridgeType == SnoopBridgeType.Listen || channelForSpeak)
            {
                return "in";
            }

            return "none";
        }

        /// <summary>
        /// Возвращает идентификатор snoop-канала по идентификатору канала, который необходимо слушать, и по владельцу канала
        /// </summary>
        /// <remarks>
        /// Если делать Id snoop-канала как по документации, то иногда происходит ошибка в Asterisk ARI из за длинной строки.
        /// Рекомендуемое наименование для snoop-канала: $"{SnoopChannelPrefix}_{channelIdForSnoop}_by_{snooper.Extension}_{snooper.ChannelId}";
        /// Пример Id, при котором происходит ошибка: SNOOP_vvm-01-sip-asterisk-01.stage.demo.ru-1573805482.581_by_107_vvm-01-sip-asterisk-01.stage.demo.ru-1573805476.578
        /// </remarks>
        private static string GetSnoopChannelId(ChannelRoleType agentRole, string agentExtension)
        {
            var snoopChannelId = $"{SnoopChannelPrefix}_{DateTime.Now.Ticks}_{agentRole}_{agentExtension}";
            return snoopChannelId;
        }

        private static string GetSnoopChannelIdForRecord(ChannelRoleType role, string extension)
        {
            var recordChannelId = $"{DateTime.Now.Ticks}_{role}_{extension}_RECORD";
            return recordChannelId;
        }
    }
}