using System;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Сущность пользователя.
    /// </summary>
    public class User : Participant
    {
        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </remarks>
        public User()
        {
        }

        /// <inheritdoc />
        public User(Guid id, string extension, string userName)
        {
            Id = id;
            Extension = extension;
            UserName = userName ?? extension;
        }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Extension участника звонка
        /// </summary>
        public virtual string Extension { get; set; }

        /// <inheritdoc />
        public override string ParticipantName
        {
            get => UserName;
            set => UserName = value;
        }

        /// <inheritdoc />
        public override string ParticipantExtension
        {
            get => Extension;
            set => Extension = value;
        }
    }
}