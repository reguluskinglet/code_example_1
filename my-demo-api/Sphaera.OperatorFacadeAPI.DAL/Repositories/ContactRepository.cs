using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для контакта
    /// </summary>
    public class ContactRepository : Repository<Contact>, IContactRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public ContactRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
