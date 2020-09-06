using System;
using demo.DDD;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Пользователь карточки.
    /// </summary>
    public class CaseUser: BaseEntity
    {
        /// <inheritdoc />
        public CaseUser()
        {

        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="userId"></param>
        public CaseUser(Guid userId, Case @case):base(Guid.NewGuid())
        {
            UserId = userId;
            Case = @case;
        }

        /// <inheritdoc />
        public override AggregateRoot Root => Case.Root;

        /// <summary>
        /// Карточка.
        /// </summary>
        public Case Case { get; private set; }

        /// <summary>
        /// Карточка.
        /// </summary>
        public Guid UserId { get; private set; }
    }
}
