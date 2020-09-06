using System;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace demo.DemoApi.DAL.Core
{
    /// <summary>
    /// Наполнитель данных
    /// </summary>
    public static class DatabaseSeed
    {
        /// <summary>
        /// Создание БД если нет
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void CreateDataBaseIfNotExists(IServiceProvider serviceProvider)
        {
            var connection = serviceProvider.GetService<IConfiguration>().GetConnectionString("DatabaseConnection");

            string[] parameters = connection.Split(";");
            string paramName = "Database";
            string[] databaseParam = parameters.First(s => s.StartsWith(paramName, StringComparison.InvariantCultureIgnoreCase))?.Split('=');

            if (databaseParam?.Length != 2)
            {
                throw new InvalidOperationException($"Не найдено значение параметра \"{paramName}\" в строке подключения.");
            }

            var catalog = databaseParam[1];
            connection = string.Join(";", parameters.Where(s => !s.StartsWith(paramName, StringComparison.InvariantCultureIgnoreCase)));

            var databasesQuery = "select * from postgres.pg_catalog.pg_database where datname = @name";
            var createDatabaseQuery = $"CREATE DATABASE \"{catalog}\"";

            using (var db = new NpgsqlConnection(connection))
            {
                if (!db.Query(databasesQuery, new { name = catalog }).Any())
                {
                    db.Execute(string.Format(createDatabaseQuery, catalog));
                }
            }
        }
    }
}
