using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList
{
    /// <summary>
    /// Dto с данными об странице карточек событий
    /// </summary>
    public class CaseFolderPageDto
    {
        /// <summary>
        /// Список карточек событий
        /// </summary>
        public List<CaseFolderListItemDto> CaseFolders { get; set; }

        /// <summary>
        /// Предоставляет параметры пагинации
        /// </summary>
        public PageDto PageView { get; set; }
    }
}
