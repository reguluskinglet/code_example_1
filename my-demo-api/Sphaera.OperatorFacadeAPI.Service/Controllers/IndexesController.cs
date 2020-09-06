using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Index;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер по получению информации о индексах карточек событий
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class IndexesController : demoControllerBase
    {
        private readonly IndexApplicationService _indexApplicationService;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public IndexesController(IndexApplicationService indexApplicationService, ILogger logger)
        {
            _logger = logger;
            _indexApplicationService = indexApplicationService;
        }

        /// <summary>
        /// Получить дерево индексов определенного типа карточки инцидента
        /// </summary>
        [HttpGet]
        [Route("getByCaseTypeId/{caseTypeId}")]
        public async Task<demoResult<IndexesDto>> GetCaseTypeIndexes(Guid caseTypeId)
        {
            if (caseTypeId == default)
            {
                _logger.Warning($"{nameof(caseTypeId)} not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<IndexesDto> result = await _indexApplicationService.GetIndexTreeByCaseTypeId(caseTypeId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            if (result.Value == null)
            {
                return BadRequest(ErrorCodes.IndexTreeNotFound);
            }

            return Answer(result);
        }

        /// <summary>
        /// Получить текущий индекс карточки инцидента
        /// </summary>
        [HttpGet]
        [Route("getCaseIndex/{caseId}")]
        public async Task<demoResult<IndexDto>> GetCaseIndex(Guid caseId)
        {
            if (caseId == default)
            {
                _logger.Warning($"{nameof(caseId)} not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<IndexDto> result = await _indexApplicationService.GetIndexByCaseId(caseId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }

        /// <summary>
        /// Сохранить или обновить индекс карточки инцидента
        /// </summary>
        [HttpPost]
        [Route("saveCaseIndex")]
        public async Task<demoResult<IndexDto>> SaveCaseIndex([FromBody] CaseIndexDto caseIndexDto)
        {
            if (caseIndexDto == null || !caseIndexDto.IsValid())
            {
                _logger.Warning($"Model not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _indexApplicationService.SaveCaseIndex(caseIndexDto.CaseId, caseIndexDto.IndexId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }
    }
}
