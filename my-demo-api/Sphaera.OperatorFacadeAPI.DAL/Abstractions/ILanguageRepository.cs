using System.Threading.Tasks;
using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория языков 
    /// </summary>
    public interface ILanguageRepository : IRepository<Language>
    {
        /// <summary>
        /// Получить данные о языке по коду языка
        /// </summary>
        Task<Language> GetByCode(LanguageCode code);
    }
}
