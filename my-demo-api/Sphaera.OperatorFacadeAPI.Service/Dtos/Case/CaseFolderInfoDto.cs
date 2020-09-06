using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Dto для запросов на получения данных карточек событий
    /// </summary>
    public class CaseFolderInfoDto
    {
        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Id оператора
        /// </summary>
        public Guid OperatorId { get; set; }
    }
}