using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using demo.DDD;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Tests.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Entity
{
    [Collection("ServicesFixture")]
    public class ContactEntityTest
    {
        private readonly ServiceProvider _provider;

        public ContactEntityTest(ServicesFixture fixture)
        {
            ServiceCollection serviceCollection = fixture.Services;
            _provider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async void AddContactToCall_MustBeAdded()
        {
            //action
            var contact = new Contact
            {
                Extension = "89997777771"
            };

            await SaveContact(contact);

            //Assert
            var contactFromDb = await GetContact(contact.Id);
            contactFromDb.ShouldNotBeNull();
            (contactFromDb is Contact).ShouldBeTrue();
        }

        private async Task SaveContact(Contact contact)
        {
            using var unitOfWork = _provider.GetService<UnitOfWork>();
            var contactRepository = new ContactRepository(unitOfWork);
            unitOfWork.Begin();
            await contactRepository.Add(contact);
            await unitOfWork.CommitAsync();
        }

        private async Task<Participant> GetContact(Guid id)
        {
            using var unitOfWork = _provider.GetService<UnitOfWork>();
            var participantRepository = new ParticipantRepository(unitOfWork);
            unitOfWork.Begin();
            return await participantRepository.GetById(id);
        }
    }
}
