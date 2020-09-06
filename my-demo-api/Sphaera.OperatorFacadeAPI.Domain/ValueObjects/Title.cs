using System;

namespace demo.DemoApi.Domain.ValueObjects
{
    /// <summary>
    /// Заголовок карточки.
    /// </summary>
    public class Title
    {
        /// <summary>
        /// Id карточки.
        /// </summary>
        public Guid CaseId { get; internal set; }
        
        /// <summary>
        /// Текст заголовка.
        /// </summary>
        public string Text { get; internal set; }
    }
}
