using System;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Dtos.Line
{
    /// <summary>
    /// Dto линии вызова.
    /// </summary>
    public class LineByCaseFolderDto
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
        /// Id инициатора разговора, который инициировал создание линии
        /// </summary>
        public Guid CallerId { get; set; }
    }
}
