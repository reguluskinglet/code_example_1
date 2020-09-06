using demo.DemoApi.Service.Dtos.Logging;
using System.Collections.Generic;
using Xunit;
using demo.Monitoring.Logger;
using Moq;
using Newtonsoft.Json;
using System;
using Shouldly;
using demo.DemoApi.Service.Infrastructure.Logging;

namespace demo.DemoApi.Service.Tests.Services
{
    [Collection("ServicesFixture")]
    public class WebClientLoggingServiceTests
    {
        private ILogger GetMockedLogger(Action<string, IDictionary<string, string>> logInformation)
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.Information(
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>()
            )).Callback(logInformation);
            return loggerMock.Object;
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void LogMessageFromWebClient_MessageIsValidJsonString()
        {
            const string testLogMessage = "Тестовое сообщение \n строка 2 \n \"текст в кавычках\"";

            var webClientLoggingService = new WebClientLoggingService(GetMockedLogger((message, parameters) =>
            {
                var receivedMessage = JsonConvert.DeserializeObject<string>($"\"{message}\"");
                receivedMessage.ShouldBe(testLogMessage);
            }));

            var messageItem = new WebClientLogInfo
            {
                Message = testLogMessage,
                AdditionalParams = new Dictionary<string, string>(),
                Level = "",
                Stacktrace = "",
                ClientVersion = "0.1.0"
            };
            var logs = new List<WebClientLogInfo> { messageItem };

            webClientLoggingService.Log(new WebClientLogs { Logs = logs });
        }
    }
}
