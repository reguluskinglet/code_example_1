using demo.DDD;
using demo.DDD.Repository;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для Заявителя
    /// </summary>
    public class ApplicantRepository : Repository<Applicant>, IApplicantRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <inheritdoc />
        public ApplicantRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
