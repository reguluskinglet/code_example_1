using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория шаблона карточки
    /// </summary>
    public interface ICaseTypeRepository : IRepository<CaseType>
    {
        CaseType GetFirst();
    }
}
