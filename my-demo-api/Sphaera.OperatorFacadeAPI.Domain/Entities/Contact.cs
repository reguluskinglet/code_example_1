namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Сущность контакта адресной книги.
    /// </summary>
    public class Contact : Participant
    {
        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </remarks>
        public Contact()
        {
        }

        /// <inheritdoc />
        public Contact(string extension, string name, string organization, string position, string contactRouteName)
        {
            Extension = extension;
            Name = name;
            Organization = organization;
            Position = position;
            ContactRouteName = contactRouteName;
        }

        /// <summary>
        /// т.к У Contact нету имени, а у пользователя есть, то в базовом классе присутствует данный метод 
        /// </summary>
        public override string ParticipantName
        {
            get => Name;
            set => Name = value;
        }

        /// <summary>
        /// Телефонный номер из адресной книги
        /// </summary>
        public virtual string Extension { get; set; }

        /// <inheritdoc />
        public override string ParticipantExtension
        {
            get => Extension;
            set => Extension = value;
        }

        /// <summary>
        /// ФИО контакта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Должность контакта
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Организация, к которому относится контакт
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Название способа связи
        /// </summary>
        public string ContactRouteName { get; set; }
    }
}