using System;

namespace demo.DemoGateway.Client.Dto
{
    /// <summary>
    /// Маршрут звонка (Отправляется по Http из CTI сервиса в DemoGateway)
    /// </summary>
    public class RouteCallClientDto
    {
        /// <summary>
        /// CallId участника разговора, кому направлен вызов
        /// </summary>
        public Guid ToCallId { get; set; }

        /// <summary>
        /// CallId участника разговора, инициировавшего вызов или действие
        /// </summary>
        public Guid? FromCallId { get; set; }

        /// <summary>
        /// Номер участника разговора, кому направлен вызов
        /// </summary>
        public string ToExtension { get; set; }

        /// <summary>
        /// Участник разговора, инициировавший вызов или действие (например вызов ассистента или подключение в режиме конференции)
        /// </summary>
        public string FromExtension { get; set; }

        /// <summary>
        /// Идентификатор активной линии
        /// </summary>
        public Guid? LineId { get; set; }
    }
}