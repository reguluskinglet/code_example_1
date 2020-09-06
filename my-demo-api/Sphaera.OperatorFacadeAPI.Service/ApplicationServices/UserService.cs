using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using demo.CallManagement.Client;
using demo.CallManagement.HttpContracts.Dto;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Authorization;
using demo.DemoApi.Service.Dtos.Calls;
using demo.UserManagement.Client;
using demo.UserManagement.HttpContracts.Dto;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис пользователей
    /// </summary>
    public class UserService
    {
        private readonly ILogger _logger;
        private readonly CallManagementServiceClient _callManagementServiceClient;
        private readonly UserManagementServiceClient _userManagementServiceClient;
        private readonly IMapper _mapper;

        /// <summary>
        /// Создать
        /// </summary>
        public UserService(IMapper mapper, ILogger logger, CallManagementServiceClient callManagementService, UserManagementServiceClient userManagementServiceClient)
        {
            _mapper = mapper;
            _logger = logger;
            _callManagementServiceClient = callManagementService;
            _userManagementServiceClient = userManagementServiceClient;
        }

        /// <summary>
        /// Получить пользователя по Id
        /// </summary>
        public async Task<Result<UserDto>> GetUserById(Guid userId)
        {
            Result<UserClientDto> result = await _userManagementServiceClient.GetUserById(userId);
            if (result.IsFailure)
            {
                _logger.Warning($"GetUserById. Error get user by Id. {result.ErrorMessage}");
                return Result.Failure<UserDto>(ErrorCodes.UserNotFound);
            }

            var dtoModel = _mapper.Map<UserDto>(result.Value);

            Result<List<CallClientDto>> callsResult = await _callManagementServiceClient.GetActualUserCalls();
            if (callsResult.IsFailure)
            {
                _logger.Warning($"GetUserInfo. Error getting information about actual user calls. UserId: {userId}. {callsResult.ErrorMessage}");
            }
            else
            {
                var activeCall = _mapper.Map<CallDto>(callsResult.Value?.FirstOrDefault());
                if (activeCall != null)
                {
                    dtoModel.SetCurrentCallStates(activeCall);
                }
            }

            return Result.Success(dtoModel);
        }

        /// <summary>
        /// Получить всех залогиненных пользователей
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<ActiveUserDto>>> GetActive()
        {
            Result<List<UserClientDto>> result = await _userManagementServiceClient.GetActive();
            if (result.IsFailure)
            {
                _logger.Warning($"GetActive. Active users not found. {result.ErrorMessage}");
                return Result.Failure<List<ActiveUserDto>>(ErrorCodes.UserNotFound);
            }

            var activeUsers = _mapper.Map<List<UserDto>>(result.Value);
            var resultUsers = new List<ActiveUserDto>();

            foreach (var userClientDto in activeUsers)
            {
                List<CallClientDto> actualUserCalls = await GetActualUserCalls(userClientDto.Id);
                var activeUserInfo = ActiveUserDto.MapFromQueueEntity(userClientDto, actualUserCalls);
                resultUsers.Add(activeUserInfo);
            }

            return Result.Success(resultUsers);
        }

        /// <summary>
        /// Изменить статус активности пользователя
        /// </summary>
        public async Task ChangeActivityStatus(Guid userId, bool isActive)
        {
            _logger.Verbose($"ChangeActivityStatus. Begin. User with Id: {userId} status changed to IsActive:`{isActive}`.");
            
            Result result = await _userManagementServiceClient.ChangeActivityStatus(new ChangeActivityStatusClientDto { UserId = userId, IsActive = isActive });
            if (result.IsFailure)
            {
                _logger.Error($"ChangeActivityStatus. User with Id: {userId} not found.");
                return;
            }

            _logger.Verbose($"ChangeActivityStatus. End. User with Id: {userId} status changed to IsActive:`{isActive}`.");
        }

        /// <summary>
        /// Взять настройки пользователя по его id
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Настройки пользователя</returns>
        public async Task<Result<string>> GetSettings(Guid userId)
        {
            Result<string> result = await _userManagementServiceClient.GetWindowSettings(userId);
            if (result.IsFailure)
            {
                _logger.Warning($"GetSettings. Error get settings. UserId: {userId}. {result.ErrorMessage}");
                return Result.Failure<string>(ErrorCodes.UserNotFound);
            }

            return Result.Success(result.Value);
        }

        /// <summary>
        /// Задать настройки пользователя используя Id
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="preferences">Настройки пользователя</param>
        /// <returns>Успешность операции</returns>
        public async Task<Result> SetSettings(Guid userId, string preferences)
        {
            Result result = await _userManagementServiceClient.SetWindowSettings(new SetUserWindowClientDto { UserId = userId, WindowSettings = preferences });
            if (result.IsFailure)
            {
                _logger.Warning($"GetSettings. Error set settings. UserId: {userId}. {result.ErrorMessage}");
                return Result.Failure(ErrorCodes.UserNotFound);
            }

            return result;
        }

        /// <summary>
        /// Разлогинивание пользователя
        /// </summary>
        public async Task<Result> Logout(Guid userId)
        {
            Result result = await _userManagementServiceClient.Logout(userId);
            if (result.IsFailure)
            {
                _logger.Warning($"Logout. Error logout of user. {result.ErrorMessage}");
                return Result.Failure(ErrorCodes.UserNotFound);
            }

            return result;
        }

        /// <summary>
        /// Получить токен авторизации
        /// </summary>
        /// <returns></returns>
        public async Task<Result<string>> GetToken(LoginDto model)
        {
            Result<string> tokenResult = await _userManagementServiceClient.GetToken(new LoginModelClientDto
            {
               Login = model.Login,
               Password = model.Password
            });

            if (tokenResult.IsFailure)
            {
                _logger.Warning($"GetToken. Error getting authorization token. Login: {model.Login}. {tokenResult.ErrorMessage}");
                return Result.Failure<string>(ErrorCodes.UnableToGetToken);
            }

            return Result.Success(tokenResult.Value);
        }

        private async Task<List<CallClientDto>> GetActualUserCalls(Guid userId)
        {
            var actualUserCalls = new List<CallClientDto>();
            Result<List<CallClientDto>> actualUserCallsResult = await _callManagementServiceClient.GetActualUserCalls(userId);
            if (actualUserCallsResult.IsFailure)
            {
                _logger.Warning($"UserService.GetActualUserCalls. UserId: {userId}. {actualUserCallsResult.ErrorMessage}");
            }
            else
            {
                actualUserCalls = actualUserCallsResult.Value;
            }

            return actualUserCalls;
        }
    }
}
