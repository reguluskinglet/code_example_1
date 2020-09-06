using System;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Модель данных поля карточки
    /// </summary>
    public class CaseData
    {
        /// <summary>
        /// Id блока карточки
        /// </summary>
        public Guid BlockId { get; set; }
        
        /// <summary>
        /// Id поля карточки
        /// </summary>
        public Guid FieldId { get; set; }
        
        /// <summary>
        /// Значение пол якарточки 
        /// </summary>
        public string Value { get; set; }
    }
}
