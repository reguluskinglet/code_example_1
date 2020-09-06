using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Role;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер для ролей и рабочих заданий
    /// </summary>
    [Authorize]
    [Route("api/[controller]"), ApiController]
    public class RolesController : demoControllerBase
    {
        private readonly InboxService _inboxService;

        /// <inheritdoc />
        public RolesController(InboxService inboxService)
        {
            _inboxService = inboxService;
        }

        /// <summary>
        /// Получить все роли, назначенные пользователю
        /// </summary>
        [HttpGet]
        public async Task<demoResult<List<RoleClientDto>>> Get()
        {
            Result<List<RoleDto>> rolesResult = await _inboxService.GetUserRoles();

            return Answer(rolesResult);
        }

        /// <summary>
        /// Получить все рабочие задания, доступные пользователю
        /// </summary>
        [HttpGet]
        [Route("workChoices")]
        public async Task<demoResult<List<WorkChoiceClientDto>>> GetWorkChoices()
        {
            Result<List<WorkChoiceDto>> workChoicesResult = await _inboxService.GetUserWorkChoices();

            return Answer(workChoicesResult);
        }
    }
}
