using System;

namespace demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList
{
    /// <summary>
    /// Карточка события
    /// </summary>
    public class CaseListItemDto
    {
        /// <summary>
        /// Идентификатор карточки
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Идентификатор типа карточки
        /// </summary>
        public Guid TypeId { get; set; }

        /// <summary>
        /// Название типа карточки
        /// </summary>
        public string TypeTitle { get; set; }

        /// <summary>
        /// Индекс
        /// </summary>
        public string Index { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Заявитель
        /// </summary>
        public string Applicant { get; set; }

        /// <summary>
        /// Пострадавший
        /// </summary>
        public string Victim { get; set; }

        /// <summary>
        /// Создать DTO из сущности
        /// </summary>
        public static CaseListItemDto MapFromCaseEntity(Domain.Entities.Case entity, string address, string applicant, string victim)
        {
            if (entity == null)
            {
                return null;
            }

            var dto = new CaseListItemDto
            {
                Id  = entity.Id,
                CaseFolderId = entity.CaseFolder.Id,
                CreatedDate = entity.Created,
                Address = address,
                Applicant = applicant,
                Victim = victim,
                Index = entity.IndexValue,
                State = entity.Status,
                TypeTitle = entity.Type.GetTitle(),
                TypeId = entity.Type.Id
            };
            
            return dto;
        }
    }
}
