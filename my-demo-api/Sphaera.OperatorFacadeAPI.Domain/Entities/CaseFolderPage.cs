using System.Collections.Generic;
using demo.DDD;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Страница инцидентов
    /// </summary>
    public class CaseFolderPage : AggregateRoot
    {
        /// <summary>
        /// Список инцидентов
        /// </summary>
        public IList<CaseFolder> CaseFolders { get; set; }

        /// <summary>
        /// Общее количество инцидентов
        /// </summary>
        public int TotalCount { get; set; }
    }
}