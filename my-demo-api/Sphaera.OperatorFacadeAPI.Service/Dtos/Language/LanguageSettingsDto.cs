using Newtonsoft.Json;

namespace demo.DemoApi.Service.Dtos.Language
{
    /// <summary>
    /// Dto для установки языковых настроек
    /// </summary>
    public class LanguageSettingsDto
    {
        /// <summary>
        /// Язык по умолчанию
        /// </summary>
        [JsonProperty(PropertyName = "defaultLanguage")]
        public string DefaultLanguage { get; set; }

        /// <summary>
        /// Текущий язык
        /// </summary>
        [JsonProperty(PropertyName = "currentLanguage")]
        public string CurrentLanguage { get; set; }
    }
}
