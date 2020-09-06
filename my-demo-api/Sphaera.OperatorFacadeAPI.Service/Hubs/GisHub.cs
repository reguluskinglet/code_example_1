using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos;

namespace demo.DemoApi.Service.Hubs
{
    /// <summary>
    /// SignalR Hub для окна с картой
    /// </summary>
    public class GisHub : Hub
    {
        private readonly GisService _gisService;

        /// <inheritdoc />
        public GisHub(GisService gisService)
        {
            _gisService = gisService;
        }

        /// <summary>
        /// Сохранить текущего оператора для маппинга оператора на Connection
        /// </summary>
        /// <param name="user"></param>
        public async Task SetCurrentOperator(UserDto user)
        {
            await _gisService.SetCurrentOperator(user, Context.ConnectionId);
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _gisService.RemoveOperator(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
