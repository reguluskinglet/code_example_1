using System;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.Service.Dtos.Language
{
    /// <summary>
    /// Dto для установки текущего языка пользователя
    /// </summary>
    public class SetUserLanguageDto
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Код языка
        /// </summary>
        public LanguageCode Code { get; set; }
    }
}
