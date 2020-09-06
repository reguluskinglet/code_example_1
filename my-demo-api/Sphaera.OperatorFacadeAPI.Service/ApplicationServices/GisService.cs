using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using demo.FunctionalExtensions;
using demo.GisFacade.Client;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices.Cache;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Hubs;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для взаимодействия с картой
    /// </summary>
    public class GisService
    {
        private const string GisOperatorsCacheCollectionName = "GisOperatorsCacheCollectionName";
        private readonly ILogger _logger;
        private readonly IHubContext<GisHub> _gisHubContext;
        private readonly CacheProvider _cacheProvider;
        private readonly GisFacadeClient _gisFacadeClient;

        /// <inheritdoc />
        public GisService(
            ILogger logger,
            IHubContext<GisHub> gisHubContext,
            CacheProvider cacheProvider,
            GisFacadeClient gisFacadeClient)
        {
            _logger = logger;
            _gisHubContext = gisHubContext;
            _cacheProvider = cacheProvider;
            _gisFacadeClient = gisFacadeClient;
        }

        /// <summary>
        /// Оповестить всех клиентов о новом звонке в очереди
        /// </summary>
        /// <param name="callId"></param>
        /// <returns></returns>
        public async Task NotifyAboutNewCall(Guid callId)
        {
            await _gisHubContext.Clients.All.SendAsync("OnNewCall", callId);
        }

        /// <summary>
        /// Оповестить клиента о принятом звонке
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="callId"></param>
        /// <returns></returns>
        public async Task NotifyAboutAcceptedCall(Guid userId, Guid callId)
        {
            var connectionId = await _cacheProvider.GetAsync<string>(GisOperatorsCacheCollectionName, userId.ToString());

            if (string.IsNullOrWhiteSpace(connectionId))
            {
                _logger.Warning($"Не найден оператор с {nameof(userId)}: {userId}");
            }
            else
            {
                await _gisHubContext.Clients.Client(connectionId).SendAsync("OnCallAccepted", callId);
            }
        }

        /// <summary>
        /// Оповестить клиентов об окончании звонка
        /// </summary>
        /// <param name="callId"></param>
        /// <returns></returns>
        public async Task NotifyAboutCallEnd(Guid callId)
        {
            await _gisHubContext.Clients.All.SendAsync("OnCallEnd", callId);
        }

        /// <summary>
        /// Сделать запрос в GisFacade для создания маркера происшествия и оповестить клиента
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public async Task<Result> AddIncidentMarkerToMap(Guid caseFolderId, double latitude, double longitude)
        {
            var addMarkerResult = await _gisFacadeClient.AddIncidentMarker(caseFolderId, latitude, longitude);
            if (addMarkerResult.IsFailure)
            {
                _logger.Warning(addMarkerResult.ErrorMessage);
                return Result.Failure(ErrorCodes.UnableToAddIncidentMarkerToMap);
            }

            return Result.Success();
        }

        /// <summary>
        /// Привязать текущего оператора к ConnectionId
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hubConnectionId"></param>
        public async Task SetCurrentOperator(UserDto user, string hubConnectionId)
        {
            if (user == null || user.Id == default)
            {
                _logger.Warning("Opened client window with a card without an authorized user.");
                return;
            }

            await _cacheProvider.SetKeyValueAsync(GisOperatorsCacheCollectionName, user.Id.ToString(), hubConnectionId);
        }

        /// <summary>
        /// Удалить соответствие оператора и ConnectionId
        /// </summary>
        /// <param name="hubConnectionId"></param>
        public async Task RemoveOperator(string hubConnectionId)
        {
            var result = await _cacheProvider.RemoveKeyByValue(GisOperatorsCacheCollectionName, hubConnectionId);
            if (result.IsFailure)
            {
                _logger.Warning($"Оператор с {nameof(hubConnectionId)} не найден");
            }
        }
    }
}
