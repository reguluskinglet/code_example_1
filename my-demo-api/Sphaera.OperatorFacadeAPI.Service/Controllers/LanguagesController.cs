using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Attributes;
using demo.DemoApi.Service.Dtos.Language;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Методы для работы с языками.
    /// </summary>
    [Route("api/[controller]"), ApiController]
    public class LanguagesController : demoControllerBase
    {
        private readonly LanguageService _languageService;
        private readonly ILogger _logger;

        /// <summary>
        /// Конструктор для инъекции зависимостей
        /// </summary>
        /// <param name="languageService"></param>
        /// <param name="logger"></param>
        public LanguagesController(LanguageService languageService, ILogger logger)
        {
            _languageService = languageService;
            _logger = logger;
        }

        /// <summary>
        /// Получение языка по коду
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getByCode/{code}")]
        [CheckETag]
        public async Task<demoResult<LanguageExtendedDto>> GetByCode(LanguageCode code)
        {
            var languageResult = await _languageService.GetByCode(code);
            
            if (languageResult.IsFailure)
            {
                return BadRequest(languageResult.ErrorCode);
            }

            return Answer(languageResult);
        }

        /// <summary>
        /// Получить все поддерживаемые языки
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<demoResult<List<LanguageDto>>> GetAll()
        {
            var languagesResult = await _languageService.GetAll();

           if (languagesResult.IsFailure)
           {
               _logger.Warning($"Error getting all languages. {languagesResult.ErrorMessage}");
                return BadRequest(languagesResult.ErrorCode);
           }

           return Answer(languagesResult);
        }

        /// <summary>
        /// Получение языковых настроек пользователя
        /// </summary>
        /// <returns></returns>
        [HttpGet("settingsForCurrentUser")]
        public async Task<demoResult<string>> GetLanguageSettingsForCurrentUser()
        {
            var userId = GetUserId();

            Result<string> languageResult = await _languageService.GetLanguageSettingsByUserId(userId);

            if (languageResult.IsFailure)
            {
                _logger.Warning($"Error getting language settings for current user. {languageResult.ErrorMessage}");
                return BadRequest(languageResult.ErrorCode);
            }

            return Answer(languageResult);
        }

        /// <summary>
        /// Сохранить текущий язык пользователя
        /// </summary>
        /// <returns></returns>
        [HttpPut("setCurrentLanguage")]
        public async Task<demoResult> SetCurrentLanguage([FromBody] SetUserLanguageDto userLanguageDto)
        {
            var userId = GetUserId();

            var languageResult = await _languageService.SetCurrentLanguage(userId, userLanguageDto.Code);

            if (languageResult.IsFailure)
            {
                _logger.Warning($"Error set current language for user. {languageResult.ErrorMessage}");
                return BadRequest(languageResult.ErrorCode);
            }

            return Ok();
        }
    }
}
