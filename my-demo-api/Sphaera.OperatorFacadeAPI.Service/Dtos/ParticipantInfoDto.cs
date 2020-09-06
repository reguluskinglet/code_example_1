using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Подробная информация об участнике разговора
    /// </summary>
    public class ParticipantInfoDto
    {
        /// <summary>
        /// Идентификатор участника
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ФИО участника разговора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Должность участника разговора
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Организация, к которой принадлежит участник разговора
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Номер телефона участника разговора
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Суммарная информация об участнике разговора
        /// </summary>
        public string ParticipantInfo { get; set; }

        /// <summary>
        /// Время окончания разговора
        /// </summary>
        public string EndCallTime { get; set; }

        /// <summary>
        /// Время начала разговора
        /// </summary>
        public string StartCallTime { get; set; }
    }
}
