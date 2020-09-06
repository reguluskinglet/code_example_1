using System.Threading.Tasks;
using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Интерфейс репозитория карточки
    /// </summary>
    public class CaseRepository : Repository<Case>, ICaseRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public CaseRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
