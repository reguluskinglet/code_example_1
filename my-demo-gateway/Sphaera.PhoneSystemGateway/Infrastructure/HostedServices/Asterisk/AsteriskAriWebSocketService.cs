using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsterNET.ARI;
using AsterNET.ARI.Models;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.Commands.Factory;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Serialization;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.HostedServices.Asterisk
{
    /// <summary>
    /// Сервис для работы с Asterisk ARI
    /// </summary>
    public class AsteriskAriWebSocketService
    {
        private readonly ILogger _logger;
        private AsteriskAriClient _ariClient;
        private readonly CommandFactory _commandFactory;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AsteriskAriWebSocketService(ILogger logger, CommandFactory commandFactory)
        {
            _logger = logger;
            _commandFactory = commandFactory;
            _ariClient = commandFactory.GetAsteriskAriClient();
        }

        /// <summary>
        /// Конструктор для UnitTests
        /// Позволяет передать AriClient
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="commandFactory"></param>
        /// <param name="ariClient"></param>
        public AsteriskAriWebSocketService(
            ILogger logger,
            CommandFactory commandFactory,
            IAriClient ariClient
        ) : this(logger, commandFactory)
        {
            _ariClient = new AsteriskAriClient(logger, ariClient);
        }

        /// <summary>
        /// Подписаться на события AsteriskAri
        /// </summary>
        public void Start()
        {
            _logger.Information("AsteriskAriWebSocketService Started.", GetOptionsForLogging());
            _ariClient = _commandFactory.GetAsteriskAriClient();
            _ariClient.StartEvent += AriClientOnStasisStart;
            _ariClient.EndEvent += AriClientOnEndEvent;
            _ariClient.ChannelDestroyed += AriClientOnChannelDestroyed;
            _ariClient.RecordingStarted += AriClient_RecordingStarted;
            _ariClient.RecordingFinished += AriClient_RecordingFinished;
        }

        /// <summary>
        /// Создать новый канал
        /// </summary>
        public async Task<bool> OriginateChannel(string args, string extensionFrom, string extensionTo)
        {
            return await _ariClient.Originate(args, extensionFrom, extensionTo);
        }

        /// <summary>
        /// Установить статус изоляции.
        /// </summary>
        public async Task<Result> SetIsolationStatus(IsolationStatusDto model)
        {
            try
            {
                var args = new IsolationStasisStartEventArgs
                {
                    IsolationStatusData = model
                };

                await _commandFactory.GetCommand(StasisStartEventType.IsolationCommand).Execute(null, args);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.Warning(e.Message);
                return Result.Failure(ErrorCodes.UnableToSetIsolationStatus);
            }
        }

        /// <summary>
        /// Установить статус слышимости микрофона.
        /// </summary>
        public async Task<Result> SetMuteStatus(MuteStatusDto dto)
        {
            try
            {
                var args = new MuteStasisEventArgs()
                {
                    MuteStatusData = dto
                };

                await _commandFactory.GetCommand(StasisStartEventType.MuteCommand).Execute(null, args);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.Warning(e.Message);
                return Result.Failure(ErrorCodes.UnableToSetMuteStatus);
            }
        }

        /// <summary>
        /// Поменяться ролями между главным в разговоре и ассистентом
        /// </summary>
        public async Task<Result> SwitchMainUser(RouteCallDto routeDto)
        {
            try
            {
                var args = new StasisStartEventArgs
                {
                    RouteData = routeDto
                };

                await _commandFactory.GetCommand(StasisStartEventType.SwitchRolesCommand).Execute(null, args);
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.Warning(e.Message);
                return Result.Failure(ErrorCodes.UnableToSwitchMainUser);
            }
        }

        /// <summary>
        /// Принудительное удаление канала из звонка
        /// </summary>>
        public async Task ForceHangUp(RouteCallDto model)
        {
            var args = new StasisStartEventArgs
            {
                RouteData = model
            };

            await _commandFactory.GetCommand(StasisStartEventType.ForceHangUpCommand).Execute(null, args);
        }

        private async void AriClientOnStasisStart(object sender, StasisStartEvent e)
        {
            _logger.Information($"AsteriskAriWebSocketService.OnStasisStartEvent. {e.Channel.Id}");
            await ProcessStasisEvents(e.Args, e.Channel);
        }

        private async void AriClientOnEndEvent(object sender, StasisEndEvent e)
        {
            await _commandFactory.GetCommand(StasisStartEventType.DeleteChannelCommand).Execute(e.Channel, null);
        }

        private async void AriClientOnChannelDestroyed(object sender, ChannelDestroyedEvent e)
        {
            await _commandFactory.GetCommand(StasisStartEventType.RejectedCallFromUser).Execute(e.Channel, null);
        }

        private async void AriClient_RecordingStarted(object sender, RecordingStartedEvent e)
        {
            var args = new RecordingEventArgs(e.Timestamp, e.Recording.Name);
            await _commandFactory.GetCommand(StasisStartEventType.RecordingStarted).Execute(null, args);
        }

        private async void AriClient_RecordingFinished(object sender, RecordingFinishedEvent e)
        {
            var args = new RecordingEventArgs(e.Timestamp, e.Recording.Name);
            await _commandFactory.GetCommand(StasisStartEventType.RecordingEnded).Execute(null, args);
        }

        private async Task ProcessStasisEvents(IList<string> args, Channel channel)
        {
            _logger.Information($"ProcessStasisEvents.Start. ChannelId: {channel.Id}");
            if (!args.Any())
            {
                _logger.Information($"ProcessStasisEvents.RegisterIncomingCallCommand. ChannelId: {channel.Id}");
                await _commandFactory.GetCommand(StasisStartEventType.RegisterIncomingCallCommand).Execute(channel, null);
                return;
            }

            StasisStartEventArgs startEventArgs;
            try
            {
                startEventArgs = JsonSerializer.DecodeData<StasisStartEventArgs>(args[0]);
            }
            catch (Exception ex)
            {
                _logger.Error("AriClientOnStasisStartEvent. Args parsing error.", ex);
                return;
            }

            _logger.Information($"ProcessStasisEvents.GetCommand. EventType: {startEventArgs.EventType}. ChannelId: {channel.Id}");

            if (startEventArgs.EventType == StasisStartEventType.IgnoreStasisEvent)
            {
                return;
            }

            await _commandFactory.GetCommand(startEventArgs.EventType).Execute(channel, startEventArgs);
        }

        private Dictionary<string, string> GetOptionsForLogging()
        {
            var options = _commandFactory.GetAsteriskAriOptions();
            return new Dictionary<string, string>
            {
                {"asteriskHost", options.Ip},
                {"asteriskPort", options.Port.ToString()}
            };
        }

        /// <summary>
        /// Получить список всех бриджей
        /// </summary>
        public async Task<IList<BridgeDto>> GetAllBridges()
        {
            return await _ariClient.GetAllBridges();
        }

        /// <summary>
        /// Удалить все бриджи
        /// </summary>
        public async Task DestroyAllBridges()
        {
            await _ariClient.DestroyAllBridges();
        }
    }
}