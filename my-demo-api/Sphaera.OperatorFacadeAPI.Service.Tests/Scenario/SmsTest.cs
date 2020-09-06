using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using demo.Authorization.Contexts;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.Http.ServiceResponse;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.Client.Options;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.InboxDistribution.HttpContracts.Enums;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.DAL.Migrations;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Controllers;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Hubs;
using demo.DemoApi.Service.Hubs.Core;
using demo.DemoApi.Service.Tests.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Scenario
{
    [Collection("ServicesFixture")]
    public class SmsTest : IAsyncLifetime
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly Mock<IHubContext<PhoneHub>> _operatorClientMock;
        private readonly Mock<InboxDistributionServiceClient> _inboxDistributionClientMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly Guid _smsInboxId;
        private readonly ISmsRepository _smsRepository;

        public SmsTest(ServicesFixture fixture)
        {
            var services = fixture.Services;

            _httpMessageHandler = fixture.HttpMessageHandler;
            _inboxDistributionClientMock = fixture.InboxDistributionClientMock;
            _operatorClientMock = fixture.OperatorClientMock;
            _serviceProvider = services.BuildServiceProvider();
            _smsRepository = _serviceProvider.GetService<ISmsRepository>();
            _smsInboxId = Guid.NewGuid();
        }

        public async Task InitializeAsync()
        {
            await ClearAllSms();
        }

        public async Task DisposeAsync()
        {
            await ClearAllSms();
        }

        [Fact]
        public async Task Sms_WhenSmsArriveAndGet_AllMustBeSuccess()
        {
            //arrange
            var userId = Guid.NewGuid();

            var callsController = _serviceProvider.GetScopedController<CallsController>(userId);
            Guid caseFolderId = Guid.Empty;
            _operatorClientMock.Setup(x => x.Clients.All.SendCoreAsync(
                "smsAccepted",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>())).Returns<string, object[], CancellationToken>(async (x, datas, z) =>
                {
                    var result = JsonConvert.SerializeObject(datas[0]);
                    var jObject = JObject.Parse(result);
                    caseFolderId = Guid.Parse(jObject["caseFolder"]["id"].ToString());
                }
            );
            var smsText = "У меня коронавирус, помогите!";
            var extension = "4124";

            //action
            var result = await _serviceProvider.MakeSms(smsText, extension);
            var acceptCallDto = CreateAcceptDto(result.ItemId);

            _inboxDistributionClientMock.Setup(x => x.AcceptItem(It.IsAny<AcceptItemClientDto>())).ReturnsAsync(Result.Success(new UserItemClientDto()
                {
                    ItemType = ClientInboxItemType.Sms,
                    ItemId = result.ItemId,
                    CaseTypeId = new Guid("6a9f90c4-2b7e-4ec3-a38f-000000000112")
                })
            );

            var operatorId = userId;
            SetCurrentUserId(operatorId);
            var smsInRepo = await IsSmsInRepo(result.ItemId);

            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.EndsWith($"users/{userId}")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new JsonContent(new UserDto {Id = userId })
            });

            var acceptResult = await callsController.Accept(acceptCallDto);

            await Task.Delay(1000);
            var caseFolder = await GetCaseFolder(caseFolderId);

            //assert
            caseFolder.ShouldNotBeNull();
            var descriptionFieldValue = caseFolder.GetFieldValue(Domain.Entities.Case.DescriptionFieldId);
            var phoneFieldValue = caseFolder.GetFieldValue(Domain.Entities.Case.NumberFieldId);
            descriptionFieldValue.ShouldBe(smsText);
            phoneFieldValue.ShouldBe(extension);

            acceptResult.ShouldBeOfType<demoResult>("Accept Call from Applicant error");
            smsInRepo.ShouldBeTrue();
        }

        private async Task<CaseFolder> GetCaseFolder(Guid caseFolderId)
        {
            var uow = _serviceProvider.GetScopedService<UnitOfWork>();
            using (uow.Begin())
            {
                var caseFolderRepository = new CaseFolderRepository(uow);
                var caseFolder = await caseFolderRepository.GetById(caseFolderId);
                return caseFolder;
            }
        }

        private async Task<bool> IsSmsInRepo(Guid resultItemId)
        {
            using var unitOfWork = _serviceProvider.GetService<UnitOfWork>();
            unitOfWork.Begin();
            var smsInInbox = await _smsRepository.GetById(resultItemId);
            if (smsInInbox == null)
            {
                return false;
            }

            return true;
        }

        private AcceptInboxItemDto CreateAcceptDto(Guid itemId)
        {
            return new AcceptInboxItemDto
            {
                InboxId = _smsInboxId,
                ItemId = itemId
            };
        }

        private async Task ClearAllSms()
        {
            var uow = _serviceProvider.GetScopedService<UnitOfWork>();
            using (uow.Begin())
            {
                var smsRepository = new SmsRepository(uow);
                await smsRepository.DeleteAllSmsAsync();
                await uow.CommitAsync();
            }
        }

        private void SetCurrentUserId(Guid id)
        {
            AuthorizationContext.CurrentUserId = id;
        }


    }
}
