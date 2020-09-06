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
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Authorization;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Пользователи
    /// </summary>
    [Authorize]
    [Route("api/[controller]"), ApiController]
    public class UsersController : demoControllerBase
    {
        private readonly ILogger _logger;
        private readonly UserService _userService;

        /// <summary>
        /// Создать
        /// </summary>
        public UsersController(ILogger logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Авторизироваться в системе и получить токен
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<demoResult<string>> Login([FromBody] LoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Login))
            {
                _logger.Warning($"User with login {model.Login} not found.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                _logger.Warning($"User with password {model.Password} not found.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<string> tokenResult = await _userService.GetToken(model);
            if (tokenResult.IsFailure)
            {
                _logger.Warning($"Error on UsersController Login. {tokenResult.ErrorMessage}");
                return BadRequest(tokenResult.ErrorCode);
            }

            return Answer(tokenResult);
        }

        /// <summary>
        /// Получить текущего пользователя
        /// </summary>
        [HttpGet("current")]
        public async Task<demoResult<UserDto>> GetCurrentUser()
        {
            var userId = GetUserId();

            Result<UserDto> userResult = await _userService.GetUserById(userId);
            if (userResult.IsFailure)
            {
                return BadRequest(userResult.ErrorCode);
            }

            return Answer(userResult);
        }

        /// <summary>
        /// Получить пользователя по Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<demoResult<UserDto>> Get(Guid id)
        {
            if (id == default)
            {
                _logger.Warning("UserId is invalid.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            Result<UserDto> userResult = await _userService.GetUserById(id);
            if (userResult.IsFailure)
            {
                return BadRequest(userResult.ErrorCode);
            }

            return Answer(userResult);
        }

        /// <summary>
        /// Получить настройки окон пользователя
        /// </summary>
        [HttpGet]
        [Route("preferences")]
        public async Task<demoResult<string>> GetSettings()
        {
            var userId = GetUserId();

            _logger.WithTag("operatorId", userId).Information("Get settings.");
            Result<string> result = await _userService.GetSettings(userId);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Answer(result);
        }

        /// <summary>
        /// Задать настройки окна пользователя
        /// </summary>
        [HttpPut]
        [Route("preferences")]
        public async Task<demoResult> SetSettings([FromBody] SetUserWindowDto userWindowDto)
        {
            _logger.WithTag($"{nameof(userWindowDto.UserId)}", userWindowDto.UserId).Information($"Set up of user settings {userWindowDto.WindowSettings}");
            var result = await _userService.SetSettings(userWindowDto.UserId, userWindowDto.WindowSettings);

            if (result.IsFailure)
            {
                return BadRequest(result.ErrorCode);
            }

            return Ok();
        }

        /// <summary>
        /// Выход текущего пользователя из системы
        /// </summary>
        [HttpPost]
        [Route("logout")]
        public async Task<demoResult> Logout()
        {
            var userId = GetUserId();

            Result result = await _userService.Logout(userId);
            if (result.IsFailure)
            {
                _logger.Warning($"User with Id {userId} not found.");
                return BadRequest(ErrorCodes.UserNotFound);
            }

            return Answer(result);
        }

        /// <summary>
        /// Получить всех активных пользователей
        /// </summary>
        [HttpGet]
        [Route("active")]
        public async Task<demoResult<IEnumerable<ActiveUserDto>>> GetActive()
        {
            _logger.Debug("Getting a list of active users from UsersController");

            Result<List<ActiveUserDto>> result = await _userService.GetActive();
            if (result.IsFailure)
            {
                _logger.Warning("Active users not found.");
                return BadRequest(ErrorCodes.UserNotFound);
            }

            return Answer(result);
        }
    }
}
