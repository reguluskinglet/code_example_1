using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using demo.CallManagement.Client;
using demo.CallManagement.Client.Options;
using demo.DDD;
using demo.Http.Client;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.Client.Options;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Controllers;
using demo.DemoApi.Service.Hubs;
using demo.DemoApi.Service.Hubs.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Core
{
    public class ServicesFixture
    {
        public Mock<InboxDistributionServiceClient> InboxDistributionClientMock
        {
            get;
            private set;
        }

        public Mock<CallManagementServiceClient> CallManagementServiceClientMock
        {
            get;
            private set;
        }

        public Mock<HttpMessageHandler> HttpMessageHandler
        {
            get;
            private set;
        }
        public Mock<IHubContext<PhoneHub>> OperatorClientMock { get; }
        public ServiceCollection Services { get; }

        public ServicesFixture()
        {
            var serviceCollection = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            new Startup(config).ConfigureServices(serviceCollection);
            MockInboxDistributionClient();
            MockCallManagmentClient();
            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddSingleton(new Mock<ILogger>());
            serviceCollection.AddScoped<CallsController>();
            serviceCollection.AddScoped<LinesController>();
            serviceCollection.AddScoped<InboxesController>();
            serviceCollection.AddScoped<UnitOfWork>();
            OperatorClientMock = new Mock<IHubContext<PhoneHub>>();
            serviceCollection.AddSingleton(OperatorClientMock.Object);
            serviceCollection.AddSingleton(InboxDistributionClientMock.Object);
            serviceCollection.AddSingleton(CallManagementServiceClientMock.Object);
            serviceCollection.AddSingleton(MockHttpFactory());
            OperatorClientMock.Setup(x => x.Clients.All.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>())).Returns<string, object[], CancellationToken>(async (x, datas, z) =>
                {
                    //...
                }
            );

            Program.InitializeDataBase(serviceCollection.BuildServiceProvider());

            Services = serviceCollection;
        }

        private IHttpClientFactory MockHttpFactory()
        {
            var content = new BaseClientResult<ErrorCodes>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };

            HttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new JsonContent(content)
                });

            var httpClient = new HttpClient(HttpMessageHandler.Object) { BaseAddress = new Uri("https://www.test.test/") };
            var httpFactoryMock = new Mock<IHttpClientFactory>();
            httpFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return httpFactoryMock.Object;
        }

        private void MockInboxDistributionClient()
        {
            var options = new OptionsWrapper<InboxDistributionServiceOptions>( new InboxDistributionServiceOptions () );
            InboxDistributionClientMock = new Mock<InboxDistributionServiceClient>(
                MockHttpFactory(),
                Mock.Of<ILogger>(),
                options);
        }

        private void MockCallManagmentClient()
        {
            var options = new OptionsWrapper<CallManagementServiceOptions>( new CallManagementServiceOptions () );
            CallManagementServiceClientMock = new Mock<CallManagementServiceClient>(
                MockHttpFactory(),
                Mock.Of<ILogger>(),
                options);
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
