using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using demo.FunctionalExtensions;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.InboxDistribution.HttpContracts.Enums;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Inbox;
using demo.DemoApi.Service.Dtos.Receiver;
using demo.DemoApi.Service.Dtos.Role;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис очередей.
    /// </summary>
    public class InboxService
    {
        private readonly InboxDistributionServiceClient _inboxDistributionServiceClient;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор для инъекции зависимостей.
        /// </summary>
        public InboxService(
            InboxDistributionServiceClient inboxDistributionServiceClient,
            ILogger logger,
            IMapper mapper)
        {
            _inboxDistributionServiceClient = inboxDistributionServiceClient;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение очередей пользователя со списком всех элементов
        /// </summary>
        public async Task<Result<InboxesResponseDto>> GetInboxes(List<Guid> roleIds)
        {
            Result<List<InboxClientDto>> inboxDataListResult = await _inboxDistributionServiceClient.GetInboxes(roleIds);
            if (inboxDataListResult.IsFailure)
            {
                _logger.Warning($"InboxService.GetInboxes. Error getting inboxes. {inboxDataListResult.ErrorMessage}");
                return Result.Failure<InboxesResponseDto>(ErrorCodes.UnableToGetInbox);
            }

            List<InboxDto> inboxDataList = inboxDataListResult.Value
                .Select(inbox => new InboxDto
                {
                    Name = inbox.Name,
                    InboxId = inbox.InboxId,
                    ItemsCount = inbox.Items.Count,
                    RegulatoryTimeStatus = inbox.RegulatoryTimeStatus.ToString(),
                    OldestInboxItem = inbox.Items.OrderByDescending(dto => dto.ArrivalDateTime).Select(x => GetInboxItem(x)).FirstOrDefault(),
                    InboxItems = inbox.Items.Select(GetInboxItem).ToList()
                })
                .ToList();

            return Result.Success(new InboxesResponseDto
            {
                Inboxes = inboxDataList
            });
        }

        /// <summary>
        /// Получение очередей пользователя со списком всех элементов
        /// </summary>
        public async Task<Result<InboxDto>> GetInbox(Guid inboxId)
        {
            Result<InboxClientDto> inboxDataResult = await _inboxDistributionServiceClient.GetInbox(inboxId);
            if (inboxDataResult.IsFailure)
            {
                _logger.Warning($"InboxService.GetInbox. Error getting inbox. {inboxDataResult.ErrorMessage}");
                return Result.Failure<InboxDto>(ErrorCodes.UnableToGetInbox);
            }

            var inbox = inboxDataResult.Value;

            var inboxDto = new InboxDto
            {
                Name = inbox.Name,
                InboxId = inbox.InboxId,
                ItemsCount = inbox.Items.Count,
                RegulatoryTimeStatus = inbox.RegulatoryTimeStatus.ToString(),
                OldestInboxItem = inbox.Items.Select(x => GetInboxItem(x)).FirstOrDefault(),
                InboxItems = inbox.Items.Select(GetInboxItem).ToList()
            };

            return Result.Success(inboxDto);
        }

        /// <summary>
        /// Получить список ролей, назначенных пользователю
        /// </summary>
        public async Task<Result<List<RoleDto>>> GetUserRoles()
        {
            Result<List<RoleClientDto>> rolesListResult = await _inboxDistributionServiceClient.GetRoles();
            if (rolesListResult.IsFailure)
            {
                _logger.Warning($"InboxService.GetUserRoles. Error getting user roles. {rolesListResult.ErrorMessage}");
                return Result.Failure<List<RoleDto>>(ErrorCodes.UnableToGetRoles);
            }

            var result = _mapper.Map<List<RoleDto>>(rolesListResult.Value);
            return Result.Success(result);
        }

        /// <summary>
        /// Получить список рабочих заданий, доступных пользователю
        /// </summary>
        public async Task<Result<List<WorkChoiceDto>>> GetUserWorkChoices()
        {
            Result<List<WorkChoiceClientDto>> workChoicesResult = await _inboxDistributionServiceClient.GetWorkChoices();
            if (workChoicesResult.IsFailure)
            {
                _logger.Warning($"InboxService.GetUserWorkChoices. Error getting workChoices. {workChoicesResult.ErrorMessage}");
                return Result.Failure<List<WorkChoiceDto>>(ErrorCodes.UnableToGetWorkChoices);
            }

            var result = _mapper.Map<List<WorkChoiceDto>>(workChoicesResult.Value);
            return Result.Success(result);
        }

        /// <summary>
        /// Получить списки ресиверов для подключения к разговору
        /// </summary>
        public async Task<Result<ReceiversResponseDto>> GetConnectionReceivers()
        {
            Result<List<ReceiverClientDto>> receiversResult = await _inboxDistributionServiceClient.GetConnectionReceivers();
            if (receiversResult.IsFailure)
            {
                _logger.Warning($"InboxService.GetConnectionReceivers. Error getting connection receivers. {receiversResult.ErrorMessage}");
                return Result.Failure<ReceiversResponseDto>(ErrorCodes.UnableToGetReceivers);
            }

            List<ReceiverClientDto> allReceivers = receiversResult.Value;
            List<ReceiverClientDto> singleReceivers = allReceivers.Where(x => x.Type == ClientReceiverType.Single).ToList();
            List<ReceiverClientDto> groupReceivers = allReceivers.Where(x => x.Type == ClientReceiverType.Group).ToList();
            var response = new ReceiversResponseDto
            {
                SingleReceivers = _mapper.Map<List<ReceiverDto>>(singleReceivers),
                GroupReceivers = _mapper.Map<List<ReceiverDto>>(groupReceivers)
            };

            return Result.Success(response);
        }

        private static InboxItemDto GetInboxItem(InboxItemClientDto inboxItemClientDto)
        {
            return new InboxItemDto
            {
                ItemId = inboxItemClientDto.ItemId,
                ArrivalTime = inboxItemClientDto.ArrivalDateTime,
                Caller = new ParticipantInfoDto
                {
                    Extension = inboxItemClientDto.CreatorExtension,
                    ParticipantInfo = inboxItemClientDto.CreatorExtension
                }
            };
        }
    }
}
