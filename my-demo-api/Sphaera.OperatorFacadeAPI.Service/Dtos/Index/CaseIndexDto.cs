using System;

namespace demo.DemoApi.Service.Dtos.Index
{
    /// <summary>
    /// Данные для сохранения или обновления индекса карточки инцидента.
    /// </summary>
    public class CaseIndexDto
    {
        /// <inheritdoc />
        public CaseIndexDto(Guid caseId, Guid indexId)
        {
            CaseId = caseId;
            IndexId = indexId;
        }

        /// <summary>
        /// Идентификатор карточки инцидента
        /// </summary>
        public Guid CaseId { get; set; }

        /// <summary>
        /// Идентификатор индекса
        /// </summary>
        public Guid IndexId { get; set; }

        /// <summary>
        /// Проверка валидности модели
        /// </summary>
        public bool IsValid()
        {
            return CaseId != default
                && IndexId != default;
        }
    }
}
