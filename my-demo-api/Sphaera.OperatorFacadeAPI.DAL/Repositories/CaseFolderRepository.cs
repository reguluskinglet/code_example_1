using System.Linq;
using System.Threading.Tasks;
using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Репозиторий инцидентов
    /// </summary>
    public class CaseFolderRepository : Repository<CaseFolder>, ICaseFolderRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public CaseFolderRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Получить страницу инцидентов
        /// </summary>
        public async Task<CaseFolderPage> GetCaseFolderPage(int pageIndex, int pageSize)
        {
            var query = _unitOfWork.Query<CaseFolder>();

            var page = query
                .OrderByDescending(x => x.Cases.Max(c => c.Created))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var caseFolderPage = new CaseFolderPage
            {
                CaseFolders = page,
                TotalCount = query.Count()
            };

            return await Task.FromResult(caseFolderPage);
        }
    }
}
