using System;

namespace demo.DemoApi.Service.Dtos.Language
{
    /// <summary>
    /// Dto с данными о языках
    /// </summary>
    public class LanguageExtendedDto : LanguageDto
    {
        /// <summary>
        /// Идентификатор языка
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Данные для перевода.
        /// </summary>
        public string Data { get; set; }
    }
}