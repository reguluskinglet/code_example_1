using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using demo.CallManagement.Client;
using demo.CallManagement.HttpContracts.Dto;
using demo.FunctionalExtensions;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.InboxDistribution.HttpContracts.Enums;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Migrations;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Controllers;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Hubs;
using demo.DemoApi.Service.Tests.Core;
using Xunit;
using AcceptCallClientDto = demo.CallManagement.HttpContracts.Dto.AcceptCallClientDto;

namespace demo.DemoApi.Service.Tests.Scenario
{
    [Collection("ServicesFixture")]
    public class AcceptCallTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<InboxDistributionServiceClient> _inboxDistributionMock;
        private readonly Mock<CallManagementServiceClient> _callManagementServiceClientMock;
        private readonly Mock<PhoneHubMessageService> _phoneHubMessageServiceMock;

        public AcceptCallTests(ServicesFixture fixture)
        {
            var services = fixture.Services;
            _inboxDistributionMock = fixture.InboxDistributionClientMock;
            _callManagementServiceClientMock = fixture.CallManagementServiceClientMock;
            Mock<IHubContext<PhoneHub>> operatorClientMock = fixture.OperatorClientMock;
            _phoneHubMessageServiceMock = new Mock<PhoneHubMessageService>(
                Mock.Of<ILogger>(),
                operatorClientMock.Object);
            services.AddSingleton(_phoneHubMessageServiceMock.Object);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task AcceptCall()
        {
            //Arrange
            var userId =  Guid.NewGuid();
            var callsController = _serviceProvider.GetScopedController<CallsController>(userId);

            var inboxId = Guid.NewGuid();

            _inboxDistributionMock
                .Setup(x => x.AcceptNextItem(It.IsAny<AcceptNextItemClientDto>()))
                .ReturnsAsync(Result.Success(new UserItemClientDto()
                {
                    ItemType = ClientInboxItemType.Call,
                    ItemId = Guid.NewGuid(),
                    CreateCaseCard = true,
                    CaseTypeId = new Guid(AddCaseTableAndCaseTemplateTable.CaseTypeId112)
                }));

            _callManagementServiceClientMock.Setup(x => x.GetCallById(It.IsAny<Guid>())).ReturnsAsync(
                Result.Success(new CallClientDto()
                {

                    Line = new LineClientDto
                    {
                        CaseFolderId = null,

                    }
                })
            );

            _callManagementServiceClientMock.Setup(x => x.AcceptCall(It.IsAny<AcceptCallClientDto>())).ReturnsAsync(
                Result.Success(new CallClientDto()
                {
                    Line = new LineClientDto()
                }));

            //Act
            await callsController.Accept(
                new AcceptInboxItemDto()
                {
                    InboxId = inboxId
                }
            );

            //Assert
            _phoneHubMessageServiceMock
                .Verify(x=> x.NotifyUsersAboutAcceptedCall(
                    It.IsAny<Guid?>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>()), Times.Once);

        }

    }
}
