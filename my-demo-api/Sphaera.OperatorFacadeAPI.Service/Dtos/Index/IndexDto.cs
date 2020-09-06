using System;

namespace demo.DemoApi.Service.Dtos.Index
{
    /// <summary>
    /// Информация об одном индексе
    /// </summary>
    public class IndexDto
    {
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
    }
}