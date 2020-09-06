using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Shouldly;
using demo.Authorization.Contexts;
using demo.CallManagement.Client;
using demo.DDD;
using demo.Http.ServiceResponse;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.DAL.Migrations;
using demo.DemoApi.DAL.Migrations.Metadata;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.CaseStrategy.Enums;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Service.Controllers;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Case;
using demo.DemoApi.Service.Hubs.Core;
using demo.DemoApi.Service.Tests.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Controllers
{
    [Collection("ServicesFixture")]
    public class CaseControllerTests : IDisposable
    {
        private readonly CasesController _caseController;
        private readonly UnitOfWork _unitOfWork;
        private readonly ICaseTypeRepository _caseTypeRepository;
        private readonly ICaseFolderRepository _caseFolderRepository;
        private readonly Mock<HttpMessageHandler> _httpMessageHandler;
        private readonly Guid _case112TypeId = new Guid(AddCaseTableAndCaseTemplateTable.CaseTypeId112);
        private readonly ServiceProvider _provider;

        public CaseControllerTests(ServicesFixture fixture)
        {
            fixture.Services.AddSingleton<CasesController>();
            _httpMessageHandler = fixture.HttpMessageHandler;
            _provider = fixture.Services.BuildServiceProvider();

            _caseTypeRepository = _provider.GetService<ICaseTypeRepository>();
            _caseFolderRepository = _provider.GetService<ICaseFolderRepository>();
            _caseController = _provider.GetService<CasesController>();
            _unitOfWork = _provider.GetService<UnitOfWork>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void Get_Case_ShouldExists()
        {
            //arrange
            _unitOfWork.Begin();

            var caseType = _caseTypeRepository.GetFirst();
            var userId = Guid.NewGuid();
            var caseFolder = new CaseFolder();
            caseFolder.AddCaseCard(caseType, userId);

            await _caseFolderRepository.Add(caseFolder);

            await _unitOfWork.CommitAsync();

            //action
            SetCurrentUserId(userId);

            demoResult<CaseDto> caseCardResult = await _caseController.GetCase(caseFolder.Id);

            //Assert
            caseCardResult.Result.ShouldBeOfType<demoOkResult<CaseDto>>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void Get_CaseWithNotExistingCallId_ShouldReturnBedRequest()
        {
            //Arrange
            var caseFolderId = new Guid("81fa8ce9-e2e6-433c-a9fb-5dcd5fd25245");
            var userId = new Guid("81fa8ce9-e2e6-433c-a9fb-5dcd5fd12123");

            _unitOfWork.Begin();

            //Action
            SetCurrentUserId(userId);

            demoResult<CaseDto> caseCardResult = await _caseController.GetCase(caseFolderId);

            //Assert
            caseCardResult.Result.ShouldBeOfType<demoErrorResult>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void GetField_FromCaseData_ShouldExists()
        {
            var fieldId = new Guid("81fa8ce9-e2e6-0001-0001-000000000001");
            var fieldValue = "Fire";
            //Arrange

            var caseData = $"[{{\"fieldId\":\"{fieldId.ToString()}\",\"blockId\":\"81fa8ce9-e2e6-0001-0001-000000000000\",\"value\":\"{fieldValue}\"}}]";

            Guid caseFolderId = await CreateCaseFolder(caseData, new Guid());

            //Action
            var result = (await _caseController.GetField(caseFolderId, fieldId)).Result as demoOkResult<CaseFieldDto>;

            //Assert
            result.Value.Value.ShouldBe(fieldValue);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void GetField_FromCaseData_ShouldNotExists()
        {
            //Arrange
            var caseData = "[{\"fieldId\":\"81fa8ce9-e2e6-0001-0001-000000000001\",\"blockId\":\"81fa8ce9-e2e6-0001-0001-000000000000\",\"value\":\"FieldValue\"}]";

            Guid caseFolderId = await CreateCaseFolder(caseData, Guid.NewGuid());

            //Action
            var result = await _caseController.GetField(caseFolderId, Guid.NewGuid());

            //Assert
            result.Result.ShouldBeOfType<demoErrorResult>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void UpdateField_WithValidId_SuccessfullyUpdated()
        {
            //Arrange
            var fieldId = new Guid("81fa8ce9-e2e6-0001-0001-000000000001");
            var fieldValue = "Fire";
            var caseData = $"[{{\"fieldId\":\"{fieldId.ToString()}\",\"blockId\":\"81fa8ce9-e2e6-0001-0001-000000000000\",\"value\":\"SomeValue\"}}]";
            Guid userId = UsersMetadata.UserIdOperator112;

            var caseFolderId = await CreateCaseFolder(caseData, userId);

            //Action
            string caseDataDto;
            var caseFieldDto = new CaseFieldDto
            {
                CaseFolderId = caseFolderId,
                FieldId = fieldId,
                Value = fieldValue,
            };

            SetCurrentUserId(userId);
            var result = await _caseController.UpdateField(caseFieldDto);
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                caseDataDto = caseFolder.Data;
            }

            //Assert
            result.ShouldBeOfType<demoResult>();
            result.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
            caseDataDto.ShouldContain(fieldValue);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void UpdateField_WithNotExistingId_ShouldFail()
        {
            //Arrange
            var fieldId = new Guid("81fa8ce9-e2e6-0001-0001-000000000002");
            var fieldValue = "Fire";
            var caseData = $"[{{\"fieldId\":\"{fieldId.ToString()}\",\"blockId\":\"81fa8ce9-e2e6-0001-0001-000000000000\",\"value\":\"SomeValue\"}}]";
            await CreateCaseFolder(caseData, new Guid());

            //Action
            var caseFieldDto = new CaseFieldDto
            {
                CaseFolderId = Guid.Empty,
                FieldId = fieldId,
                Value = fieldValue
            };

            var result = await _caseController.UpdateField(caseFieldDto);

            //Assert
            result.ShouldBeOfType<demoErrorResult>();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void UpdateCaseFolderStatus_WhenCaseStatusIsClosed_ShouldBeSuccess()
        {
            //Arrange
            var fieldId = new Guid("81fa8ce9-e2e6-0001-0001-000000000001");
            var caseData = $"[{{\"fieldId\":\"{fieldId.ToString()}\",\"blockId\":\"81fa8ce9-e2e6-0001-0001-000000000000\",\"value\":\"SomeValue\"}}]";
            Guid userId = UsersMetadata.UserIdOperator112;

            var  caseFolderId = await CreateCaseFolder(caseData, userId);
            SetCurrentUserId(userId);

            //Action

            var caseDtoResult = (await _caseController.GetCase(caseFolderId)).Result as demoOkResult<CaseDto>;
            var caseDto = caseDtoResult?.Value as CaseDto;

            var closeCaseCardDto = new CloseCaseCardDto
            {
                CaseCardId = caseDto.CaseId,
            };

            var closeCaseResult = await _caseController.CloseCaseCard(closeCaseCardDto);

            CaseFolder caseFolder;
            using (_unitOfWork.Begin())
            {
                caseFolder = await _caseFolderRepository.GetById(caseFolderId);
            }

            var caseStatusesDtoResult = (await _caseController.Statuses(caseFolderId)).Result as demoOkResult<CaseStatusesInfoDto>;
            var caseStatusesDto = caseStatusesDtoResult.Value as CaseStatusesInfoDto;

            //Assert
            closeCaseResult.ShouldBeOfType<demoResult>();
            closeCaseResult.HttpStatusCode.ShouldBe(HttpStatusCode.OK);
            caseFolder.Status.ShouldBe(CaseFolderStatus.Closed);
            caseStatusesDto.CanCloseCaseCard.ShouldBeFalse();
            caseStatusesDto.Statuses.ShouldContain(x => x.Status == Case112Status.Closed.ToString());
        }

        /// <summary>
        /// Создание Line, CaseFolder и добавление Data к CaseFolder
        /// </summary>
        /// <param name="caseData"></param>
        /// <returns>LineId</returns>
        private async Task<Guid> CreateCaseFolder(string caseData, Guid userId)
        {
            using var uow = _provider.GetScopedService<UnitOfWork>();
            uow.Begin();
            var caseTypeRepo = new CaseTypeRepository(uow);
            var caseType = await caseTypeRepo.GetById(_case112TypeId);

            var caseFolder = new CaseFolder { Data = caseData };
            caseFolder.AddCaseCard(caseType, userId);
            var caseFolderRepository = new CaseFolderRepository(uow);
            await caseFolderRepository.Add(caseFolder);
            await uow.CommitAsync();
            return caseFolder.Id;
        }

        public async void Dispose()
        {
        }

        private void SetCurrentUserId(Guid id)
        {
            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsoluteUri.EndsWith($"users/{id}")),
                ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new JsonContent(new UserDto { Id = id })
            });

            AuthorizationContext.CurrentUserId = id;
        }
    }
}
