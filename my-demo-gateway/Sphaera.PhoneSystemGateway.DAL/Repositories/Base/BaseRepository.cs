using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace demo.DemoGateway.DAL.Repositories.Base
{
    /// <summary>
    /// Базовый класс для репозиториев
    /// </summary>
    public abstract class BaseRepository
    {
        private readonly string _connection;

        /// <summary>
        /// Конструктор
        /// </summary>
        protected BaseRepository(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DemoGatewayDatabaseConnection");
        }

        /// <summary>
        /// Получить подключение.
        /// </summary>
        protected IDbConnection OpenConnection(string connection = null)
        {
            var npgsql = new NpgsqlConnection(connection ?? _connection);
            npgsql.Open();

            DefaultTypeMap.MatchNamesWithUnderscores = true;

            return npgsql;
        }
    }
}
