using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Moq;
using Shouldly;
using demo.Monitoring.Logger;
using demo.DemoGateway.DAL;
using demo.DemoGateway.Dtos.Sms;
using demo.DemoGateway.Infrastructure.Parsers.Sms;
using Xunit;

namespace demo.DemoGateway.UnitTests.Services
{
    public class SmsParserTests
    {
        private readonly SmsParser _smsParser;

        public SmsParserTests()
        {
            var logger = new Mock<ILogger>();
            var fieldParser = new SmsFieldParser();
            _smsParser = new SmsParser(logger.Object, fieldParser);
        }

        [Theory]
        [MemberData(nameof(CorrectMessageData))]
        public void GetSmsText_ShouldBeSuccessful(string fullMessage, string expectedText)
        {
            //Act
            var resultText = _smsParser.GetSmsText(fullMessage);

            //Assert
            resultText.IsFailure.ShouldBeFalse();
            var expectedMessageTrimmed = Regex.Replace(expectedText, "\r|\n|\\s", string.Empty);
            var resultMessageTrimmed = Regex.Replace(resultText.Value, "\r|\n|\\s", string.Empty);
            resultMessageTrimmed.ShouldBe(expectedMessageTrimmed);
        }

        [Theory]
        [MemberData(nameof(MessageWithMetadataData))]
        public void GetMetadata_ShouldBeSuccessful(string fullMessage, SmsMetadata expectedData)
        {
            //Act
            var result = _smsParser.GetMetadata(fullMessage);

            //Assert
            result.IsFailure.ShouldBeFalse();
            result.Value.Position.ShouldNotBeNull();
            result.Value.Position.Latitude.ShouldBe(expectedData.Position.Latitude);
            result.Value.Position.Longitude.ShouldBe(expectedData.Position.Longitude);
            result.Value.Timestamp.ShouldBe(expectedData.Timestamp);
            result.Value.Radius.ShouldBe(expectedData.Radius);
            result.Value.InnerRadius.ShouldBe(expectedData.InnerRadius);
            result.Value.OuterRadius.ShouldBe(expectedData.OuterRadius);
            result.Value.OpeningAngle.ShouldBe(expectedData.OpeningAngle);
            result.Value.StartAngle.ShouldBe(expectedData.StartAngle);
        }

        public static IEnumerable<object[]> CorrectMessageData => new[]
        {
            new object[]
            {
                ResourceManager.GetResource($"CorrectMessageData1.txt"),
                "Hello world!"
            },
            new object[]
            {
                ResourceManager.GetResource($"CorrectMessageData2.txt"),
                $"First Line {Environment.NewLine} Second Line{Environment.NewLine} Third Line"
            },
        };

        public static IEnumerable<object[]> MessageWithMetadataData => new[]
        {
            new object[]
            {
                ResourceManager.GetResource($"MessageWithMetadata1.txt"),
                new SmsMetadata
                {
                    Position = new SmsPosition
                    {
                        Latitude = 50.1657,
                        Longitude = 53.194,
                    },
                    Timestamp = "2020-03-02T10:52:22.604Z",
                    Radius = 250,
                },
            },
            new object[]
            {
                ResourceManager.GetResource($"MessageWithMetadata2.txt"),
                new SmsMetadata
                {
                    Position = new SmsPosition
                    {
                        Latitude = 53,
                        Longitude = 47,
                    },
                    Timestamp = "2020-02-02T10:52:22.604Z",
                    InnerRadius = 150,
                    OuterRadius = 450,
                    StartAngle = 20,
                    OpeningAngle = 20
                },
            },
        };
    }
}