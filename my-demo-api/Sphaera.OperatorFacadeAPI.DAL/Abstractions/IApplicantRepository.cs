using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория заявителей
    /// </summary>
    public interface IApplicantRepository: IRepository<Applicant>
    {
        
    }
}
