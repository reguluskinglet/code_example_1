using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для обмена ролями между операторами
    /// </summary>
    public class ExchangeRolesDto
    {
        /// <summary>
        /// Идентификатор линии
        /// </summary>
        public Guid LineId { get; set; }

        /// <summary>
        /// Id ассистента
        /// </summary>
        public Guid ToUserId { get; set; }
    }

    /// <summary>
    /// Dto для изменения статуса микрофона
    /// </summary>
    public class MicrophoneChangeStateDto
    {
        /// <summary>
        /// Идентификатор линии
        /// </summary>
        public Guid LineId { get; set; }

        /// <summary>
        /// Идентификатор звонка
        /// </summary>
        public Guid CallId { get; set; }

        /// <summary>
        /// Выключен ли микрофон
        /// </summary>
        public bool IsMuted { get; set; }
    }
}