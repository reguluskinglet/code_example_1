using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Dto, содержащее идентификаторы инцидента и оператора
    /// </summary>
    public class CaseOperatorDto
    {
        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Проверка на валидности модели
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseFolderId != default;
        }
    }
}
