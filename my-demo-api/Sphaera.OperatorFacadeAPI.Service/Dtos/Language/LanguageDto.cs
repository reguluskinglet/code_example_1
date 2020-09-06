using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.Service.Dtos.Language
{
    /// <summary>
    /// Dto для формирования списка языков
    /// </summary>
    public class LanguageDto
    {
        /// <summary>
        /// Код языка.
        /// </summary>
        public LanguageCode Code { get; set; }

        /// <summary>
        /// Название языка.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Направление отображения интерфейса.
        /// </summary>
        public DisplayDirectionType DisplayDirection { get; set; }
    }
}