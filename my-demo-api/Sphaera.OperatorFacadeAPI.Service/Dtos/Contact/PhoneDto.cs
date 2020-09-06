using System;

namespace demo.DemoApi.Service.Dtos.Contact
{
    /// <summary>
    /// Dto с данными об телефоне контакта
    /// </summary>
    public class PhoneDto
    {
        /// <summary>
        /// Идентификатор телефона
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор контакта
        /// </summary>
        public Guid ContactId { get; set; }

        /// <summary>
        /// Номер телефона
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Название способа связи
        /// </summary>
        public string ContactRouteName { get; set; }
    }
}
