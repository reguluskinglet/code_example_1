using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.DemoApi.Service.ApplicationServices.Abstractions;
using demo.DemoApi.Service.Dtos.Line;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер линии.
    /// </summary>
    [Authorize]
    [Route("api/[controller]"), ApiController]
    public class LinesController : demoControllerBase
    {
        private readonly ILineService _lineService;

        /// <summary>
        /// Конструктор для инъекции зависимостей.
        /// </summary>
        /// <param name="lineService"></param>
        public LinesController(ILineService lineService)
        {
            _lineService = lineService;
        }

        /// <summary>
        /// Получить все линии для пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<demoResult<List<LineViewDto>>> GetByUserId()
        {
            Result<List<LineViewDto>> result = await _lineService.GetUserLines(GetUserId());

            return Answer(result);
        }

        /// <summary>
        /// Получить все линии по CaseFolderId
        /// </summary>
        /// <returns></returns>
        [HttpGet("getByCaseFolder/{caseFolderId}")]
        public async Task<demoResult<List<LineByCaseFolderViewDto>>> GetByCaseFolder(Guid caseFolderId)
        {
            Result<List<LineByCaseFolderViewDto>> result = await _lineService.GetLinesByCaseFolderId(caseFolderId);

            return Answer(result);
        }
    }
}
