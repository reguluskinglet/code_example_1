using System.Linq;
using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Интерфейс репозитория шаблона карточки
    /// </summary>
    public class CaseTypeRepository : Repository<CaseType>, ICaseTypeRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public CaseTypeRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Получить первый шаблон
        /// </summary>
        /// <returns></returns>
        public CaseType GetFirst()
        {
            var type = _unitOfWork.Query<CaseType>()
                .FirstOrDefault();

            return type;
        }
    }
}
