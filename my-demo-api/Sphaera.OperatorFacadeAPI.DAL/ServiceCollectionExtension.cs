using Microsoft.Extensions.DependencyInjection;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.DAL.Repositories;

namespace demo.DemoApi.DAL
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
            serviceCollection.AddScoped<IVersionInfoRepository, VersionInfoRepository>();
            serviceCollection.AddTransient<ICaseRepository, CaseRepository>();
            serviceCollection.AddTransient<ICaseTypeRepository, CaseTypeRepository>();
            serviceCollection.AddTransient<IApplicantRepository, ApplicantRepository>();
            serviceCollection.AddTransient<ICaseFolderRepository, CaseFolderRepository>();
            serviceCollection.AddTransient<ISmsRepository, SmsRepository>();
            serviceCollection.AddTransient<ILanguageRepository, LanguageRepository>();
            serviceCollection.AddTransient<IContactRepository, ContactRepository>();
            serviceCollection.AddTransient<IParticipantRepository, ParticipantRepository>();
        }
    }
}
