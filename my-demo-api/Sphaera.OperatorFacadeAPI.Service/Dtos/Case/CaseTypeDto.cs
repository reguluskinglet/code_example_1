using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Dto шаблона карточки
    /// </summary>
    public class CaseTypeDto
    {
        /// <summary>
        /// Id шаблона
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Заголовок шаблона
        /// </summary>
        public string Title { get; set; }
    }
}