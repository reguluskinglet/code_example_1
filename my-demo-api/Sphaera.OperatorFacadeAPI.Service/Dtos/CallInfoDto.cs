using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Информация о звонке
    /// </summary>
    public class CallInfoDto
    {
        /// <summary>
        /// Id звонка
        /// </summary>
        public Guid CallId { get; set; }

        /// <summary>
        /// Участник разговора, который инициировал вызов
        /// </summary>
        public ParticipantInfoDto Caller { get; set; }
    }
}
