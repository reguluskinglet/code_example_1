using Microsoft.Extensions.DependencyInjection;
using demo.DemoApi.Service.ApplicationServices.Abstractions;
using demo.DemoApi.Service.ApplicationServices.Cache;
using demo.DemoApi.Service.ApplicationServices.Lines;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Класс для расширение возможностей стандартной коллекции сервисов
    /// </summary>
    public static class ApplicationLayerExtension
    {
        /// <summary>
        /// Добавление прикладных сервисов к коллекции сервисов
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void AddApplicationLayer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<GisService>();
            serviceCollection.AddTransient<PhoneHubMessageService>();
            serviceCollection.AddTransient<CaseService>();
            serviceCollection.AddTransient<CaseFolderService>();
            serviceCollection.AddTransient<CacheProvider>();
            serviceCollection.AddTransient<ILineService, LineService>();
            serviceCollection.AddTransient<InboxService>();
            serviceCollection.AddTransient<IndexApplicationService>();
            serviceCollection.AddTransient<ContactManagementService>();
            serviceCollection.AddTransient<CallManagementService>();
            serviceCollection.AddTransient<LanguageService>();
            serviceCollection.AddTransient<UserService>();
            serviceCollection.AddTransient<AudioRecordService>();
        }
    }
}
