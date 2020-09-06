using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Репозиторий операторов
    /// </summary>
    public class ParticipantRepository : Repository<Participant>, IParticipantRepository
    {
        /// <inheritdoc />
        public ParticipantRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
