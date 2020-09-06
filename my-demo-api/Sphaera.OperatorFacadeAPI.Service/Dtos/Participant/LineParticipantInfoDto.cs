using System;
using System.Collections.Generic;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Dtos.Participant
{
    /// <summary>
    /// Dto для отображения участников в линии.
    /// </summary>
    public class LineParticipantInfoDto
    {
        /// <summary>
        /// Id участника.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id звонка.
        /// </summary>
        public Guid CallId { get; set; }

        /// <summary>
        /// Extension участника.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Информация об участнике.
        /// </summary>
        public string ParticipantInfo { get; set; }

        /// <summary>
        /// Статусы вызова.
        /// </summary>
        public List<CallStatusDto> CallStates { get; set; }

        /// <summary>
        /// Тип подключения.
        /// </summary>
        public string ConnectionMode { get; set; }

        /// <summary>
        /// Может ли участник быть изолирован.
        /// </summary>
        public bool CanChangeIsolationStatus { get; set; }
    }
}
