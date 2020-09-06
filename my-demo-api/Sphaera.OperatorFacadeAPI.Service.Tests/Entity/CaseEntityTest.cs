using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using demo.DDD;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Tests.Core;
using Xunit;
using Case = demo.DemoApi.Domain.Entities.Case;

namespace demo.DemoApi.Service.Tests.Entity
{
    [Collection("ServicesFixture")]
    public class CaseEntityTest
    {
        private readonly ServiceProvider _provider;

        public CaseEntityTest(ServicesFixture fixture)
        {
            ServiceCollection serviceCollection = fixture.Services;

            _provider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async void Remove_CaseFolder_CaseMustBeRemoved()
        {
            //Arrange
            var caseType = GetCaseType();
            var caseFolder = new CaseFolder();
            caseFolder.AddCaseCard(caseType, new Guid());
            var caseCard = caseFolder.Cases.FirstOrDefault();
            await SaveCaseFolder(caseFolder);

            //action
            await RemoveEntity(caseFolder);
            var result = await GetEntity<Case>(x => x.Id == caseCard.Id);

            //arrange
            result.ShouldBeNull();
        }

        private CaseType GetCaseType()
        {
            using var unitOfWork = _provider.GetService<UnitOfWork>();
            unitOfWork.Begin();
            var caseRepo = new CaseTypeRepository(unitOfWork);
            return caseRepo.GetFirst();
        }

        private async Task SaveCaseFolder(CaseFolder caseFolder)
        {
            var unitOfWork = _provider.GetScopedService<UnitOfWork>();

            using (unitOfWork.Begin())
            {
                var caseFolderRepository = new CaseFolderRepository(unitOfWork);
                await caseFolderRepository.Add(caseFolder);
                await unitOfWork.CommitAsync();
            }
        }

        private async Task RemoveEntity<T>(T entity)
        {
            using var unitOfWork = _provider.GetService<UnitOfWork>();
            unitOfWork.Begin();
            await unitOfWork.DeleteAsync(entity);
            await unitOfWork.CommitAsync();
        }

        private async Task<T> GetEntity<T>(Expression<Func<T, bool>> predicate)
        {
            using var unitOfWork = _provider.GetService<UnitOfWork>();
            unitOfWork.Begin();
            return unitOfWork.Query<T>().FirstOrDefault(predicate);
        }
    }
}
