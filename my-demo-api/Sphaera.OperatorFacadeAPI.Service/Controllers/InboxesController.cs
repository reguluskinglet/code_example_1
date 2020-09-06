using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Inbox;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Методы для работы с очередями.
    /// </summary>
    [Route("api/[controller]"), ApiController]
    public class InboxesController : demoControllerBase
    {
        private readonly InboxService _inboxService;

        /// <summary>
        /// Конструктор для инъекции зависимостей
        /// </summary>
        /// <param name="inboxService"></param>
        public InboxesController(InboxService inboxService)
        {
            _inboxService = inboxService;
        }

        /// <summary>
        /// Получение списка доступных очередей по выбранным ролям
        /// </summary>
        [HttpGet]
        public async Task<demoResult<InboxesResponseDto>> Get([FromQuery] Guid[] roleIds)
        {
            Result<InboxesResponseDto> inboxesResult = await _inboxService.GetInboxes(roleIds.ToList());

            return Answer(inboxesResult);
        }

        /// <summary>
        /// Получение списка доступных очередей по выбранным ролям
        /// </summary>
        [HttpGet]
        [Route("{inboxId}")]
        public async Task<demoResult<InboxDto>> Get(Guid inboxId)
        {
            Result<InboxDto> inboxesResult = await _inboxService.GetInbox(inboxId);

            return Answer(inboxesResult);
        }
    }
}
