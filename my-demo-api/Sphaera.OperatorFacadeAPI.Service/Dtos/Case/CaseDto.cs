using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Карточка события
    /// </summary>
    public class CaseDto
    {
        /// <summary>
        /// Идентификатор Case
        /// </summary>
        public Guid CaseId { get; set; }

        /// <summary>
        /// Идентификатор инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Template в виде JSON
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Данные для полей карточки в виде JSON
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Стили для шаблона
        /// </summary>
        public string Css { get; set; }

        /// <summary>
        /// Данные активированных полей плана в виде JSON
        /// </summary>
        public string ActivatedPlanInstructions { get; set; }

        /// <summary>
        /// Данные о широте
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Данные о долготе
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Идентификатор типа карточки инцидента
        /// </summary>
        public Guid? CaseTypeId { get; set; }

        /// <summary>
        /// Создать DTO из сущности
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static CaseDto MapFromCaseEntity(Domain.Entities.Case entity)
        {
            if (entity == null)
            {
                return null;
            }

            var dto = new CaseDto
            {
                CaseId = entity.Id,
                CaseFolderId = entity.CaseFolder.Id,
                Template = entity.Type?.Data,
                Data = entity.CaseFolder.Data,
                Css = entity.Type?.Css,
                ActivatedPlanInstructions = entity.ActivatedPlanInstructions,
                Latitude = entity.CaseFolder.Latitude,
                Longitude = entity.CaseFolder.Longitude,
                CaseTypeId = entity.Type?.Id
            };

            return dto;
        }
    }
}
