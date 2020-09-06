using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AsterNET.ARI;
using AsterNET.ARI.Middleware;
using AsterNET.ARI.Middleware.Default;
using AsterNET.ARI.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Infrastructure.Options;
using Channel = AsterNET.ARI.Models.Channel;

namespace demo.DemoGateway.Infrastructure.HostedServices.Asterisk
{
    /// <summary>
    /// Класс для взаимодействия с Asterisk ARI
    /// </summary>
    public class AsteriskAriClient
    {
        /// <summary>
        /// Название приложения в Asterisk
        /// </summary>
        private const string AppName = "ccng";

        /// <summary>
        /// Формат записи аудио файлов
        /// </summary>
        public const string RecordingFormat = "wav";

        private readonly object _bridgesLock = new object();
        private readonly List<string> _bridgesToDestroy = new List<string>();
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly AsteriskAriSmsService _smsService;
        private IAriClient _ariClient;
        private ConnectionState _currentConnectionState;
        private string _endpointHost;

        /// <summary>
        /// Событие StasisStartEvent
        /// </summary>
        public event EventHandler<StasisStartEvent> StartEvent;

        /// <summary>
        /// Событие StasisEndEvent
        /// </summary>
        public event EventHandler<StasisEndEvent> EndEvent;

        /// <summary>
        /// Событие ChannelDestroyed
        /// </summary>
        public event EventHandler<ChannelDestroyedEvent> ChannelDestroyed;

        /// <summary>
        /// Событие BridgeDestroyed
        /// </summary>
        public event EventHandler<BridgeDestroyedEvent> BridgeDestroyed;

        /// <summary>
        /// Событие начала записи
        /// </summary>
        public event EventHandler<RecordingStartedEvent> RecordingStarted;

        /// <summary>
        /// Событие окончания записи
        /// </summary>
        public event EventHandler<RecordingFinishedEvent> RecordingFinished;

        /// <summary>
        /// Конструктор
        /// </summary>
        public AsteriskAriClient(ILogger logger,
            IOptions<AsteriskOptions> options,
            IHostApplicationLifetime applicationLifetime,
            AsteriskAriSmsService smsService)
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _smsService = smsService;

            var ariOptions = options.Value;
            var endpoint = new StasisEndpoint(ariOptions.Ip, ariOptions.Port, ariOptions.Username, ariOptions.Password);
            Start(endpoint);
        }

        /// <summary>
        /// Конструктор для UnitTests. Позволяет передать AriClient
        /// </summary>
        public AsteriskAriClient(ILogger logger, IAriClient ariClient)
        {
            _ariClient = ariClient;
            _logger = logger;
        }

        private void Start(StasisEndpoint endpoint)
        {
            try
            {
                _logger.Information("Starting Asterisk ARI!");

                var ariClient = new AriClient(endpoint, AppName);
                ariClient.OnConnectionStateChanged += AriClientOnConnectionStateChanged;
                ariClient.Connect();
                _ariClient = ariClient;
                _endpointHost = endpoint.Host;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error($"Ошибка подключения к Asterisk ARI. Host: {endpoint.Host}:{endpoint.Port}, Endpoint: {endpoint.AriEndPoint}", ex);
                throw;
            }
        }

        /// <summary>
        /// Ответить на вызов
        /// </summary>
        public async Task Answer(string channelId)
        {
            await _ariClient.Channels.AnswerAsync(channelId);
        }

        /// <summary>
        /// Получить канал по Id
        /// </summary>
        public async Task<Channel> GetChannel(string channelId)
        {
            return await _ariClient.Channels.GetAsync(channelId);
        }

        /// <summary>
        /// Удалить канал из бриджа
        /// </summary>
        public async Task RemoveChannelFromBridge(string bridgeId, string channelId)
        {
            await _ariClient.Bridges.RemoveChannelAsync(bridgeId, channelId);
        }

        /// <summary>
        /// Создать новый бридж
        /// </summary>
        public async Task<Bridge> CreateBridge(string bridgeId = null, string bridgeType = "mixing")
        {
            return await _ariClient.Bridges.CreateAsync(bridgeType, bridgeId);
        }

        /// <summary>
        /// Получить бридж по Id
        /// </summary>
        public async Task<Bridge> GetBridge(string bridgeId)
        {
            try
            {
                return await _ariClient.Bridges.GetAsync(bridgeId);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"DestroyBridge Error. BridgeId: {bridgeId}. Message: {ex.Message}", ex);
            }

