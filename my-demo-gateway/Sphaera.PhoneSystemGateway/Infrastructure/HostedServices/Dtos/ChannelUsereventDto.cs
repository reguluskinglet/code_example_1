namespace demo.DemoGateway.Infrastructure.HostedServices.Dtos
{
    /// <summary>
    /// Данные, которые содержатся в сообщении, которое пришло с Астериска
    /// </summary>
    public class ChannelUserEventDto
    {
        /// <summary>
        /// Заявитель/номер заявителя
        /// </summary>
        public string SenderExtension { get; set; }

        /// <summary>
        /// Тело сообщения
        /// </summary>
        public string Body { get; set; }
    }
}