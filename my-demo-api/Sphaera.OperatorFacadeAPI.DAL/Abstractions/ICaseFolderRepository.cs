using System.Collections.Generic;
using System.Threading.Tasks;
using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория инцидента
    /// </summary>
    public interface ICaseFolderRepository : IRepository<CaseFolder>
    {
        /// <summary>
        /// Получить страницу инцидентов
        /// </summary>
        Task<CaseFolderPage> GetCaseFolderPage(int pageIndex, int pageSize);

    }
}
