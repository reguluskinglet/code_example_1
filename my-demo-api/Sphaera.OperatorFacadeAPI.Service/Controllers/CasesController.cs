using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Dtos.Case;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Карточки
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class CasesController : demoControllerBase
    {
        private readonly ILogger _logger;
        private readonly CaseService _caseService;

        /// <inheritdoc />
        public CasesController(ILogger logger, CaseService caseService)
        {
            _logger = logger;
            _caseService = caseService;
        }

        /// <summary>
        /// Получить Case по Id
        /// </summary>
        /// <param name="caseId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<demoResult<CaseDto>> Get(Guid caseId)
        {
            if (caseId == default)
            {
                _logger.Warning($"{nameof(caseId)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<CaseDto> result = await _caseService.GetCaseById(caseId);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Получить Case по CaseFolderId для указанного оператора
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getCase")]
        public async Task<demoResult<CaseDto>> GetCase(Guid caseFolderId)
        {
            if (caseFolderId == default)
            {
                _logger.Warning($"{nameof(caseFolderId)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<CaseDto> result = await _caseService.GetCaseByCaseFolderIdAsync(caseFolderId, GetUserId());
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Получение данных поля карточки
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("field")]
        public async Task<demoResult<CaseFieldDto>> GetField(Guid caseFolderId, Guid fieldId)
        {
            if (caseFolderId == default)
            {
                _logger.Warning($"{nameof(caseFolderId)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            if (fieldId == default)
            {
                _logger.Warning($"{nameof(fieldId)} not set.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.GetFieldDataAsync(caseFolderId, fieldId);

            if (result.IsFailure)
            {
                return Answer(result);
            }

            if (result.Value.Value == null)
            {
                return BadRequest(ErrorCodes.ValidationError);
            }

            return Answer(result);
        }

        /// <summary>
        /// Обновление данных поля карточки
        /// </summary>
        /// <param name="fieldDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("field")]
        public async Task<demoResult> UpdateField([FromBody] CaseFieldDto fieldDto)
        {
            if (fieldDto == null || !fieldDto.IsValid())
            {
                _logger.Warning($"Model not valid.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.UpdateFieldAsync(GetUserId(), fieldDto);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Отправка события изменения поля карточки.
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("setActiveField")]
        public async Task<demoResult> SetActiveField(Guid caseFolderId, Guid fieldId)
        {
            var result = await _caseService.SetActiveFieldAsync(GetUserId(), caseFolderId, fieldId);

            if (result.IsFailure)
            {
                _logger.Warning("Error changing the status of the active card field");
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Обновление данных поля карточки
        /// </summary>
        [HttpGet]
        [Route("caseTitles")]
        public async Task<demoResult<TitlesDto>> GetCaseTitles(Guid caseFolderId)
        {
            var result = await _caseService.GetCaseTitlesAsync(caseFolderId);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }

        /// <summary>
        /// Получить список шаблонов
        /// </summary>
        [HttpGet]
        [Route("caseTypes")]
        public async Task<demoResult<TitlesDto>> GetCaseTypes()
        {
            List<CaseTypeDto> result = await _caseService.GetCaseTypesInfoAsync();
            return Ok(result);
        }

        /// <summary>
        /// Получение данных плана реагирования
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("plan")]
        public async Task<demoResult<List<ActivatedPlanInstructionDto>>> GetPlanData(Guid caseId, Guid planId)
        {
            if (caseId == default)
            {
                _logger.Warning($"{nameof(caseId)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            if (planId == default)
            {
                _logger.Warning($"{nameof(planId)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            List<ActivatedPlanInstructionDto> activatedInstructions = await _caseService.GetActivatedPlanInstructionsAsync(caseId, planId);

            return Ok(activatedInstructions);
        }

        /// <summary>
        /// Обновление активированных инструкций плана
        /// </summary>
        /// <param name="activatedPlanInstructionDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("plan")]
        public async Task<demoResult> UpdatePlan([FromBody] ActivatedPlanInstructionDto activatedPlanInstructionDto)
        {
            if (activatedPlanInstructionDto == null || !activatedPlanInstructionDto.IsValid())
            {
                _logger.Warning($"Model not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.UpdateActivatedPlanInstructionsAsync(GetUserId(), activatedPlanInstructionDto);

            if (result.IsFailure)
            {
                _logger.Warning("Error updating card field");
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Получение координат места происшествия
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("location/{caseFolderId}")]
        public async Task<demoResult<IncidentLocationDto>> GetLocationData(Guid caseFolderId)
        {
            if (caseFolderId == default)
            {
                _logger.Warning($"{nameof(caseFolderId)} not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<IncidentLocationDto> result = await _caseService.GetLocationAsync(caseFolderId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }
        
        /// <summary>
        /// Получение координат места происшествия в ESPG:3857
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getIncidentLocation/{caseFolderId}")]
        public async Task<demoResult<IncidentLocationDto>> GetIncidentLocationData(Guid caseFolderId)
        {
            if (caseFolderId == default)
            {
                _logger.Warning($"{nameof(caseFolderId)} not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<IncidentLocationDto> result = await _caseService.GetIncidentLocationEspg3857Async(caseFolderId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }

        /// <summary>
        /// Обновление координат места происшествия
        /// </summary>
        /// <param name="locationDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("location")]
        public async Task<demoResult> UpdateLocation([FromBody] UpdateLocationDto locationDto)
        {
            if (locationDto == null || !locationDto.IsValid())
            {
                _logger.Warning($"Model not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.UpdateLocationAsync(locationDto);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Отправка события изменения поля координат.
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("setActiveLocationField")]
        public async Task<demoResult> SetActiveField(Guid caseFolderId, CoordinateFieldType fieldType)
        {
            if (fieldType == CoordinateFieldType.Undefined || caseFolderId == default)
            {
                _logger.Warning($"Model not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.SetActiveLocationFieldAsync(GetUserId(), caseFolderId, fieldType);

            if (result.IsFailure)
            {
                _logger.Warning(result.ErrorMessage);
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Отобразить на карте
        /// </summary>
        /// <param name="caseOperatorDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("showLocation")]
        public async Task<demoResult> ShowLocationOnMap([FromBody] CaseOperatorDto caseOperatorDto)
        {
            if (caseOperatorDto == null || !caseOperatorDto.IsValid())
            {
                _logger.Warning($"Model not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.AddIncidentLocationOnMapAsync(caseOperatorDto);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }
        
        /// <summary>
        /// Отобразить на карте местоположение заявителя.
        /// </summary>
        /// <param name="caseApplicantDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("useApplicantLocation")]
        public async Task<demoResult> UseApplicantLocationAsIncidentLocation([FromBody] CaseApplicantDto caseApplicantDto)
        {
            if (caseApplicantDto == null || !caseApplicantDto.IsValid())
            {
                _logger.Warning($"Model {nameof(caseApplicantDto)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.UseApplicantLocationAsIncidentLocationAsync(GetUserId(), caseApplicantDto);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }
        
        /// <summary>
        /// Отобразить на карте местоположение заявителя.
        /// </summary>
        /// <param name="caseApplicantDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("showApplicantLocationOnMap")]
        public async Task<demoResult> ShowApplicantLocationOnMap([FromBody] CaseApplicantDto caseApplicantDto)
        {
            if (caseApplicantDto == null || !caseApplicantDto.IsValid())
            {
                _logger.Warning($"Model {nameof(caseApplicantDto)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.AddApplicantLocationOnMapAsync(caseApplicantDto);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Получение координат местоположения заявителя в ESPG-3857.
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="caseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("applicantLocation")]
        public async Task<demoResult<ApplicantLocationDto>> GetApplicantLocationData(Guid caseFolderId, Guid userId)
        {
            if (caseFolderId == default || userId == default)
            {
                _logger.Warning($"Parameters not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<ApplicantLocationDto> result = await _caseService.GetApplicantLocationAsync(caseFolderId, userId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok(result.Value);
        }
        
        /// <summary>
        /// Получение информации о статусах Cases в CaseFolder
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("statuses")]
        public async Task<demoResult<CaseStatusesInfoDto>> Statuses(Guid caseFolderId)
        {
            var userId = GetUserId();

            if (caseFolderId == default)
            {
                _logger.Warning($"{nameof(caseFolderId)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }
            var result = await _caseService.GetStatusesInfo(caseFolderId, userId);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Изменить статус Case в "Closed"
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("close")]
        public async Task<demoResult> CloseCaseCard([FromBody] CloseCaseCardDto dto)
        {
            if (dto == null || !dto.IsValid())
            {
                _logger.Warning($"Model {nameof(dto)} not valid");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _caseService.CloseCaseCard(GetUserId(), dto);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }
    }
}