            return null;
        }

        /// <summary>
        /// Уничтожить бридж
        /// </summary>
        /// <returns></returns>
        public void DestroyBridge(string bridgeId)
        {
            lock (_bridgesLock)
            {
                try
                {
                    if (_bridgesToDestroy.Contains(bridgeId))
                    {
                        return;
                    }

                    _bridgesToDestroy.Add(bridgeId);
                    _ariClient.Bridges.Destroy(bridgeId);
                    _bridgesToDestroy.Remove(bridgeId);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains(((int)HttpStatusCode.NotFound).ToString()))
                    {
                        _bridgesToDestroy.Remove(bridgeId);
                    }
                    else
                    {
                        _logger.Warning($"DestroyBridge HttpRequest Error. BridgeId: {bridgeId}. Message: {ex.Message}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Добавить канал в бридж
        /// </summary>
        public async Task AddChannelToBridge(string bridgeId, string channelId)
        {
            try
            {
                await _ariClient.Bridges.AddChannelAsync(bridgeId, channelId);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"AddChannelToBridge Error. BridgeId: {bridgeId}. ChannelId: {channelId}. Message: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Отключиться от разговора
        /// </summary>
        public async Task HangupChannel(string channelId)
        {
            try
            {
                await _ariClient.Channels.HangupAsync(channelId);
            }
            catch (HttpRequestException ex)
            {
                if (!ex.Message.Contains(((int)HttpStatusCode.NotFound).ToString()))
                {
                    _logger.Warning($"HangupChannel. HttpRequestError. {ex.Message}. ChannelId: {channelId}", ex);
                }
            }
        }

        /// <summary>
        /// Запустить проигрывание фонового сообщения на удержании
        /// </summary>
        public async Task StartMohInBridgeAsync(string bridgeId)
        {
            try
            {
                await _ariClient.Bridges.StartMohAsync(bridgeId);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"StartMohInBridgeAsync Error. {ex.Message}.", ex);
            }
        }

        /// <summary>
        /// Остановить проигрывание фонового сообщения на удержании
        /// </summary>
        public async Task StopMohInBridgeAsync(string bridgeId)
        {
            try
            {
                await _ariClient.Bridges.StopMohAsync(bridgeId);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"StopMohInBridgeAsync Error. {ex.Message}.", ex);
            }
        }

        /// <summary>
        /// Вызывает Unhold метод в Ari
        /// Если канал входящего вызова (заявителя или другого участника, звонившего в службу) остался один в Bridge и имеет статус "Hold",
        /// мы должны снимать с него статус
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        // ReSharper disable once IdentifierTypo
        public async Task UnholdAsync(string channelId)
        {
            try
            {
                await _ariClient.Channels.UnholdAsync(channelId);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"{nameof(UnholdAsync)} Error. {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создать новый канал. После вызова возникнет событие StasisStartEvent
        /// </summary>
        public async Task<bool> Originate(string args, string extensionFrom, string extensionTo, string channelId = null)
        {
            var endpoint = GetOriginateEndpoint(extensionTo);
            try
            {
                var channel = await _ariClient.Channels.OriginateAsync(endpoint, app: AppName, appArgs: $"\"{args}\"", callerId: extensionFrom, channelId: channelId, timeout: 45);
                _logger.Debug($"AsteriskAriClient.Originate called. Endpoint: {endpoint}, From: {extensionFrom}, To: {extensionTo}. ChannelId: {channel?.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Originate Error. {ex.Message}. Args: {args}", ex);
            }

            return false;
        }

        /// <summary>
        /// Создать snoop-копию канала. После вызова возникнет событие StasisStartEvent
        /// </summary>
        /// <returns>Id созданного канала</returns>
        public async Task<Result<string>> SnoopChannel(string agentChannelId, string spy = null, string whisper = null,
            string args = null, string snoopChannelId = null)
        {
            try
            {
                var channelInfo = $"SNOOP channel {snoopChannelId}; OriginalChannelId: {agentChannelId}; whisper: {whisper}; spy: {spy}";
                _logger.Information(channelInfo);
                var channel = await _ariClient.Channels.SnoopChannelAsync(agentChannelId, AppName, spy, whisper, $"\"{args}\"", snoopChannelId);
                await _ariClient.Channels.SetChannelVarAsync(snoopChannelId, "AdditionalInfo", channelInfo);
                return Result.Success(channel.Id);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warning($"SnoopChannel Error. {ex.Message}. Args: {args}", ex);
            }
            catch (Exception ex)
            {
                _logger.Warning($"SnoopChannel.Error. {ex.Message}", ex);
            }

            return Result.Failure(ErrorCodes.SnoopError);
        }

        /// <summary>
        /// Выключить исходящий звук канала
        /// </summary>
        public async Task MuteChannel(string channelId, string direction = "out")
        {
            await _ariClient.Channels.MuteAsync(channelId, direction);
        }

        /// <summary>
        /// Включить исходящий звук канала
        /// </summary>
        public async Task UnmuteChannel(string channelId, string direction = "out")
        {
            await _ariClient.Channels.UnmuteAsync(channelId, direction);
        }

        /// <summary>
        /// Проигрывать гудки в канал
        /// </summary>
        public async Task<string> PlayBeeps(string channelId)
        {
            try
            {
                var playbackId = Guid.NewGuid().ToString();
                await _ariClient.Channels.PlayWithIdAsync(channelId, playbackId, "tone:ring;tonezone=ru");
                return playbackId;
            }
            catch (Exception e)
            {
                _logger.Warning($"PlayBeeps.Error {e.Message}. ChannelId: {channelId}", e);
                return null;
            }
        }

        /// <summary>
        /// Остановить проигрывание гудков
        /// </summary>
        public async Task StopBeeps(string playbackId)
        {
            try
            {
                await _ariClient.Playbacks.StopAsync(playbackId);
            }
            catch (Exception e)
            {
                _logger.Warning($"StopBeeps.Error {e.Message}. PlaybackId: {playbackId}", e);
            }
        }

        /// <summary>
        /// Начать запись звука в бридже
        /// </summary>
        public async Task<Result> StartRecordingBridge(string bridgeId, string fileName = null)
        {
            if (fileName == null)
            {
                fileName = bridgeId;
            }

            try
            {
                await _ariClient.Bridges.RecordAsync(bridgeId, fileName, RecordingFormat);
            }
            catch (Exception e)
            {
                _logger.Warning($"StartRecordingBridge.Error {e.Message}. bridgeId: {bridgeId}, fileName: {fileName}", e);
                return Result.Failure(ErrorCodes.RecordingError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Начать запись звука канала
        /// </summary>
        public async Task<Result> StartRecordingChannel(string channelId, string fileName = null)
        {
            if (fileName == null)
            {
                fileName = channelId;
            }

            try
            {
                await _ariClient.Channels.RecordAsync(channelId, fileName, RecordingFormat);
            }
            catch (Exception e)
            {
                _logger.Warning($"StartRecordingChannel.Error {e.Message}. channelId: {channelId}, fileName: {fileName}", e);
                return Result.Failure(ErrorCodes.RecordingError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Получение PJSIP Endpoint
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetOriginateEndpoint(string extension)
        {
            return $"PJSIP/{extension}@kamailio";
        }

        private void AriClientOnConnectionStateChanged(object sender)
        {
            var newConnectionState = ((WebSocketEventProducer)sender).State;

            _logger.Information($"AriClient State Changed: OldState: {_currentConnectionState}. NewState: {newConnectionState}");

            if (_currentConnectionState != ConnectionState.Open && newConnectionState == ConnectionState.Open)
            {
                _logger.Information("Socket connection with ASTERISK has been established");
                _ariClient.OnStasisStartEvent += AriClientOnStasisStartEvent;
                _ariClient.OnStasisEndEvent += AriClientOnStasisEndEvent;
                _ariClient.OnChannelDestroyedEvent += AriClient_OnChannelDestroyedEvent;
                _ariClient.OnBridgeDestroyedEvent += AriClient_OnBridgeDestroyedEvent;
                _ariClient.OnChannelUsereventEvent += _smsService.OnChannelUserEvent;
                _ariClient.OnRecordingStartedEvent += AriClient_OnRecordingStartedEvent;
                _ariClient.OnRecordingFinishedEvent += AriClient_OnRecordingFinishedEvent;
            }
            else if (_currentConnectionState == ConnectionState.Open && newConnectionState != ConnectionState.Open)
            {
                _logger.Error("The connection with ASTERISK on sockets was broken. Start disabling Application");

                _applicationLifetime.StopApplication();
            }
            else if (_currentConnectionState == ConnectionState.Connecting && newConnectionState == ConnectionState.Connecting)
            {
                _logger.Error("The connection with ASTERISK on sockets was broken. Start disabling Application");

                _applicationLifetime.StopApplication();
            }
            _currentConnectionState = newConnectionState;
        }

        private void AriClient_OnRecordingStartedEvent(IAriClient sender, RecordingStartedEvent e)
        {
            RecordingStarted?.Invoke(this, e);
        }

        private void AriClient_OnRecordingFinishedEvent(IAriClient sender, RecordingFinishedEvent e)
        {
            RecordingFinished?.Invoke(this, e);
        }

        private void AriClient_OnBridgeDestroyedEvent(IAriClient sender, BridgeDestroyedEvent e)
        {
            BridgeDestroyed?.Invoke(this, e);
        }

        private void AriClient_OnChannelDestroyedEvent(IAriClient sender, ChannelDestroyedEvent e)
        {
            ChannelDestroyed?.Invoke(this, e);
        }

        private void AriClientOnStasisStartEvent(IAriClient sender, StasisStartEvent e)
        {
            StartEvent?.Invoke(this, e);
        }

        private void AriClientOnStasisEndEvent(IAriClient sender, StasisEndEvent e)
        {
            EndEvent?.Invoke(this, e);
        }

        /// <summary>
        /// Получить список всех бриджей и отдельно существующих каналов
        /// </summary>
        public async Task<IList<BridgeDto>> GetAllBridges()
        {
            var bridges = (await _ariClient.Bridges.ListAsync())
                .Select(BridgeDto.MapFromBridge)
                .OrderBy(x => x.Id)
                .ToList();
            var channelsInBridges = bridges.SelectMany(x => x.Channels);

            var allChannels = (await _ariClient.Channels.ListAsync()).Select(x => x.Id);
            var channelsWithoutBridge = allChannels.Except(channelsInBridges).ToList();

            if (channelsWithoutBridge.Any())
            {
                bridges.Add(new BridgeDto
                {
                    Id = "ChannelsWithoutBridge",
                    Channels = channelsWithoutBridge
                });
            }

            return bridges;
        }

        /// <summary>
        /// Удалить все бриджи и каналы.
        /// </summary>
        public async Task DestroyAllBridges()
        {
            var bridges = await _ariClient.Bridges.ListAsync();
            var channelsInBridges = new List<string>();
            foreach (var bridge in bridges)
            {
                try
                {
                    channelsInBridges.AddRange(bridge.Channels);
                    await _ariClient.Bridges.DestroyAsync(bridge.Id);
                }
                catch (Exception ex)
                {
                    _logger.Information($"DestroyAllBridges.Error {ex.Message}. BridgeId: {bridge.Id}");
                }
            }

            var allChannels = (await _ariClient.Channels.ListAsync()).Select(x => x.Id);
            var channelsWithoutBridge = allChannels.Except(channelsInBridges).ToList();

            foreach (var channelId in channelsWithoutBridge)
            {
                try
                {
                    await _ariClient.Channels.HangupAsync(channelId);
                }
                catch (Exception ex)
                {
                    _logger.Information($"DestroyAllBridges.Error delete channel without bridge {ex.Message}. ChannelId: {channelId}");
                }
            }
        }

        /// <summary>
        /// Создать Id канала
        /// </summary>
        public string CreateChannelId(ChannelRoleType role, string extension)
        {
            var snoopChannelId = $"{_endpointHost}_{DateTime.Now.Ticks}_{role}_{extension}";
            return snoopChannelId;
        }

        /// <summary>
        /// Взять данные астериска.
        /// </summary>
        /// <returns>Данные астериска</returns>
        public async Task<Result<string>> GetInfoAsync()
        {
            try
            {
                _logger.Information("Try get info");
                var result = await _ariClient.Asterisk.GetInfoAsync();
                _logger.WithTag("data", result).Information("Get info success");
                return Result.Success(JsonConvert.SerializeObject(result));
            }
            catch (Exception e)
            {
                _logger.Warning("Get info failed", e);
                return Result.Failure(ErrorCodes.GetAsteriskInfoError);
            }
        }
    }
}