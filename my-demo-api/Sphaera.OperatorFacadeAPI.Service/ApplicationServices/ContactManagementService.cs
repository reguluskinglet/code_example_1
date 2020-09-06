using System.Threading.Tasks;
using AutoMapper;
using demo.ContactManagement.Client;
using demo.ContactManagement.HttpContracts.Dto;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos.Contact;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для работы с контактами
    /// </summary>
    public class ContactManagementService
    {
        private readonly ContactManagementServiceClient _contactManagementClient;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public ContactManagementService(ContactManagementServiceClient contactManagementClient, ILogger logger, IMapper mapper)
        {
            _contactManagementClient = contactManagementClient;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить страницу контактов
        /// </summary>
        public async Task<Result<ContactsPageDto>> GetContactsPage(string filter, int pageSize, int pageIndex)
        {
            Result<ContactPageClientDto> result = await _contactManagementClient.GetContactsPage(filter, pageSize, pageIndex);
            if (result.IsFailure)
            {
                var message = $"ContactManagementService. Не удалось получить ContactPage из ContactManagementService. filter: {filter}; pageSize: {pageSize} pageIndex: {pageIndex}";
                _logger.Warning($"{message}. {result.ErrorMessage}");
                return Result.Failure<ContactsPageDto>(ErrorCodes.UnableToGetContactsPage);
            }

            var dto = _mapper.Map<ContactsPageDto>(result.Value);
            return Result.Success(dto);
        }
    }
}
