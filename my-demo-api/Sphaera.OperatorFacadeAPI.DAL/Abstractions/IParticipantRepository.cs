using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория участников вызова
    /// </summary>
    public interface IParticipantRepository : IRepository<Participant>
    {
    }
}
