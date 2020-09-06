using System;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Заявитель. Тот, кто звонит.
    /// </summary>
    public class Applicant : Participant
    {
        /// <inheritdoc />
        public Applicant()
        {
        }

        /// <inheritdoc />
        public Applicant(string extension)
        {
            Extension = extension;
        }

        /// <summary>
        /// Extension участника звонка
        /// </summary>
        public virtual string Extension { get; set; }

        /// <summary>
        /// т.к У Applicant нету имени, а у пользователя есть, то в базовом классе присутствует данный метод 
        /// </summary>
        public override string ParticipantName
        {
            get => null;
            set => throw new NotImplementedException($"У Applicant отсутствует свойство set {nameof(ParticipantName)}");
        }

        /// <inheritdoc />
        public override string ParticipantExtension
        {
            get => Extension;
            set => Extension = value;
        }
    }
}
