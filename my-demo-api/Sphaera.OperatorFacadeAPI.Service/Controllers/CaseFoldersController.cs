using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Инциденты
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class CaseFoldersController : demoControllerBase
    {
        private readonly ILogger _logger;
        private readonly CaseFolderService _caseFolderService;

        /// <inheritdoc />
        public CaseFoldersController(ILogger logger, CaseFolderService caseFolderService)
        {
            _logger = logger;
            _caseFolderService = caseFolderService;
        }

        /// <summary>
        /// Получить страницу инцидентов
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getCaseFolderPage")]
        public async Task<demoResult<CaseFolderPageDto>> GetCaseFolderPage(int pageSize = 10, int pageIndex = 1)
        {
            var caseFolderPage = await _caseFolderService.GetCaseFolderPage(pageIndex,pageSize);
            if (caseFolderPage.IsFailure)
            {
                return BadRequest(caseFolderPage.ErrorCode);
            }

            return Answer(caseFolderPage);
        }

        /// <summary>
        /// Получить инцидент
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<demoResult<CaseFolderListItemDto>> Get(Guid id)
        {
            var caseFolder = await _caseFolderService.Get(id);
            if (caseFolder.IsFailure)
            {
                return BadRequest(caseFolder.ErrorCode);
            }

            return Answer(caseFolder);
        }
    }
}
