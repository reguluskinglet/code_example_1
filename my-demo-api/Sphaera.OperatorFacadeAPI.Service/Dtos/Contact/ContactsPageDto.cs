using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Contact
{
    /// <summary>
    /// Dto с данными об странице контактов
    /// </summary>
    public class ContactsPageDto
    {
        /// <summary>
        /// Список контактов
        /// </summary>
        public IEnumerable<ContactDto> Contacts { get; set; }

        /// <summary>
        /// Предоставляет параметры пагинации
        /// </summary>
        public PageViewModel PageViewModel { get; set; }
    }
}