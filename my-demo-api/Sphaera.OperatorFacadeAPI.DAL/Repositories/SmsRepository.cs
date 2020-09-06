using System;
using System.Collections.Generic;
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
    /// Репозиторий смс.
    /// </summary>
    public class SmsRepository : Repository<Sms>, ISmsRepository
    {
        private readonly UnitOfWork _unitOfWork;

        /// <summary>
        /// Конструктор для инъекции зависимостей
        /// </summary>
        /// <param name="unitOfWork"></param>
        public SmsRepository(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Удалить все sms из БД
        /// </summary>
        public async Task DeleteAllSmsAsync()
        {
            await _unitOfWork.CreateSQLQuery("DELETE FROM sms").ExecuteUpdateAsync();
        }
    }
}
