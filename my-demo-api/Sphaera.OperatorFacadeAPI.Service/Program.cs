using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using demo.Monitoring.Common;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Core;

namespace demo.DemoApi.Service
{
    /// <summary>
    /// Программа
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Начало
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")
                .Build();

            var provider = host.Services;
            var logger = provider.GetService<ILogger>();
            logger.Information($"{MonitoringContext.ServiceName} START");

            var configurationRoot = provider.GetService<IConfiguration>() as IConfigurationRoot;
            logger.WriteConfig(configurationRoot);

            try
            {
                using (var scope = provider.CreateScope())
                {
                    InitializeDataBase(scope.ServiceProvider);
                }

                host.Run();
            }
            catch (Exception e)
            {
                logger.Error("Ошибка при старте приложения", e);
            }

            logger.Information($"{MonitoringContext.ServiceName} STOP");
        }

        /// <summary>
        /// Создать создателя хоста
        /// </summary>
        /// <param name="args"></param>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseShutdownTimeout(TimeSpan.FromSeconds(10))
                .UseStartup<Startup>();
        }

        /// <summary>Мигрировать БД и добавить/обновить данные.</summary>
        /// <param name="services"></param>
        public static void InitializeDataBase(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.Information("Актуализация базы данных");
            DatabaseSeed.CreateDataBaseIfNotExists(services);
            var runner = services.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
            logger.Information("Актуализация базы данных успешно завершена");
        }
    }
}
