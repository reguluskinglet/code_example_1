using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Информация об записи участника разговора
    /// </summary>
    public class CallRecordInfoDto
    {
        /// <summary>
        /// Идентификатор записи разговора
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Подробная информация об участнике разговора
        /// </summary>
        public ParticipantInfoDto ParticipantInfo { get; set; }

        /// <summary>
        /// Идентификатор звонка
        /// </summary>
        public Guid CallId { get; set; }

        /// <summary>
        /// Является ли участник разговора пользователем Emercore
        /// </summary>
        public bool IsEmercoreUser { get; set; }

        /// <summary>
        /// Флаг, указывающий на полную запись разговора
        /// </summary>
        public bool IsFullRecord { get; set; }
    }
}
