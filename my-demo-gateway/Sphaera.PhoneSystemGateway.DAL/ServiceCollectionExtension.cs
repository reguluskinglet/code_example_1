using Microsoft.Extensions.DependencyInjection;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Repositories;

namespace demo.DemoGateway.DAL
{
    /// <summary>
    /// Расширения для добавления репозиториев в проект
    /// </summary>
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Добавление репозиториев в проект
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void AddDataAccessLayer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IChannelRepository, ChannelRepository>();
            serviceCollection.AddTransient<IAudioRecordRepository, AudioRecordRepository>();
        }
    }
}
