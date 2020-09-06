using System;
using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Contact
{
    /// <summary>
    /// Dto с данными о контакте из адресной книги
    /// </summary>
    public class ContactDto
    {
        /// <summary>
        /// Идентификатор контакта
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ФИО контакта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Должность контакта
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Организация, к которой относится контакт
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Список телефонов для контакта
        /// </summary>
        public List<PhoneDto> Phones { get; set; }
    }
}