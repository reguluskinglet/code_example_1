using System.Linq;
using System.Threading.Tasks;
using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Репозиторий языков
    /// </summary>
    public class LanguageRepository : Repository<Language>, ILanguageRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <summary>
        /// Конструктор репозитория языков
        /// <param name="unitOfWork"></param>
        /// </summary>
        public LanguageRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Получить данные о языке по коду языка
        /// </summary>
        public async Task<Language> GetByCode(LanguageCode code)
        {
            var entity = _unitOfWork
                .Query<Language>()
                .FirstOrDefault(x => x.Code == code);

            return await Task.FromResult(entity);
        }
    }
}
