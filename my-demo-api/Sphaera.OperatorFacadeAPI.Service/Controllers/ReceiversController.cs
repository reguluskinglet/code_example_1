using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Receiver;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер для ролей и рабочих заданий
    /// </summary>
    [Authorize]
    [Route("api/[controller]"), ApiController]
    public class ReceiversController : demoControllerBase
    {
        private readonly InboxService _inboxService;

        /// <inheritdoc />
        public ReceiversController(InboxService inboxService)
        {
            _inboxService = inboxService;
        }

        /// <summary>
        /// Получить получателей для подключения к разговору
        /// </summary>
        [HttpGet]
        public async Task<demoResult<ReceiversResponseDto>> Get()
        {
            Result<ReceiversResponseDto> receiversResult = await _inboxService.GetConnectionReceivers();

            return Answer(receiversResult);
        }
    }
}
