using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using demo.DDD.Repository;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория sms
    /// </summary>
    public interface ISmsRepository : IRepository<Sms>
    {
        /// <summary>
        /// Удалить все sms из БД
        /// </summary>
        Task DeleteAllSmsAsync();
    }
}
