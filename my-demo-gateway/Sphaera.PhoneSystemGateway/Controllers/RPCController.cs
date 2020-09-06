using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;

namespace demo.DemoGateway.Controllers
{
    /// <summary>
    /// Удаленный вызов процедур.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RPCController : demoControllerBase
    {
        private readonly AsteriskAriApiService _asteriskAriApiService;
        private readonly ILogger _logger;

        /// <summary>
        /// Создать
        /// </summary>
        /// <param name="asteriskAriApiService"></param>
        public RPCController(AsteriskAriApiService asteriskAriApiService, ILogger logger)
        {
            _asteriskAriApiService = asteriskAriApiService;
            _logger = logger;
        }

        /// <summary>
        /// Ответить на входящий ожидающий вызов
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("acceptIncomingCall")]
        public async Task<demoResult> AcceptIncomingCall(RouteCallDto model)
        {
            var errorMessage = ValidateBaseParams(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.AcceptIncomingCall(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Добавить пользователя в разговор в режиме конференции
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("addToConference")]
        public async Task<demoResult> AddToConference(RouteCallDto model)
        {
            var errorMessage = ValidateBaseParams(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.AddToConference(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Добавить пользователя в режиме ассистирования
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("addAssistant")]
        public async Task<demoResult> AddAssistant(RouteCallDto model)
        {
            var errorMessage = ValidateBaseParams(model);
            if (string.IsNullOrEmpty(errorMessage) && !model.FromCallId.HasValue)
            {
                errorMessage = "FromCallId not set.";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.AddAssistant(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Добавить пользователя в режиме частичного ассистирования
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("addPartialAssistant")]
        public async Task<demoResult> AddPartialAssistant(RouteCallDto model)
        {
            var errorMessage = ValidateBaseParams(model);
            if (string.IsNullOrEmpty(errorMessage) && !model.FromCallId.HasValue)
            {
                errorMessage = "FromCallId not set.";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.AddPartialAssistant(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Изолировать звонок
        /// </summary>
        /// <param name="dto">Dto для изоляции звонка</param>
        [HttpPost("isolate")]
        public async Task<demoResult> SetIsolationStatus(IsolationStatusDto dto)
        {
            if (dto.CallId == Guid.Empty)
            {
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.SetIsolationStatus(dto);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Включить/Отключить звук с микрофона
        /// </summary>
        /// <param name="dto">Dto для отключения звука</param>
        [HttpPost("mute")]
        public async Task<demoResult> SetMuteStatus(MuteStatusDto dto)
        {
            if (dto.CallId == Guid.Empty)
            {
                return BadRequest(ErrorCodes.ValidationError);
            }

            var muteStatusResult = await _asteriskAriApiService.SetMuteStatus(dto);
            if (muteStatusResult.IsFailure)
            {
                return Answer(muteStatusResult);
            }

            return Ok();
        }
        /// <summary>
        /// Смена ролей между двумя пользователями
        /// </summary>
        /// <param name="model"></param>
        [HttpPost("exchangeRoles")]
        public async Task<demoResult> ExchangeRoles(RouteCallDto model)
        {
            string errorMessage = null;
            if (!model.LineId.HasValue)
            {
                errorMessage = "LineId not set.";
            }
            else if (!model.FromCallId.HasValue)
            {
                errorMessage = "FromCallId not set.";
            }
            else if (model.ToCallId == default)
            {
                errorMessage = "ToCallId not set.";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.ExchangeRoles(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        /// <summary>
        /// Принудительное удаление канала
        /// </summary>
        [HttpPost("forceHangUp")]
        public async Task<demoResult> ForceHangUp(RouteCallDto model)
        {
            if (model.ToCallId == Guid.Empty)
            {
                return BadRequest(ErrorCodes.ValidationError);
            }

            await _asteriskAriApiService.ForceHangUp(model);
            return Ok();
        }

        /// <summary>
        /// Осуществить вызов от пользователя на указанный номер
        /// </summary>
        [HttpPost("callFromUser")]
        public async Task<demoResult> CallFromUser(RouteCallDto model)
        {
            var errorMessage = ValidateBaseParams(model);
            if (string.IsNullOrEmpty(errorMessage) && string.IsNullOrEmpty(model.FromExtension))
            {
                errorMessage = "FromExtension not set.";
            }
            else if (!model.FromCallId.HasValue)
            {
                errorMessage = "FromCallId not set.";
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                _logger.Warning(errorMessage);
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _asteriskAriApiService.CallFromUser(model);
            if (result.IsFailure)
            {
                return Answer(result);
            }

            return Ok();
        }

        private static string ValidateBaseParams(RouteCallDto model)
        {
            string errorMessage = null;

            if (!model.LineId.HasValue)
            {
                errorMessage = "LineId not set.";
            }
            else if (model.ToCallId == Guid.Empty)
            {
                errorMessage = "ToCallId not set.";
            }
            else if (string.IsNullOrEmpty(model.ToExtension))
            {
                errorMessage = "ToExtension not set.";
            }

            return errorMessage;
        }
    }
}