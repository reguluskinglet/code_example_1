using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using demo.Monitoring.Logger;
using Xunit;

namespace demo.DemoGateway.Tests.Core
{
    public class ServicesFixture : IDisposable
    {
        public ServicesFixture()
        {
            var serviceCollection = new ServiceCollection();

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            new Startup(config).ConfigureServices(serviceCollection);

            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddSingleton(new Mock<ILogger>());

            Services = serviceCollection;
        }

        public ServiceCollection Services { get; }

        public void Dispose()
        {
            // clean up test data from the database
        }

        [CollectionDefinition("ServicesFixture")]
        public class TestFixtureCollection : ICollectionFixture<ServicesFixture>
        {
            // This class has no code, and is never created. Its purpose is simply
            // to be the place to apply [CollectionDefinition] and all the
            // ICollectionFixture<> interfaces.
        }
    }
}