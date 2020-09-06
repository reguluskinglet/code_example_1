using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos.Language;
using demo.UserManagement.Client;
using demo.UserManagement.HttpContracts.Dto;
using demo.UserManagement.HttpContracts.Enums;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис языков.
    /// </summary>
    public class LanguageService
    {
        private readonly ILanguageRepository _languageRepository;
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly UserManagementServiceClient _userManagementServiceClient;

        /// <summary>
        /// Конструктор для инъекции зависимостей.
        /// </summary>
        public LanguageService(
            ILanguageRepository languageRepository,
            UnitOfWork unitOfWork,
            IMapper mapper, ILogger logger,
            UserManagementServiceClient userManagementServiceClient)
        {
            _unitOfWork = unitOfWork;
            _languageRepository = languageRepository;
            _mapper = mapper;
            _logger = logger;
            _userManagementServiceClient = userManagementServiceClient;
        }

        /// <summary>
        /// Получить язык по коду.
        /// </summary>
        public async Task<Result<LanguageExtendedDto>> GetByCode(LanguageCode code)
        {
            using (_unitOfWork.Begin())
            {
                var language = await _languageRepository.GetByCode(code);
                if (language == null)
                {
                    _logger.Warning("Language not found.");
                    return Result.Failure<LanguageExtendedDto>(ErrorCodes.LanguageNotFound);
                }

                var languageExtendedDto = _mapper.Map<LanguageExtendedDto>(language);
                return Result.Success(languageExtendedDto);
            }
        }

        /// <summary>
        /// Получить все поддерживаемые языки
        /// </summary>
        public async Task<Result<IList<LanguageDto>>> GetAll()
        {
            using (_unitOfWork.Begin())
            {
                IList<Language> languages = await _languageRepository.GetAll();
                if (languages == null || languages.Count == 0)
                {
                    _logger.Warning("LanguageService. Languages has not been found");
                    return Result.Failure<IList<LanguageDto>>(ErrorCodes.LanguagesNotFound);
                }

                var languageDtos = _mapper.Map<IList<LanguageDto>>(languages);
                return Result.Success(languageDtos);
            }
        }

        /// <summary>
        /// Получить языковые настройки текущего пользователя
        /// </summary>
        public async Task<Result<string>> GetLanguageSettingsByUserId(Guid userId)
        {
            if (userId == default)
            {
                _logger.Warning("LanguageService. UserId not set");
                return Result.Failure<string>(ErrorCodes.ValidationError);
            }

            Result<string> result = await _userManagementServiceClient.GetLanguageSettings(userId);
            if (result.IsFailure)
            {
                _logger.Warning($"LanguageService. Language settings has not been found by userId: {userId}");
                return Result.Failure(ErrorCodes.LanguagesNotFound);
            }

            return Result.Success(result.Value);
        }

        /// <summary>
        /// Установить текущий язык пользователя
        /// </summary>
        public async Task<Result> SetCurrentLanguage(Guid userId, LanguageCode code)
        {
            if (userId == default)
            {
                _logger.Warning("LanguageService. UserId not set");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            Result<string> result = await _userManagementServiceClient.SetCurrentLanguage(new SetUserLanguageClientDto{UserId = userId, Code = (LanguageCodeClient)code});
            if (result.IsFailure)
            {
                _logger.Warning($"LanguageService. Language settings has not been changed by userId: {userId}");
                return Result.Failure(ErrorCodes.LanguagesNotFound);
            }

            _logger.Information($"LanguageService. Language settings changed for user with userId: {userId}");
            return Result.Success();
        }
    }
}