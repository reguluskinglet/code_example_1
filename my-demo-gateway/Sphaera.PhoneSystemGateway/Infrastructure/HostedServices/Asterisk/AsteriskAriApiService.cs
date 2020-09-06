using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Serialization;
using Channel = demo.DemoGateway.DAL.Entities.Channel;

namespace demo.DemoGateway.Infrastructure.HostedServices.Asterisk
{
    /// <summary>
    /// Для обращения к ARI по HTTP API
    /// </summary>
    public class AsteriskAriApiService
    {
        private readonly ILogger _logger;
        private readonly IChannelRepository _channelRepository;
        private readonly AsteriskAriWebSocketService _ariWebSocketService;

        /// <inheritdoc />
        public AsteriskAriApiService(ILogger logger, IChannelRepository channelRepository, AsteriskAriWebSocketService ariWebSocketService)
        {
            _logger = logger;
            _channelRepository = channelRepository;
            _ariWebSocketService = ariWebSocketService;
        }

        /// <summary>
        /// Создать канал для пользователя, который принимает входящий вызов
        /// </summary>
        public async Task<Result> AcceptIncomingCall(RouteCallDto routeDto)
        {
            if (!routeDto.FromCallId.HasValue)
            {
                _logger.Warning("AcceptIncomingCall. Missing id waiting incoming call");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            if (!routeDto.LineId.HasValue)
            {
                _logger.Warning("AcceptIncomingCall. LineId not found.");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            _logger.Information($"AcceptIncomingCall. Пользователь {routeDto.ToExtension} принимает вызов c Id {routeDto.FromCallId}. CallId: {routeDto.ToCallId}");

            var incomingCallChannel = await _channelRepository.GetChannelByCallId(routeDto.FromCallId.Value);
            if (incomingCallChannel == null)
            {
                _logger.Warning("AcceptIncomingCall. Incoming call channel not found.");
                return Result.Failure(ErrorCodes.ChannelNotFound);
            }

            var args = new StasisStartEventArgs
            {
                EventType = StasisStartEventType.AcceptIncomingCall,
                ChannelId = incomingCallChannel.ChannelId,
                RouteData = routeDto
            };

            var originateResult = await OriginateAsync(args, incomingCallChannel.Extension, routeDto.ToExtension);
            return originateResult;
        }

        /// <summary>
        /// Добавить участника в разговор в режиме конференции
        /// </summary>
        public async Task<Result> AddToConference(RouteCallDto routeDto)
        {
            _logger.Information($"Add {routeDto.ToExtension} in conference mode. CallId: {routeDto.ToCallId}, LineId: {routeDto.LineId}");

            if (!routeDto.LineId.HasValue)
            {
                _logger.Warning("AddToConference. LineId not found.");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            var lineId = routeDto.LineId.Value;
            var bridgeId = await _channelRepository.GetMainBridgeId(lineId);
            if (bridgeId == null)
            {
                _logger.Warning($"AddToConference. Main Bridge not found. LineId: {lineId}");
                return Result.Failure(ErrorCodes.BridgeNotFound);
            }

            Channel fromCallChannel = null;
            if (routeDto.FromCallId.HasValue)
            {
                fromCallChannel = await _channelRepository.GetChannelByCallId(routeDto.FromCallId.Value);
            }

            var args = new StasisStartEventArgs
            {
                EventType = StasisStartEventType.Conference,
                BridgeId = bridgeId,
                RouteData = routeDto
            };

            var originateResult = await OriginateAsync(args, fromCallChannel?.Extension, args.RouteData.ToExtension);

            return originateResult;
        }

        /// <summary>
        /// Сделать вызов пользователя в режиме ассистирования
        /// </summary>
        public async Task<Result> AddAssistant(RouteCallDto routeDto)
        {
            _logger.Information($"AddAssistant. Add assistant {routeDto.ToExtension}");
            return await AddAssistantSpecificType(routeDto, StasisStartEventType.Assistant);
        }

        /// <summary>
        /// Сделать вызов пользователя в режиме частичного ассистирования
        /// </summary>
        public async Task<Result> AddPartialAssistant(RouteCallDto routeDto)
        {
            _logger.Information($"AddPartialAssistant. Add partial assistant {routeDto.ToExtension}");
            return await AddAssistantSpecificType(routeDto, StasisStartEventType.PartialAssistant);
        }

        /// <summary>
        /// Установить статус изоляции.
        /// </summary>
        public async Task<Result> SetIsolationStatus(IsolationStatusDto model)
        {
            _logger.Information($"SetIsolationStatus. Call isolation: {model.CallId}; {model.Isolated}");
            return await _ariWebSocketService.SetIsolationStatus(model);
        }

        /// <summary>
        /// Установить статус слышимости микрофона
        /// </summary>
        public async Task<Result> SetMuteStatus(MuteStatusDto dto)
        {
            _logger.Information($"SetMuteStatus. Mute Microphone: {dto.CallId}; {dto.Muted}");
            return await _ariWebSocketService.SetMuteStatus(dto);
        }

        /// <summary>
        /// Поменяться ролями между двумя участниками линии вызова
        /// </summary>
        public async Task<Result> ExchangeRoles(RouteCallDto routeDto)
        {
            _logger.WithTag("line_id", routeDto.LineId)
                .WithTag("from_call_id", routeDto.FromCallId)
                .WithTag("to_extension", routeDto.ToExtension)
                .Information($"ExchangeRoles. MainCallId: {routeDto.FromCallId}. Assistant: {routeDto.ToExtension}.");

            return await _ariWebSocketService.SwitchMainUser(routeDto);
        }

        /// <summary>
        /// Инициализировать прямой вызов от пользователя на номер <see cref="RouteCallDto.ToExtension"/>
        /// </summary>
        /// <remarks>
        /// При вызове Originate создается только канал для пользователя, который звонит на номер назначения.
        /// Канал для участника, которому звонит пользователь, будет создан после создания канала оператора при обработке события StasisStartEvent
        /// </remarks>
        public async Task<Result> CallFromUser(RouteCallDto routeDto)
        {
            var logMessage = $"Call from {routeDto.FromExtension} to {routeDto.ToExtension}. CallId: {routeDto.ToCallId} LineId: {routeDto.LineId}";
            _logger.Information($"CallFromUser. {logMessage}");

            var args = new StasisStartEventArgs
            {
                EventType = StasisStartEventType.CallFromUser,
                RouteData = routeDto
            };

            var originateResult = await OriginateAsync(args, "Служба 112", args.RouteData.FromExtension);
            return originateResult;
        }

        /// <summary>
        /// Принудительное удаление канала из звонка
        /// </summary>>
        public async Task ForceHangUp(RouteCallDto model)
        {
            _logger.Information($"ForceHangUp. Принудительное удаление канала: {model.ToCallId}");
            await _ariWebSocketService.ForceHangUp(model);
        }

        /// <summary>
        /// Получить все бриджи
        /// </summary>
        public async Task<IList<BridgeDto>> GetAllBridges()
        {
            return await _ariWebSocketService.GetAllBridges();
        }

        /// <summary>
        /// Удалить все бриджи
        /// </summary>
        public async Task DeleteAllBridges()
        {
            await _ariWebSocketService.DestroyAllBridges();
        }

        private async Task<Result> AddAssistantSpecificType(RouteCallDto routeDto, StasisStartEventType eventType)
        {
            _logger.WithTag("to_call_id", routeDto.ToCallId)
                .WithTag("from_call_id", routeDto.FromCallId)
                .WithTag("assistant_extension", routeDto.ToExtension)
                .Information($"Add {eventType}. FromCallId: {routeDto.FromCallId}. Assistant: {routeDto.ToExtension}. CallId: {routeDto.ToCallId}");

            if (!routeDto.LineId.HasValue)
            {
                _logger.Warning($"Add{eventType}. LineId not found.");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            if (!routeDto.FromCallId.HasValue)
            {
                _logger.Warning($"Add{eventType}. FromCallId not found.");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            var setMainChannelResult = await SetMainUser(routeDto.LineId.Value, routeDto.FromCallId.Value);
            if (setMainChannelResult.IsFailure)
            {
                _logger.Warning($"Add{eventType}. {setMainChannelResult.ErrorMessage}");
                return Result.Failure(setMainChannelResult.ErrorCode);
            }

            var fromCallChannel = await _channelRepository.GetChannelByCallId(routeDto.FromCallId.Value);

            var args = new StasisStartEventArgs
            {
                EventType = eventType,
                BridgeId = setMainChannelResult.Value.BridgeId,
                RouteData = routeDto
            };

            var originateResult = await OriginateAsync(args, fromCallChannel.Extension, args.RouteData.ToExtension);

            return originateResult;
        }

        private async Task<Result<Channel>> SetMainUser(Guid lineId, Guid callId)
        {
            var channelForMainUser = await _channelRepository.GetChannelForMainUser(lineId);
            if (channelForMainUser != null)
            {
                return Result.Success(channelForMainUser);
            }

            channelForMainUser = await _channelRepository.GetChannelByCallId(callId);
            if (channelForMainUser == null)
            {
                _logger.Warning($"Канал с Id {callId} не найден");
                return Result.Failure<Channel>(ErrorCodes.UserNotFound);
            }

            channelForMainUser.Role = ChannelRoleType.MainUser;
            await _channelRepository.UpdateChannel(channelForMainUser);

            return Result.Success(channelForMainUser);
        }

        private async Task<Result> OriginateAsync(StasisStartEventArgs args, string fromExtension, string toExtension)
        {
            _logger.Debug($"AsteriskAriApiService.OriginateAsync. From: {fromExtension}, To: {toExtension}");
            var encodedArgs = JsonSerializer.EncodeData(args);
            var originateResult = await _ariWebSocketService.OriginateChannel(encodedArgs, fromExtension, toExtension);
            if (!originateResult)
            {
                _logger.Warning("OriginateAsync. Error creating a new channel when answering a call.");
                return Result.Failure(ErrorCodes.UnableToSaveChannel);
            }

            return Result.Success();
        }
    }
}