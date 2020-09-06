using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.Infrastructure.Options;
using StackExchange.Redis;

namespace demo.DemoApi.Service.Configuration
{
    /// <summary>
    /// Extensions for ServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Extension для инициализации SignalR через Redis
        /// </summary>
        /// <param name="serverBuilder"></param>
        /// <param name="options">Настройки подключения Redis</param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddRedisBackPlane(this ISignalRServerBuilder serverBuilder,
            RedisOptions options)
        {
            var serviceProvider = serverBuilder.Services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();

            serverBuilder
                .AddStackExchangeRedis(
                    o =>
                    {
                        o.ConnectionFactory = async writer =>
                        {
                            ConnectionMultiplexer connection = null;
                            try
                            {
                                var config = ConfigurationOptions.Parse(options.Configuration);
                                config.AbortOnConnectFail = false;

                                connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                            }
                            catch (Exception ex)
                            {
                                logger.Fatal("Ошибка инициализации Redis", ex);
                            }

                            connection.ConnectionFailed += (_, e) =>
                            {
                                logger.Error($"Ошибка подключения к Redis. Configuration: {connection.Configuration}",
                                    e.Exception);
                            };

                            if (!connection.IsConnected)
                            {
                                logger.Error($"Ошибка подкючения к Redis. Configuration: {connection.Configuration}");
                            }

                            return connection;
                        };
                    });

            return serverBuilder;
        }
    }
}
