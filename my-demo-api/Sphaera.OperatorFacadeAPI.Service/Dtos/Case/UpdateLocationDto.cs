using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Тип обновляемой координаты места
    /// </summary>
    public enum CoordinateFieldType
    {
        /// <summary>
        /// Неизвестный
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Широта
        /// </summary>
        Latitude = 1,

        /// <summary>
        /// Долгота
        /// </summary>
        Longitude = 2,
    }

    /// <summary>
    /// Координаты места происшествия
    /// </summary>
    public class UpdateLocationDto
    {
        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }
        
        /// <summary>
        /// Id пользователя.
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Обновляемая координата локации
        /// </summary>
        public CoordinateFieldType CoordinateFieldType { get; set; }

        /// <summary>
        /// Значение 
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Проверка на валидности модели
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseFolderId != default
                && CoordinateFieldType != CoordinateFieldType.Undefined;
        }
    }
}
