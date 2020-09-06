using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using demo.DDD;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Tests.Core;
using Xunit;
using Xunit.Abstractions;

namespace demo.DemoApi.Service.Tests.Entity
{
    [Collection("ServicesFixture")]
    public class TrackableEntityTest : IDisposable
    {
        private readonly ServiceProvider _provider;

        /// <inheritdoc />
        public TrackableEntityTest(ITestOutputHelper output, ServicesFixture fixture)
        {
            ServiceCollection serviceCollection = fixture.Services;

            serviceCollection.AddScoped<UnitOfWork>();
            serviceCollection.AddTransient<ICaseFolderRepository, CaseFolderRepository>();
            serviceCollection.AddTransient<ICaseRepository, CaseRepository>();
            serviceCollection.AddTransient<ICaseTypeRepository, CaseTypeRepository>();
            serviceCollection.AddTransient<ICaseRepository, CaseRepository>();

            _provider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task TrackingEvents_AddCaseFolder_Success()
        {
            var unitOfWork = _provider.GetService<UnitOfWork>();
            var caseFolderRepository = _provider.GetService<ICaseFolderRepository>();
            var caseTypeRepository = _provider.GetService<ICaseTypeRepository>();

            CaseFolder caseFolder;
            using (unitOfWork.Begin())
            {
                caseFolder = new CaseFolder();
                caseFolder.AddCaseCard(caseTypeRepository.GetFirst(), Guid.NewGuid());
                await caseFolderRepository.Add(caseFolder);
                await unitOfWork.CommitAsync();
            }

            using (unitOfWork.Begin())
            {
                CaseFolder fromDb = await caseFolderRepository.GetById(caseFolder.Id);

                Assert.NotNull(fromDb);
                Assert.Equal(caseFolder.Id, fromDb.Id);
                Assert.NotNull(fromDb.Cases);
                Assert.True(fromDb.Cases.Any());
                Assert.True(fromDb.TrackingEvents.Any());
            }
        }

        [Fact]
        public async Task TrackingEvents_UpdateCase_Success()
        {
            var unitOfWork = _provider.GetService<UnitOfWork>();
            var caseFolderRepository = _provider.GetService<ICaseFolderRepository>();
            var caseRepository = _provider.GetService<ICaseRepository>();
            var caseTypeRepository = _provider.GetService<ICaseTypeRepository>();

            Guid caseId;
            CaseFolder caseFolder;
            using (unitOfWork.Begin())
            {
                caseFolder = new CaseFolder();
                caseFolder.AddCaseCard(caseTypeRepository.GetFirst(), Guid.NewGuid());
                await caseFolderRepository.Add(caseFolder);
                caseId = caseFolder.Cases.FirstOrDefault().Id;
                await unitOfWork.CommitAsync();
            }

            using (unitOfWork.Begin())
            {
                var caseFromDb = await caseRepository.GetById(caseId);
                caseFromDb.Status = "Closed";
                await unitOfWork.CommitAsync();
            }

            using (unitOfWork.Begin())
            {
                CaseFolder caseFolderFromDb = await caseFolderRepository.GetById(caseFolder.Id);
                var caseCard = caseFolderFromDb.Cases.First();

                Assert.Equal(4, caseFolderFromDb.TrackingEvents.Count);
                Assert.Equal(3, caseCard.Root.TrackingEvents.Count(t => t.OperationType == EventType.Inserted));
                Assert.Equal(1, caseCard.Root.TrackingEvents.Count(t => t.OperationType == EventType.Updated));
            }
        }

        [Fact]
        public async Task TrackingEvents_UpdateBaseEntity_Success()
        {
            var unitOfWork = _provider.GetService<UnitOfWork>();
            var caseRepository = _provider.GetService<ICaseRepository>();
            var caseTypeRepository = _provider.GetService<ICaseTypeRepository>();

            Case caseCard;
            using (unitOfWork.Begin())
            {
                var caseTypeFromDb = caseTypeRepository.GetFirst();
                var caseFolder = new CaseFolder();
                caseFolder.AddCaseCard(caseTypeFromDb, Guid.NewGuid());
                caseCard = caseFolder.Cases.FirstOrDefault();
                await caseRepository.Add(caseCard);

                await unitOfWork.CommitAsync();
            }

            using (unitOfWork.Begin())
            {
                caseCard = await caseRepository.GetById(caseCard.Id);
                caseCard.Updated = DateTime.Now;
                await unitOfWork.CommitAsync();
            }

            using (unitOfWork.Begin())
            {
                caseCard = await caseRepository.GetById(caseCard.Id);

                Assert.NotNull(caseCard.Root);
                Assert.Equal(1, caseCard.CaseFolder.TrackingEvents.Count(t => t.OperationType == EventType.Updated));
                Assert.Equal(3, caseCard.CaseFolder.TrackingEvents.Count(t => t.OperationType == EventType.Inserted));
            }
        }

        public void Dispose()
        {
        }
    }
}
