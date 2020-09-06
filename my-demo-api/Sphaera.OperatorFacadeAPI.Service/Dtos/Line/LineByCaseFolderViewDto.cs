using System;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Dtos.Line
{
    /// <summary>
    /// Dto линии вызова.
    /// </summary>
    public class LineByCaseFolderViewDto
    {
        /// <summary>
        /// Идентификатор линии.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор инцидента.
        /// </summary>
        public Guid CaseFolderId { get; set; }

        /// <summary>
        /// Тип звонка, который инициировал создание линии
        /// </summary>
        public CallType FirstCallType { get; set; }

        /// <summary>
        /// Дата создания линии вызова.
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Информация о инициаторе вызова
        /// </summary>
        public ParticipantInfoDto CallInitiator { get; set; }

        /// <summary>
        /// Является ли инициатор разговора пользователем Emercore
        /// </summary>
        public bool IsEmercoreUser { get; set; }
    }
}
