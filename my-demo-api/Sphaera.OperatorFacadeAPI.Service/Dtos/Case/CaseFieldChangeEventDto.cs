using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Модель событий при обновлении поля карточки
    /// </summary>
    public class CaseFieldChangeEventDto
    {
        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Id поля карточки
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        public Guid OperatorId { get; set; }
    }
}
