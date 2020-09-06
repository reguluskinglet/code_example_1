using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.ApplicationServices.Lines;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Calls;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Звонки заявителей
    /// </summary>
    [Authorize]
    [Route("api/[controller]"), ApiController]
    public class CallsController : demoControllerBase
    {
        private readonly CallManagementService _callManagementService;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public CallsController(
            ILogger logger,
            CallManagementService callManagementService)
        {
            _logger = logger;
            _callManagementService = callManagementService;
        }

        /// <summary>
        /// Получить журнал звонков
        /// </summary>
        [HttpGet("getJournalCalls")]
        public async Task<demoResult<List<JournalCallsDto>>> GetJournalCalls(CallTypeFilter filter, bool notAcceptedOnly)
        {
            var journalCallsResult = await _callManagementService.GetJournalCalls(GetUserId(), filter, notAcceptedOnly);
            if (journalCallsResult.IsFailure)
            {
                _logger.Warning($"Error on CallsController GetJournalCalls. {journalCallsResult.ErrorMessage}");
                return BadRequest(journalCallsResult.ErrorCode);
            }

            return Answer(journalCallsResult);
        }

        /// <summary>
        /// Принять вызов.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("accept")]
        public async Task<demoResult> Accept([FromBody] AcceptInboxItemDto dto)
        {
            var validationResult = dto.IsValid();
            if (validationResult.IsFailure)
            {
                _logger.Warning($"Ошибка on CallsController Accept. {validationResult.ErrorMessage}");
                return BadRequest(validationResult.ErrorCode);
            }

            var result = await _callManagementService.AcceptInboxItem(GetUserId(), dto.InboxId, dto.ItemId);
            if (result.IsFailure)
            {
                _logger.Warning($"Ошибка вызова AcceptInboxItem. {result.ErrorMessage}");
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Количество вызовов ожидающих заявителей.
        /// </summary>
        /// <param name="isolationStatusDto"></param>
        /// <returns></returns>
        [HttpPut("isolate")]
        public async Task<demoResult> SetIsolationStatus([FromBody] IsolationStatusDto isolationStatusDto)
        {
            var result = await _callManagementService.SetIsolationStatus(isolationStatusDto.CallId, isolationStatusDto.Isolated);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Установка статуса удержания
        /// </summary>
        /// <param name="holdStatusDto"></param>
        /// <returns></returns>
        [HttpPut("hold")]
        public async Task<demoResult> SetHoldStatus([FromBody] HoldStatusDto holdStatusDto)
        {
            var result = await _callManagementService.SetHoldStatus(holdStatusDto.CallId, holdStatusDto.OnHold);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Завершить вызов.
        /// </summary>
        /// <param name="callId"></param>
        /// <returns></returns>
        [HttpPost("endCallByUser/{callId}")]
        public async Task<demoResult> EndCallByUser(Guid callId)
        {
            var result = await _callManagementService.EndCallByUser(callId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Добавить оператора в режим конференции.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addOperatorToConference")]
        public async Task<demoResult> AddOperatorToConference([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddCallToLine(dto.LineId, dto.CaseFolderId, GetUserId(), dto.Destination, ConnectionMode.Conference);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Вызов ассистента.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addAssistance")]
        public async Task<demoResult> AddAssistance([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddCallToLine(dto.LineId, dto.CaseFolderId, GetUserId(), dto.Destination, ConnectionMode.Assistance);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Вызов пользователя в режиме частичного ассистирования.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addPartialAssistance")]
        public async Task<demoResult> AddPartialAssistance([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddCallToLine(dto.LineId, dto.CaseFolderId, GetUserId(), dto.Destination, ConnectionMode.PartialAssistance);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Направить вызов группе пользователей в режиме конференции.
        /// </summary>
        [HttpPost("addUserFromGroupToConference")]
        public async Task<demoResult> AddUserFromGroupToConference([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddUserGroupCallToLine(dto.LineId,
                dto.CaseFolderId,
                GetUserId(),
                dto.Destination,
                ConnectionMode.Conference);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Направить вызов группе пользователей в режиме ассистирования
        /// </summary>
        [HttpPost("addAssistanceFromGroup")]
        public async Task<demoResult> AddAssistantFromGroup([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddUserGroupCallToLine(dto.LineId,
                dto.CaseFolderId,
                GetUserId(),
                dto.Destination,
                ConnectionMode.Assistance);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Направить вызов группе пользователей в режиме частичного ассистирования
        /// </summary>
        [HttpPost("addPartialAssistanceFromGroup")]
        public async Task<demoResult> AddPartialAssistanceFromGroup([FromBody] ConnectionToLineDto dto)
        {
            var result = await _callManagementService.AddUserGroupCallToLine(dto.LineId,
                dto.CaseFolderId,
                GetUserId(),
                dto.Destination,
                ConnectionMode.PartialAssistance);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Поменять роли главного оператора и ассистента.
        /// </summary>
        /// <param name="exchangeRolesDto"></param>
        /// <returns></returns>
        [HttpPost("exchangeOperatorRoles")]
        public async Task<demoResult> ExchangeOperatorRoles([FromBody] ExchangeRolesDto exchangeRolesDto)
        {
            var result = await _callManagementService.ExchangeOperatorRoles(exchangeRolesDto.LineId, GetUserId(), exchangeRolesDto.ToUserId);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Изменить состояние микрофона.
        /// </summary>
        /// <param name="microphoneChangeStateDto"></param>
        /// <returns></returns>
        [HttpPost("changeMicrophoneState")]
        public async Task<demoResult> ChangeMicrophoneState([FromBody] MicrophoneChangeStateDto microphoneChangeStateDto)
        {
            var result = await _callManagementService.MicrophoneChangeState(microphoneChangeStateDto.LineId, microphoneChangeStateDto.CallId, microphoneChangeStateDto.IsMuted);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Позвонить заявителю.
        /// </summary>
        [HttpPost("callBackToApplicant")]
        public async Task<demoResult> CallBackToApplicant([FromBody] CallBackToApplicantDto model)
        {
            if (model.CaseFolderId == default)
            {
                _logger.Warning("Id инцидента не задан");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _callManagementService.CallBackToApplicant(GetUserId(), model);
            if (result.IsFailure)
            {
                _logger.Warning($"Ошибка при перезвоне заявителю. {result.ErrorMessage}");
                return BadRequest(result.ErrorCode);
            }

            if (result.Value == CallBackToApplicantStatus.AlreadyInCall)
            {
                _logger.Warning("Ошибка при перезвоне заявителю. Заявитель уже участвует в звонке.");
                return BadRequest(ErrorCodes.CallBackToApplicantAlreadyInCall);
            }

            return Ok();
        }

        /// <summary>
        /// Позвонить на внешний номер.
        /// </summary>
        [HttpPost("callToNumber")]
        public async Task<demoResult> CallToNumber([FromBody] CallToNumberDto model)
        {
            if (model.Extension == default)
            {
                _logger.Warning("External number not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _callManagementService.CallToNumber(GetUserId(), model);
            if (result.IsFailure)
            {
                _logger.Warning($"Ошибка при звонке на внешний номер. {result.ErrorMessage}");
                return BadRequest(result.ErrorCode);
            }

            if (result.Value == CallBackToApplicantStatus.AlreadyInCall)
            {
                _logger.Warning("Ошибка при звонке на внешний номер. Вызываемый участник уже участвует в разговоре.");
                return BadRequest(ErrorCodes.CallBackToApplicantAlreadyInCall);
            }

            return Ok();
        }

        /// <summary>
        /// Получить список участников разговора.
        /// </summary>
        [HttpGet("getParticipantsList/{lineId}")]
        public async Task<demoResult<List<CallRecordInfoDto>>> GetParticipantsList(Guid lineId)
        {
            if (lineId == default)
            {
                _logger.Warning("LineId not set");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _callManagementService.GenerateCallRecordsList(lineId);
            if (result.IsFailure)
            {
                _logger.Warning($"CallsController. GetParticipantsList method. Error getting list of participants. {result.ErrorMessage}");
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }
    }
}
