using System;

namespace demo.DemoApi.Service.Dtos.Receiver
{
    /// <summary>
    /// Dto получателя
    /// </summary>
    public class ReceiverDto
    {
        /// <summary>
        /// Идентификатор получателя
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Название получателя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// То, куда приходит сигнал (B Number for Call)
        /// </summary>
        public string Destination { get; set; }
    }
}
