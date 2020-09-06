using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория контакта
    /// </summary>
    public interface IContactRepository : IRepository<Contact>
    {
        
    }
}
