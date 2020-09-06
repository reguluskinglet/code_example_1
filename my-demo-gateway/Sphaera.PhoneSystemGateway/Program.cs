using System;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using demo.Monitoring.Common;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL;
using demo.DemoGateway.Infrastructure;
using demo.DemoGateway.Infrastructure.HostedServices;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;

namespace demo.DemoGateway
{
    /// <summary>
    /// Программа
    /// </summary>
    public class Program
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

            var configurationRoot = provider.GetService<IConfiguration>() as ConfigurationRoot;
            logger.WriteConfig(configurationRoot);

            try
            {
                using (var scope = provider.CreateScope())
                {
                    InitializeDataBase(scope.ServiceProvider);
                    StartAsteriskAriClient(scope);
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

        private static void StartAsteriskAriClient(IServiceScope scope)
        {
            var asteriskAri = (AsteriskAriWebSocketService)scope.ServiceProvider.GetService(typeof(AsteriskAriWebSocketService));
            asteriskAri.Start();
        }

        /// <summary>Мигрировать БД и добавить/обновить данные.</summary>
        /// <param name="services"></param>
        public static void InitializeDataBase(IServiceProvider services)
        {
            ILogger logger = null;
            try
            {
                logger = services.GetRequiredService<ILogger>();
                logger.Information("Актуализация базы данных");
                DemoGatewaySeed.CreateDataBaseIfNotExists(services);
                var runner = services.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
                logger.Information("Актуализация базы данных успешно завершена");
            }
            catch (Exception ex)
            {
                logger.Error($"Произошла ошибка миграции БД.\n{ex.Message}\n{ex}");
                throw;
            }
        }
    }
}