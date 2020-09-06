using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using Xunit;

namespace demo.DemoGateway.Tests.Repositories
{
    public class ChannelRepositoryTests
    {
        private readonly ServiceProvider _provider;

        public ChannelRepositoryTests()
        {
            var serviceCollection = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            new Startup(config).ConfigureServices(serviceCollection);

            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddSingleton(new Mock<ILogger>());
            Program.InitializeDataBase(serviceCollection.BuildServiceProvider());

            _provider = serviceCollection.BuildServiceProvider();
            Program.InitializeDataBase(_provider);
        }

        [Fact]
        public async Task ChannelRepository_AddGet_Success()
        {
            var channelRepository = _provider.GetService<IChannelRepository>();

            var channel = new Channel
            {
                ChannelId = Guid.NewGuid().ToString(),
                BridgeId = Guid.NewGuid().ToString(),
                CallId = Guid.NewGuid(),
                Extension = "101"
            };

            await channelRepository.AddChannel(channel);
            var channelFromDb = await channelRepository.GetByChannelId(channel.ChannelId);

            Assert.NotNull(channelFromDb);
            channelFromDb.CallId.ShouldBe(channel.CallId);
        }

        [Fact]
        public async Task ChannelRepository_AddGetByCallId_Success()
        {
            var channelRepository = _provider.GetService<IChannelRepository>();

            var channel1 = new Channel
            {
                ChannelId = Guid.NewGuid().ToString(),
                BridgeId = Guid.NewGuid().ToString(),
                CallId = Guid.NewGuid(),
                Extension = "101"
            };

            await channelRepository.AddChannel(channel1);

            var channelsFromDb = await channelRepository.GetChannelByCallId(channel1.CallId);
            Assert.NotNull(channelsFromDb);
        }
    }
}
