using System.Threading.Tasks;
using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория карточки
    /// </summary>
    public interface ICaseRepository : IRepository<Case>
    {
        
    }
}
