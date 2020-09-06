using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Данные поля карточки
    /// </summary>
    public class CaseFieldDto
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
        ///Значение поля карточки 
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Проверка на валидности модели
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseFolderId != default
                && FieldId != default
                && Value != null;
        }
    }
}
