using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AsterNET.ARI;
using AsterNET.ARI.Models;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.Commands.Factory;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using Xunit;

namespace demo.DemoGateway.UnitTests.Services
{
    public class AsteriskAriApiServiceTests
    {
        [Fact]
        public async Task AcceptIncomingCall_WithCorrectData_ShouldSucceed()
        {
            var routeDto = new RouteCallDto
            {
                FromCallId = Guid.NewGuid(),
                LineId = Guid.NewGuid()
            };

            var expectedChannelId = "0b279ab6-9738-4ea7-a0bc-72d757f7f72b";
            var expectedAppName = "ccng";
            var expectedEndpoint = "PJSIP/@kamailio";
            var expectedAppArgs = GetEncodedAppArgs(CreateStasisArgs(expectedChannelId, null, StasisStartEventType.AcceptIncomingCall, routeDto));

            string appNameResult = null;
            string endpointResult = null;
            string appArgsResult = null;

            var ariClientMock = new Mock<IAriClient>();

            ariClientMock.Setup(x => x
                .Channels
                .GetAsync(It.IsAny<string>())
            ).ReturnsAsync(new Channel { Id = expectedChannelId });

            ariClientMock.Setup(x => x.Channels.OriginateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<long?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .Callback<string, string, string, long?, string, string, string, string, int?,
                    IDictionary<string, string>, string, string, string, string>(
                    (endpoint, extension, context, priority, label, app, appArgs, callerId, timeout, variables,
                        channelId, otherChannelId, originator, formats) =>
                    {
                        appNameResult = app;
                        endpointResult = endpoint;
                        appArgsResult = appArgs;
                    });

            var ariClient = ariClientMock.Object;
            var ariApiService = GetAsteriskAriApiService(ariClient);

            // Act
            var result = await ariApiService.AcceptIncomingCall(routeDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            appNameResult.ShouldBe(expectedAppName);
            endpointResult.ShouldBe(expectedEndpoint);
            appArgsResult.ShouldBe(expectedAppArgs);
        }

        [Fact]
        public async Task AddToConference_WithCorrectData_ShouldSucceed()
        {
            var routeDto = new RouteCallDto
            {
                LineId = Guid.NewGuid()
            };

            var expectedBridgeId = "someBridgeId";
            var expectedAppName = "ccng";
            var expectedEndpoint = "PJSIP/@kamailio";
            var expectedAppArgs = GetEncodedAppArgs(CreateStasisArgs(null, expectedBridgeId, StasisStartEventType.Conference, routeDto));

            string appNameResult = null;
            string endpointResult = null;
            string appArgsResult = null;

            var ariClientMock = new Mock<IAriClient>();

            ariClientMock.Setup(x => x.Channels.OriginateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<long?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .Callback<string, string, string, long?, string, string, string, string, int?,
                    IDictionary<string, string>, string, string, string, string>(
                    (endpoint, extension, context, priority, label, app, appArgs, callerId, timeout, variables,
                        channelId, otherChannelId, originator, formats) =>
                    {
                        appNameResult = app;
                        endpointResult = endpoint;
                        appArgsResult = appArgs;
                    });

            var ariClient = ariClientMock.Object;
            var ariApiService = GetAsteriskAriApiService(ariClient);

            // Act
            var result = await ariApiService.AddToConference(routeDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            appNameResult.ShouldBe(expectedAppName);
            endpointResult.ShouldBe(expectedEndpoint);
            appArgsResult.ShouldBe(expectedAppArgs);
        }

        [Fact]
        public async Task AddAssistant_WithCorrectData_ShouldSucceed()
        {
            var routeDto = new RouteCallDto
            {
                LineId = Guid.NewGuid(),
                FromCallId = Guid.NewGuid(),
                ToCallId = Guid.NewGuid(),
            };

            var expectedBridgeId = "someBridgeId";
            var expectedAppName = "ccng";
            var expectedEndpoint = "PJSIP/@kamailio";
            var expectedAppArgs = GetEncodedAppArgs(CreateStasisArgs(null, expectedBridgeId, StasisStartEventType.Assistant, routeDto));

            string appNameResult = null;
            string endpointResult = null;
            string appArgsResult = null;

            var ariClientMock = new Mock<IAriClient>();

            ariClientMock.Setup(x => x.Channels.OriginateAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<long?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    null,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ))
                .Callback<string, string, string, long?, string, string, string, string, int?,
                    IDictionary<string, string>, string, string, string, string>(
                    (endpoint, extension, context, priority, label, app, appArgs, callerId, timeout, variables,
                        channelId, otherChannelId, originator, formats) =>
                    {
                        appNameResult = app;
                        endpointResult = endpoint;
                        appArgsResult = appArgs;
                    });

            var ariClient = ariClientMock.Object;
            var ariApiService = GetAsteriskAriApiService(ariClient);

            // Act
            var result = await ariApiService.AddAssistant(routeDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            appNameResult.ShouldBe(expectedAppName);
            endpointResult.ShouldBe(expectedEndpoint);
            appArgsResult.ShouldBe(expectedAppArgs);
        }

        private static AsteriskAriApiService GetAsteriskAriApiService(IAriClient ariClient)
        {
            var channelRepositoryMock = new Mock<IChannelRepository>();
            channelRepositoryMock
                .Setup(x => x.GetChannelsByLineId(It.IsAny<Guid>()))
                .ReturnsAsync(new List<DAL.Entities.Channel>
                {
                    new DAL.Entities.Channel
                    {
                        BridgeId = "someBridgeId"
                    }
                });

            channelRepositoryMock
                .Setup(x => x.GetChannelByCallId(It.IsAny<Guid>()))
                .ReturnsAsync(new DAL.Entities.Channel
                {
                    BridgeId = "someBridgeId",
                    ChannelId = "0b279ab6-9738-4ea7-a0bc-72d757f7f72b"
                });

            channelRepositoryMock
                .Setup(x => x.GetMainBridgeId(It.IsAny<Guid>()))
                .ReturnsAsync("someBridgeId");

            channelRepositoryMock
                .Setup(x => x.GetChannelForMainUser(It.IsAny<Guid>()))
                .ReturnsAsync(new DAL.Entities.Channel
                {
                    BridgeId = "someBridgeId"
                });

            var logger = new Mock<ILogger>().Object;
            var channelRepository = channelRepositoryMock.Object;
            var serviceProvider = new Mock<IServiceProvider>().Object;
            var commandFactoryMock = new Mock<CommandFactory>(serviceProvider);
            var commandFactory = commandFactoryMock.Object;

            var ariWebsocketService = new AsteriskAriWebSocketService(
                logger,
                commandFactory,
                ariClient
            );

            var ariApiService = new AsteriskAriApiService(logger, channelRepository, ariWebsocketService);
            return ariApiService;
        }

        private StasisStartEventArgs CreateStasisArgs(string channelId, string bridgeId, StasisStartEventType eventType,
            RouteCallDto dto)
        {
            var args = new StasisStartEventArgs
            {
                EventType = eventType,
                ChannelId = channelId,
                RouteData = dto,
                BridgeId = bridgeId
            };

            return args;
        }

        private string GetEncodedAppArgs(StasisStartEventArgs args)
        {
            var serializedArgs = JsonConvert.SerializeObject(args);
            var encodedArgs = WebUtility.HtmlEncode(serializedArgs);

            return $"\"{encodedArgs}\"";
        }
    }
}