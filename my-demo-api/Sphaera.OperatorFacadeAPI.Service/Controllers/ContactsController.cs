using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Contact;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер по получению информации о контактах
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : demoControllerBase
    {
        private readonly ContactManagementService _contactManagementService;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public ContactsController(ContactManagementService contactManagementService, ILogger logger)
        {
            _logger = logger;
            _contactManagementService = contactManagementService;
        }

        /// <summary>
        /// Получить контакты
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        [HttpGet]
        [Route("getContacts")]
        public async Task<demoResult<ContactsPageDto>> GetContacts(string filter, int pageSize, int pageIndex)
        {
            if (pageSize == default)
            {
                _logger.Warning($"{nameof(pageSize)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            if (pageIndex == default)
            {
                _logger.Warning($"{nameof(pageIndex)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<ContactsPageDto> result = await _contactManagementService.GetContactsPage(filter, pageSize, pageIndex);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }
    }
}
