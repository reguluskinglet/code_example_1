using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Dto для закрытия карточки
    /// </summary>
    public class CloseCaseCardDto
    {
        /// <summary>
        /// Идентификатор Case
        /// </summary>
        public Guid CaseCardId { get; set; }
        
        /// <summary>
        /// Проверка модели на валидность
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseCardId != default;
        }
    }
}