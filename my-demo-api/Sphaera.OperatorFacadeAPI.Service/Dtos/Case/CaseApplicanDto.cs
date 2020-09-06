using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Координаты местоположения заявителя.
    /// </summary>
    public class CaseApplicantDto
    {
        /// <summary>
        /// Id инцидента.
        /// </summary>
        public Guid CaseFolderId { get; set; }
        
        /// <summary>
        /// Id карточки инцидента.
        /// </summary>
        public Guid CaseId { get; set; }
        
       
        /// <summary>
        /// Проверка на валидности модели
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseFolderId != default && CaseId != default;
        }
    }
}
