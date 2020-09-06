using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Dto для заголовка карточки
    /// </summary>
    public class CaseTitleDto
    {
        /// <summary>
        /// Id карточки
        /// </summary>
        public Guid CaseId { get; set; }

        /// <summary>
        /// Текст заголовка
        /// </summary>
        public string Text { get; set; }
    }
}