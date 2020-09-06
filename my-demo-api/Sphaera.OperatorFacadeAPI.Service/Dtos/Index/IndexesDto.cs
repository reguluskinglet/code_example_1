using System;
using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Index
{
    /// <summary>
    /// Дерево индексов для определенного типа карточки события
    /// </summary>
    public class IndexesDto
    {
        /// <inheritdoc />
        public IndexesDto()
        {
            Indexes = new List<IndexesDto>();
        }

        /// <summary>
        /// Идентификатор индекса
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Код индекса
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Название индекса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дочерние индексы
        /// </summary>
        public List<IndexesDto> Indexes { get; set; }
    }
}
