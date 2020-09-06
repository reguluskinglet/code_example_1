using System;

namespace demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList
{
    /// <summary>
    /// Данные для полей карточки 
    /// </summary>
    public class CaseFolderDataDto
    {
        /// <summary>
        /// Идентификатор блока
        /// </summary>
        public Guid BlockId { get; set; }

        /// <summary>
        /// Идентификатор поля
        /// </summary>
        public Guid FieldId { get; set; }

        /// <summary>
        /// Значение поля
        /// </summary>
        public string Value { get; set; }
    }
}
