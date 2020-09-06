using System;
using demo.DDD;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Участник звонка, должен умереть
    /// </summary>
    public abstract class Participant: AggregateRoot
    {
        /// <inheritdoc />
        public Participant() : base(Guid.NewGuid())
        {
            Id = Guid.NewGuid();
        }
        
        /// <summary>
        /// Имя пользователя 
        /// </summary>
        public abstract string ParticipantName { get; set; }
        
        /// <summary>
        /// Номер
        /// </summary>
        public abstract string ParticipantExtension { get; set; }
    }
}
