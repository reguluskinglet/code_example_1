using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using demo.DDD;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.DAL.Migrations;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Tests.Core;
using Xunit;
using Case = demo.DemoApi.Domain.Entities.Case;

namespace demo.DemoApi.Service.Tests.Repositories
{
    [Collection("ServicesFixture")]
    public class CaseRepositoryTest
    {
        private ServiceProvider _provider;

        public CaseRepositoryTest(ServicesFixture fixture)
        {
            ServiceCollection serviceCollection = fixture.Services;

            _provider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async void Add_CaseWithoutCall_SuccessfullySaved()
        {
            //Arrange
            var unitOfWork = _provider.GetService<UnitOfWork>();
            var caseCardRepository = _provider.GetService<ICaseRepository>();
            var caseCardTypeRepository = _provider.GetService<ICaseTypeRepository>();
            Guid case112TypeId = new Guid(AddCaseTableAndCaseTemplateTable.CaseTypeId112);

            //Action
            CaseType caseType;
            using (unitOfWork.Begin())
            {
                caseType = await caseCardTypeRepository.GetById(case112TypeId);
                await caseCardTypeRepository.Add(caseType);
                await unitOfWork.CommitAsync();
            }

            Case caseCard;
            using (unitOfWork.Begin())
            {
                var caseTypeFromDb = await caseCardTypeRepository.GetById(caseType.Id);
                var caseFolder = new CaseFolder();
                caseFolder.AddCaseCard(caseTypeFromDb, Guid.NewGuid());
                caseCard = caseFolder.Cases.FirstOrDefault();
                await caseCardRepository.Add(caseCard);
                await unitOfWork.CommitAsync();
            }

            //Assert
            using (unitOfWork.Begin())
            {
                var caseCardFromDb = await caseCardRepository.GetById(caseCard.Id);
                caseCardFromDb.ShouldNotBeNull();
                caseCardFromDb.Type.ShouldNotBeNull();
            }
        }
    }
}
