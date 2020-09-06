using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.ApplicationServices.Cache;

namespace demo.DemoApi.Service.Hubs
{
    /// <summary>
    /// SignalR Хаб.
    /// </summary>
    public class PhoneHub : Hub
    {
        private const string ActiveOperatorsCacheCollectionName = "ActiveOperatorsCacheCollectionName";
        private readonly ILogger _logger;
        private readonly UserService _userService;
        private readonly CacheProvider _cacheProvider;
        private readonly CallManagementService _callManagementService;

        /// <summary>
        /// Создать
        /// </summary>
        public PhoneHub(ILogger logger,
            UserService userService,
            CacheProvider cacheProvider,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _userService = userService;
            _cacheProvider = cacheProvider;
            _callManagementService = callManagementService;
        }

        /// <summary>
        /// Оператор вошел в систему
        /// </summary>
        public async Task OperatorConnect(Guid? userId)
        {
            _logger.Debug($"SignalR 'OperatorConnect' Begin");
            try
            {
                if (userId == null)
                {
                    _logger.Error($"SignalR 'OperatorConnect'. UserId is null");
                    return;
                }

                var connectionId = Context.ConnectionId;
                _logger.Debug($"User connected to PhoneHub; {nameof(userId)}: {userId} {nameof(connectionId)}: {connectionId}");
                await _userService.ChangeActivityStatus(userId.Value, true);
                await _cacheProvider.SetAsync(ActiveOperatorsCacheCollectionName, connectionId, userId);
            }
            catch (Exception e)
            {
                _logger.Error($"SignalR Exception on 'OperatorConnect'. ; {nameof(userId)}: {userId}", e);
                return;
            }

            _logger.Debug($"SignalR 'OperatorConnect' End");
        }

        /// <inheritdoc/>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.Verbose($"OnDisconnectedAsync (SignalR). Begin. Possible Exception Message: {exception?.Message}");
            try
            {
                await OperatorDisconnect();
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception e)
            {
                _logger.Error("Exception on SignalR 'OnDisconnectedAsync'", e);
                throw;
            }
        }

        /// <summary>
        /// Оператор вышел из системы
        /// </summary>
        private async Task OperatorDisconnect()
        {
            var userId = await _cacheProvider.GetAsync<Guid>(ActiveOperatorsCacheCollectionName, Context.ConnectionId);
            if (userId == default)
            {
                _logger.Warning($"OperatorDisconnect. UserId not found in cache on SignalR disconnection");
                return;
            }

            _logger.Debug($"Successfully received extension from cache while disconnecting client from SignalR userId '{userId}' ");
            await _userService.ChangeActivityStatus(userId, false);
            await _cacheProvider.RemoveAsync(ActiveOperatorsCacheCollectionName, Context.ConnectionId);
            await _callManagementService.EndAllUserCalls(userId);

            _logger.Debug($"A notification was sent that the user with Id '{userId}' disconnected from SignalR hub");
        }
    }
}
