using System;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Service.Dtos.Line;

namespace demo.DemoApi.Service.Dtos.Calls
{
    /// <summary>
    /// Dto для сущности вызова
    /// </summary>
    public class CallDto
    {
        /// <summary>
        /// Уникальный идентификатор звонка
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Текущий статус звонка
        /// </summary>
        public CallStatus Status { get; set; }

        /// <summary>
        /// Изолирован ли данный вызов
        /// </summary>
        public bool Isolated { get; set; }

        /// <summary>
        /// Признак того, что у участника разговора отключен микрофон
        /// </summary>
        public bool IsMutedMicrophone { get; set; }

        /// <summary>
        /// Звонок внешнего участника разговора (например, заявителя или контакта)
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Отмененный заявителем
        /// </summary>
        public bool Canceled { get; set; }

        /// <summary>
        /// Участник разговора, который инициировал вызов
        /// </summary>
        public Guid? CallerId { get; set; }

        /// <summary>
        /// Участник разговора, которому принадлежит данный звонок
        /// </summary>
        public Guid? ParticipantId { get; set; }

        /// <summary>
        /// Линия вызова
        /// </summary>
        public LineDto Line { get; set; }

        /// <summary>
        /// Время поступления звонка
        /// </summary>
        public DateTime ArrivalDateTime { get; set; }

        /// <summary>
        /// Время ответа на звонок
        /// </summary>
        public DateTime? AcceptDateTime { get; set; }

        /// <summary>
        /// На удержании ли данный звонок
        /// </summary>
        public bool OnHold { get; set; }

        /// <summary>
        /// Режим подключения участника разговора
        /// </summary>
        public ConnectionMode ConnectionMode { get; set; }
    }
}
