using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Заголовки карточек
    /// </summary>
    public class TitlesDto
    {
        /// <summary>
        /// Коллекция заголовков
        /// </summary>
        public IEnumerable<CaseTitleDto> Titles { get; set; } 
    }
}