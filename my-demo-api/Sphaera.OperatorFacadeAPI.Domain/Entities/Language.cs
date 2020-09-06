using System;
using demo.DDD;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Сущность языка.
    /// </summary>
    public class Language : AggregateRoot
    {
        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </remarks>
        public Language()
        {
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="code"></param>
        public Language(LanguageCode code) : base(Guid.NewGuid())
        {
            Code = code;
        }
        /// <summary>
        /// Код языка.
        /// </summary>
        public LanguageCode Code { get; set; }

        /// <summary>
        /// Название языка.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Данные для перевода.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Направление отображения интерфейса.
        /// </summary>
        public DisplayDirectionType DisplayDirection { get; set; }
    }
}
