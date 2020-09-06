using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для добавления звонка в линию
    /// </summary>
    public class ConnectionToLineDto
    {
        /// <summary>
        /// Идентификатор линии
        /// </summary>
        public Guid LineId { get; set; }

        /// <summary>
        /// Идентификатор инцидента
        /// </summary>
        public Guid? CaseFolderId { get; set; }

        /// <summary>
        /// Куда будет направлен вызов
        /// </summary>
        public string Destination { get; set; }
    }
}
