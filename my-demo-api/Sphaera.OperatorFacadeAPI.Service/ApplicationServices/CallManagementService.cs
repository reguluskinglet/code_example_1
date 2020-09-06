using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using demo.CallManagement.Client;
using demo.CallManagement.HttpContracts.Dto;
using demo.CallManagement.HttpContracts.Enums;
using demo.ContactManagement.Client;
using demo.ContactManagement.HttpContracts.Dto;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.InboxDistribution.HttpContracts.Enums;
using demo.MediaRecording.Client;
using demo.MediaRecording.Client.Dtos;
using demo.MessageContracts.InboxDistribution;
using demo.MessageContracts.InboxDistribution.Enums;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Entities.SmsMetadata;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices.Lines;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Calls;
using demo.DemoApi.Service.Dtos.Enums;
using demo.DemoApi.Service.Dtos.Line;
using demo.DemoApi.Service.Dtos.Participant;
using demo.UserManagement.Client;
using demo.UserManagement.HttpContracts.Dto;
using AcceptCallClientDto = demo.CallManagement.HttpContracts.Dto.AcceptCallClientDto;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для работы со звонками
    /// </summary>
    public class CallManagementService
    {
        private readonly CallManagementServiceClient _callManagementServiceClient;
        private readonly ContactManagementServiceClient _contactManagementClient;
        private readonly UserManagementServiceClient _userManagementServiceClient;
        private readonly ICaseTypeRepository _caseTypeRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly ICaseFolderRepository _caseFolderRepository;
        private readonly InboxDistributionServiceClient _inboxDistributionServiceClient;
        private readonly MediaRecordingServiceClient _mediaRecordingServiceClient;
        private readonly ISmsRepository _smsRepository;
        private readonly PhoneHubMessageService _phoneHubMessageService;
        private readonly ILogger _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public CallManagementService(
            UnitOfWork unitOfWork,
            CallManagementServiceClient callManagementServiceClient,
            ContactManagementServiceClient contactManagementClient,
            UserManagementServiceClient userManagementServiceClient,
            IParticipantRepository participantRepository,
            InboxDistributionServiceClient inboxDistributionServiceClient,
            MediaRecordingServiceClient mediaRecordingServiceClient,
            ICaseTypeRepository caseTypeRepository,
            ICaseFolderRepository caseFolderRepository,
            ISmsRepository smsRepository,
            PhoneHubMessageService phoneHubMessageService,
            IMapper mapper,
            ILogger logger)
        {
            _callManagementServiceClient = callManagementServiceClient;
            _contactManagementClient = contactManagementClient;
            _userManagementServiceClient = userManagementServiceClient;
            _participantRepository = participantRepository;
            _smsRepository = smsRepository;
            _inboxDistributionServiceClient = inboxDistributionServiceClient;
            _mediaRecordingServiceClient = mediaRecordingServiceClient;
            _caseTypeRepository = caseTypeRepository;
            _caseFolderRepository = caseFolderRepository;
            _phoneHubMessageService = phoneHubMessageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить журнал звонков
        /// </summary>
        public async Task<Result<List<JournalCallsDto>>> GetJournalCalls(Guid userId, CallTypeFilter filter, bool notAcceptedOnly)
        {
            var callClientTypeFilterResult = MapCallTypeEnums(filter);

            if (callClientTypeFilterResult.IsFailure)
            {
                _logger.Information($"Failed to get call journal by userId: {userId}, filter: {filter}, notAcceptedOnly: {notAcceptedOnly} в CallManagementService. {callClientTypeFilterResult.ErrorMessage}");
                return Result.Failure<List<JournalCallsDto>>(callClientTypeFilterResult.ErrorCode);
            }

            var callListClientResult = await _callManagementServiceClient.GetJournalCalls(callClientTypeFilterResult.Value, notAcceptedOnly);
            if (callListClientResult.IsFailure)
            {
                var name = Enum.GetName(typeof(CallClientTypeFilter), filter);

                var message = $"Failed to get call journal by userId: {userId}, filter: {name}, notAcceptedOnly: {notAcceptedOnly} в CallManagementService. {callListClientResult.ErrorMessage}";
                _logger.Information(message);
                return Result.Failure<List<JournalCallsDto>>(ErrorCodes.UnableToGetJournalCalls);
            }

            _unitOfWork.Begin();
            var journalCalls = await CreateJournalFromCallList(callListClientResult.Value);
            return Result.Success(journalCalls);
        }

        /// <summary>
        /// Принять элемент из очереди (например, звонок или смс)
        /// </summary>
        public async Task<Result> AcceptInboxItem(Guid userId, Guid inboxId, Guid? itemId)
        {
            _logger.Verbose($"AcceptInboxItem. InboxId: {inboxId}; ItemId: {itemId}");

            _unitOfWork.Begin();

            Result<UserItemClientDto> userItemResult = await GetUserItem(inboxId, itemId);

            if (userItemResult.IsFailure)
            {
                _logger.Warning($"Failed to get userItem by id {inboxId}. {userItemResult.ErrorMessage}");
                return Result.Failure(ErrorCodes.UnableToGetInbox);
            }

            Result<UserClientDto> currentUserResult = await _userManagementServiceClient.GetUserById(userId);
            if (currentUserResult.IsFailure)
            {
                _logger.Warning($"GetUserById. Error get user by Id. {currentUserResult.ErrorMessage}");
                return Result.Failure(ErrorCodes.UserNotFound);
            }

            var currentUser = _mapper.Map<UserDto>(currentUserResult.Value);

            var userItem = userItemResult.Value;

            Result acceptResult;
            if (userItem.ItemType == ClientInboxItemType.Sms)
            {
                acceptResult = await ProcessSms(currentUser, userItem);
            }
            else
            {
                acceptResult = await ProcessCall(currentUser, userItem);
            }

            await _phoneHubMessageService.NotifyUsersAboutInboxUpdate(inboxId);
            return acceptResult;
        }

        private async Task<Result<UserItemClientDto>> GetUserItem(Guid inboxId, Guid? itemId)
        {
            Result<UserItemClientDto> userItemResult;
            if (itemId.HasValue)
            {
                userItemResult = await _inboxDistributionServiceClient.AcceptItem(new AcceptItemClientDto
                {
                    InboxId = inboxId,
                    ItemId = itemId.Value
                });
            }
            else
            {
                userItemResult = await _inboxDistributionServiceClient.AcceptNextItem(new AcceptNextItemClientDto
                {
                    InboxId = inboxId
                });
            }

            return userItemResult;
        }

        /// <summary>
        /// Завершить все активные вызовы пользователя
        /// </summary>
        public async Task EndAllUserCalls(Guid userId)
        {
            using (_unitOfWork.Begin())
            {
                Result<UserClientDto> userResult = await _userManagementServiceClient.GetUserById(userId);
                if (userResult.IsFailure)
                {
                    _logger.Warning($"EndAllUserCalls. Error get user by Id. {userResult.ErrorMessage}");
                    return;
                }

                Result<List<EndedUserLineClientDto>> endResult = await _callManagementServiceClient.EndAllUserCalls(userId);
                if (endResult.IsFailure)
                {
                    _logger.Warning($"EndAllUserCalls. {endResult.ErrorMessage}");
                    return;
                }

                await NotifyAboutUserCallsEnded(endResult.Value);
            }
        }

        /// <summary>
        /// Завершить вызов.
        /// </summary>
        public async Task<Result> EndCallByUser(Guid callId)
        {
            var endResult = await _callManagementServiceClient.EndCallByUser(callId);
            if (endResult.IsSuccess)
            {
                await NotifyAboutUserCallsEnded(endResult.Value);
            }

            _logger.Debug($"User ended call with id {callId}.");

            return Result.Success();
        }

        /// <summary>
        /// Заявитель или другой внешний участник разговора разрывает соединение
        /// </summary>
        public async Task EndCallByExternalCaller(Guid externalCallId)
        {
            var endResult = await _callManagementServiceClient.EndCallByExternalCaller(externalCallId);
            await _inboxDistributionServiceClient.EndCall(new EndCallClientDto
            {
                CallId = externalCallId
            });
            if (endResult.IsFailure)
            {
                _logger.Warning($"EndCallByExternalCaller.  CallId: {externalCallId}. {endResult.ErrorMessage}");
                return;
            }

            await NotifyAboutUserCallsEnded(endResult.Value);
        }

        /// <summary>
        /// Заявитель отклонил или не ответил на вызов оператора
        /// </summary>
        public async Task RejectCallByExternalCaller(Guid externalCallId)
        {
            var endResult = await _callManagementServiceClient.RejectCallByExternalCaller(externalCallId);
            if (endResult.IsFailure)
            {
                _logger.Warning($"RejectCallByExternalCaller. CallId: {externalCallId}. {endResult.ErrorMessage}");
                return;
            }

            await NotifyAboutUserCallsEnded(endResult.Value);
        }

        /// <summary>
        /// Поменять роли главного оператора и ассистента.
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="fromUserId">Номер главного оператора</param>
        /// <param name="toUserId">Номер ассистента</param>
        /// <returns></returns>
        public async Task<Result> ExchangeOperatorRoles(Guid lineId, Guid fromUserId, Guid toUserId)
        {
            using (_unitOfWork.Begin())
            {
                var exchangeResult = await _callManagementServiceClient.ExchangeUserRoles(lineId, fromUserId, toUserId);
                if (exchangeResult.IsFailure)
                {
                    _logger.Warning($"ExchangeOperatorRoles. Error exchange roles. LineId: {lineId}. {exchangeResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToExchangeUserRoles);
                }
            }

            return Result.Success();
        }

        /// <summary>
        /// Поменять статус изоляции вызова.
        /// </summary>
        /// <param name="callId"> Id вызова.</param>
        /// <param name="isIsolated"> Вызов изолирован или нет. </param>
        /// <returns></returns>
        public async Task<Result> SetIsolationStatus(Guid callId, bool isIsolated)
        {
            var setIsolationStatusResult = await _callManagementServiceClient.SetIsolationStatus(callId, isIsolated);
            if (setIsolationStatusResult.IsFailure)
            {
                _logger.Warning($"SetIsolationStatus. {setIsolationStatusResult.ErrorMessage}");
                return Result.Failure(ErrorCodes.UnableToSetIsolationStatus);
            }

            return Result.Success();
        }

        /// <summary>
        /// Добавить вызов в очередь и сообщить об этом оператору.
        /// </summary>
        public async Task AddIncomingCall(IncomingInboxIntegrationEvent incomingInboxIntegrationEvent)
        {
            var itemId = incomingInboxIntegrationEvent.ItemId;

            if (itemId == default)
            {
                _logger.Warning($"Empty {nameof(itemId)}: {itemId}");
                return;
            }

            var extension = incomingInboxIntegrationEvent.CallerExtension;
            if (string.IsNullOrWhiteSpace(extension))
            {
                _logger.Warning($"AddIncomingCall. Empty CallerExtension. {nameof(itemId)}: {itemId}");
                return;
            }

            using UnitOfWork unitOfWork = _unitOfWork.Begin();

            bool isSms = incomingInboxIntegrationEvent.ContractInboxItemType == ContractInboxItemType.Sms;

            var smsMessageData = _mapper.Map<SmsMessageData>(incomingInboxIntegrationEvent.Sms);
            var caller = await GetCaller(extension);

            if (isSms)
            {
                var locationMetadata = JsonConvert.SerializeObject(smsMessageData.Location ?? new SmsLocationData { Position = new SmsPositionData() });
                var sms = new Sms(itemId)
                {
                    Status = SmsStatus.New,
                    ArrivalDateTime = incomingInboxIntegrationEvent.ArrivalDateTime,
                    Text = smsMessageData.Message,
                    Applicant = caller,
                    LocationMetadata = locationMetadata,
                    Timestamp = smsMessageData.Timestamp
                };

                await _smsRepository.Add(sms);
                await unitOfWork.CommitAsync();
                _logger.Debug($"Added incoming sms with id: {itemId}");
            }
            else
            {
                var result = await _callManagementServiceClient.AddIncomingCall(
                    itemId,
                    caller.Id,
                    incomingInboxIntegrationEvent.ArrivalDateTime);

                if (result.IsFailure)
                {
                    _logger.Error($"AddIncomingCall. {result.ErrorMessage}");
                    return;
                }

                await _participantRepository.Add(caller);
                await unitOfWork.CommitAsync();
            }

            await _phoneHubMessageService.NotifyUsersAboutInboxUpdate(incomingInboxIntegrationEvent.InboxId);
        }

        /// <summary>
        /// Добавить звонок в активную линию
        /// </summary>
        public async Task<Result> AddCallToLine(Guid lineId, Guid? caseFolderId, Guid fromUserId, string destination, ConnectionMode connectionMode)
        {
            using (_unitOfWork.Begin())
            {
                if (!Guid.TryParse(destination, out var toUserId))
                {
                    _logger.Error($"AddCallToLine. Destination for connection user to line incorrect. LineId: {lineId}. Destination: {destination}");
                    return Result.Failure(ErrorCodes.ValidationError);
                }

                Result<UserClientDto> fromUserResult = await _userManagementServiceClient.GetUserById(fromUserId);
                if (fromUserResult.IsFailure)
                {
                    _logger.Warning($"AddCallToLine. Error get current user by Id. {fromUserResult.ErrorMessage}. UserId: {fromUserId}");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                Result<UserClientDto> toUserResult = await _userManagementServiceClient.GetUserById(toUserId);
                if (toUserResult.IsFailure)
                {
                    _logger.Warning($"AddCallToLine. Error get destination user by Id. {toUserResult.ErrorMessage}. UserId: {toUserId}");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                var fromUserExtension = fromUserResult.Value.Extension;

                Result<NewCallDto> newCallResult = await _callManagementServiceClient.AddCallToLine(
                    new AddCallToLineClientDto
                    {
                        LineId = lineId,
                        FromUserId = fromUserId,
                        ToUserId = toUserId,
                        ConnectionMode = (ClientConnectionMode)connectionMode
                    });

                if (newCallResult.IsFailure)
                {
                    _logger.Warning($"AddCallToLine. {newCallResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToAddCallToLine);
                }

                var newCall = newCallResult.Value;

                var inboxResult = await _inboxDistributionServiceClient.CallToUser(new CallToUserClientDto
                {
                    CallId = newCall.Id,
                    UserId = toUserId.ToString(),
                    CallerExtension = fromUserExtension,
                    InboxItemType = ClientInboxItemType.Call
                });

                if (inboxResult.IsFailure)
                {
                    _logger.Warning($"AddCallToLine. InboxDistribution.CallToUser. {inboxResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToAddCallToLine);
                }

                await _unitOfWork.CommitAsync();

                await NotifyAboutCallAddedToLine(caseFolderId);

                return Result.Success();
            }
        }

        /// <summary>
        /// Направить звонок группе пользователей
        /// </summary>
        public async Task<Result> AddUserGroupCallToLine(Guid lineId, Guid? caseFolderId, Guid fromUserId, string destination, ConnectionMode connectionMode)
        {
            using (_unitOfWork.Begin())
            {
                Result<UserClientDto> fromUserResult = await _userManagementServiceClient.GetUserById(fromUserId);
                if (fromUserResult.IsFailure)
                {
                    _logger.Warning($"AddUserGroupCallToLine. Error get user by Id. {fromUserResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                var fromUserExtension = fromUserResult.Value.Extension;

                Result<NewCallDto> newCallResult = await _callManagementServiceClient.AddUserGroupCallToLine(
                    new AddUserGroupToLineClientDto
                    {
                        LineId = lineId,
                        FromUserId = fromUserId,
                        CaseFolderId = caseFolderId,
                        ConnectionMode = (ClientConnectionMode)connectionMode
                    });

                if (newCallResult.IsFailure)
                {
                    _logger.Warning($"AddUserGroupCallToLine. {newCallResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToAddUserGroupCallToLine);
                }

                var newCall = newCallResult.Value;

                var inboxResult = await _inboxDistributionServiceClient.CallToGroup(new CallToGroupClientDto
                {
                    CallId = newCall.Id,
                    GroupName = destination,
                    CallerExtension = fromUserExtension,
                    InboxItemType = ClientInboxItemType.Call
                });

                if (inboxResult.IsFailure)
                {
                    _logger.Warning($"AddUserGroupCallToLine. InboxDistribution.CallToGroup. {inboxResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToAddUserGroupCallToLine);
                }

                await _unitOfWork.CommitAsync();

                await NotifyAboutCallAddedToLine(caseFolderId);

                return Result.Success();
            }
        }

        /// <summary>
        /// Изменить состояние микрофона.
        /// </summary>
        public async Task<Result> MicrophoneChangeState(Guid lineId, Guid callId, bool isMuted)
        {
            var microphoneChangeStateResult = await _callManagementServiceClient.MicrophoneChangeState(lineId, callId, isMuted);

            if (microphoneChangeStateResult.IsFailure)
            {
                _logger.Warning(microphoneChangeStateResult.ErrorMessage);
                return Result.Failure(ErrorCodes.UnableToMicrophoneChangeState);
            }

            return Result.Success();
        }

        /// <summary>
        /// Установка статуса удержания
        /// </summary>
        /// <param name="callId">Id звонка, для которого изменяется статус удержания</param>
        /// <param name="hold">Статус, который нужно выставить</param>
        /// <returns></returns>
        public async Task<Result> SetHoldStatus(Guid callId, bool hold)
        {
            var setHoldStatusResult = await _callManagementServiceClient.SetHoldStatus(callId, hold);
            if (setHoldStatusResult.IsFailure)
            {
                _logger.Warning(setHoldStatusResult.ErrorMessage);
                return Result.Failure(ErrorCodes.UnableToSetHoldStatus);
            }

            return Result.Success();
        }

        /// <summary>
        /// Перезвонить заявителю
        /// </summary>
        public async Task<Result<CallBackToApplicantStatus>> CallBackToApplicant(Guid userId, CallBackToApplicantDto model)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolderId = model.CaseFolderId;
                CaseFolder caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<CallBackToApplicantStatus>(ErrorCodes.CaseFolderNotFound);
                }

                var participantResult = await GetApplicant(caseFolder);
                if (participantResult.IsFailure)
                {
                    _logger.Warning(participantResult.ErrorMessage);
                    return Result.Failure<CallBackToApplicantStatus>(participantResult.ErrorCode);
                }

                Result<UserClientDto> userResult = await _userManagementServiceClient.GetUserById(userId);
                if (userResult.IsFailure)
                {
                    _logger.Warning($"CallBackToApplicant. Error get user by Id. {userResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                var user = _mapper.Map<UserDto>(userResult.Value);

                var clientModel = new CallBackToApplicantClientDto
                {
                    UserExtension = user.Extension,
                    ParticipantExtension = participantResult.Value.ParticipantExtension,
                    CaseFolderId = caseFolderId,
                };
                var result = await _callManagementServiceClient.CallBackToApplicant(clientModel);
                if (result.IsFailure)
                {
                    _logger.Warning($"CallBackToApplicant. {result.ErrorMessage}");
                    return Result.Failure<CallBackToApplicantStatus>(ErrorCodes.UnableToCallBackToApplicant);
                }

                await _unitOfWork.CommitAsync();

                await _phoneHubMessageService.NotifyUserAboutStartedExternalCall(userId, result.Value.LineId);
                return Result.Success(CallBackToApplicantStatus.Ok);
            }
        }

        /// <summary>
        /// Позвонить на указанный номер.
        /// </summary>
        public async Task<Result<CallBackToApplicantStatus>> CallToNumber(Guid userId, CallToNumberDto model)
        {
            using (_unitOfWork.Begin())
            {
                Result<UserClientDto> userResult = await _userManagementServiceClient.GetUserById(userId);
                if (userResult.IsFailure)
                {
                    _logger.Warning($"CallToNumber. Error get user by Id. {userResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                var user = _mapper.Map<UserDto>(userResult.Value);
                var contactDtoResult = await _contactManagementClient.GetContactByNumber(model.Extension);
                Participant participant;
                if (contactDtoResult.IsFailure)
                {
                    _logger.Warning($"CallToNumber. Error getting contact. {contactDtoResult.ErrorMessage}");
                    participant = new Applicant(model.Extension);
                }
                else
                {
                    var contactDto = contactDtoResult.Value;
                    var routeName = contactDto.Phones?.FirstOrDefault(x => x.Number == model.Extension)?.ContactRouteName;
                    participant = new Contact(model.Extension, contactDto.Name, contactDto.Organization, contactDto.Position, routeName);
                }

                await _participantRepository.Add(participant);

                var clientModel = new CallToNumberClientDto
                {
                    ParticipantId = participant.Id,
                    UserExtension = user.Extension,
                    ParticipantExtension = model.Extension
                };

                var result = await _callManagementServiceClient.CallToNumber(clientModel);
                if (result.IsFailure)
                {
                    _logger.Warning($"CallToNumber. {result.ErrorMessage}");
                    return Result.Failure<CallBackToApplicantStatus>(ErrorCodes.UnableToCallToNumber);
                }

                await _unitOfWork.CommitAsync();

                await _phoneHubMessageService.NotifyUserAboutStartedExternalCall(userId, result.Value.LineId);
                return Result.Success(CallBackToApplicantStatus.Ok);
            }
        }

        /// <summary>
        /// Обработка исходящего звонка, принятого внешним участником
        /// </summary>
        public async Task ProcessAcceptedCallFromUser(Guid externalCallId)
        {
            var result = await _callManagementServiceClient.ProcessAcceptedCallFromUser(externalCallId);
            if (result.IsFailure)
            {
                _logger.Warning($"ProcessAcceptedCallFromUser. {result.ErrorMessage}");
                return;
            }

            var callDto = result.Value;
            await _phoneHubMessageService.NotifyUsersAboutExternalCallAccepted(callDto.Line.Id, callDto.Line.CaseFolderId, result.Value.ParticipantId.Value);
        }

        /// <summary>
        /// Отправить оповещения о добавлении, удалении или изменении сущности.
        /// </summary>
        public async Task NotifyAboutEntityChanged(IEnumerable<TrackingEvent> trackingEvents)
        {
            await _phoneHubMessageService.NotifyUsersAboutEntityChanged(trackingEvents);
        }

        /// <summary>
        /// СФормировать список участников разговора.
        /// </summary>
        public async Task<Result<List<CallRecordInfoDto>>> GenerateCallRecordsList(Guid lineId)
        {
            Result<List<CallTimeInfoClientDto>> result = await _callManagementServiceClient.GetCallsByLineId(lineId);
            if (result.IsFailure)
            {
                _logger.Warning($"GenerateParticipantsList. {result.ErrorMessage}");
                return Result.Failure<List<CallRecordInfoDto>>(ErrorCodes.UnableToGetCallsInLine);
            }

            Result<List<AudioRecordClientDto>> recordsResult = await _mediaRecordingServiceClient.GetAudioRecords(lineId);
            if (recordsResult.IsFailure)
            {
                _logger.Warning($"GenerateParticipantsList. {recordsResult.ErrorMessage}");
                return Result.Failure<List<CallRecordInfoDto>>(ErrorCodes.UnableToGetAudioRecords);
            }

            List<AudioRecordClientDto> records = recordsResult.Value;
            List<CallTimeInfoClientDto> participantCalls = result.Value;
            if (!participantCalls.Any())
            {
                _logger.Warning($"GenerateParticipantsList. Completed calls not found. LineId: {lineId}");
                return Result.Failure<List<CallRecordInfoDto>>(ErrorCodes.UnableToGetAudioRecords);
            }

            Result<List<CallRecordInfoDto>> callRecordsListResult = await GetRecordsList(lineId, records, participantCalls);
            if (callRecordsListResult.IsFailure)
            {
                _logger.Warning($"GenerateParticipantsList. Failed to get records list. LineId: {lineId}");
                return Result.Failure<List<CallRecordInfoDto>>(callRecordsListResult.ErrorCode);
            }

            return Result.Success(callRecordsListResult.Value);
        }

        private async Task<Result> ProcessCall(UserDto currentUser, UserItemClientDto userItem)
        {
            Result<Guid> caseFolderIdResult = await PrepareCase(userItem);
            if (caseFolderIdResult.IsFailure)
            {
                return Result.Failure(caseFolderIdResult.ErrorCode);
            }

            Result<CallClientDto> acceptCallResult = await _callManagementServiceClient.AcceptCall(
                new AcceptCallClientDto
                {
                    CallId = userItem.ItemId,
                    UserExtension = currentUser.Extension,
                    CaseFolderId = caseFolderIdResult.Value
                });

            if (acceptCallResult.IsFailure)
            {
                _logger.Warning($"AcceptInboxItem. Error accepting call. {acceptCallResult.ErrorMessage}");
                await _phoneHubMessageService.NotifyUsersAboutCallEnded(currentUser.Id, null);
                return Result.Failure(ErrorCodes.UnableToAcceptCall);
            }

            var call = acceptCallResult.Value;

            await _unitOfWork.CommitAsync();

            await _phoneHubMessageService.NotifyUsersAboutAcceptedCall(currentUser.Id, call.Line.Id, call.Line.CaseFolderId);

            return Result.Success();
        }

        private async Task<Result<Guid>> PrepareCase(UserItemClientDto userItem)
        {
            Result<CallClientDto> callInfoResult = await _callManagementServiceClient.GetCallById(userItem.ItemId);
            if (callInfoResult.IsFailure)
            {
                _logger.Error($"ProcessCall. {callInfoResult.ErrorMessage}");
                return Result.Failure(callInfoResult.ErrorCode, callInfoResult.ErrorMessage);
            }

            var caseFolderIdResult = callInfoResult.Value.Line.CaseFolderId;

            if (!userItem.CreateCaseCard)
            {
                if (caseFolderIdResult.HasValue)
                {
                    return Result.Success(caseFolderIdResult.Value);
                }

                var newEmptyCaseFolder = new CaseFolder();
                await _caseFolderRepository.Add(newEmptyCaseFolder);
                return Result.Success(newEmptyCaseFolder.Id);
            }

            var caseType = await _caseTypeRepository.GetById(userItem.CaseTypeId);
            if (caseFolderIdResult.HasValue)
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderIdResult.Value);
                caseFolder.AddCaseCard(caseType, userItem.UserId);
                return Result.Success(caseFolderIdResult.Value);
            }

            var newCaseFolder = new CaseFolder();
            newCaseFolder.AddCaseCard(caseType, userItem.UserId);
            await _caseFolderRepository.Add(newCaseFolder);
            return Result.Success(newCaseFolder.Id);
        }

        private async Task<Result> ProcessSms(UserDto currentUser, UserItemClientDto userItem)
        {
            Sms sms = await _smsRepository.GetById(userItem.ItemId);
            if (sms.Status == SmsStatus.Accepted)
            {
                _logger.Warning("SMS already accepted");
                return Result.Failure(ErrorCodes.SMSAlreadyTaken);
            }

            var caseType = await _caseTypeRepository.GetById(userItem.CaseTypeId);
            if (caseType == null)
            {
                _logger.Warning($"CaseType with Id {userItem.CaseTypeId} not found");
                return Result.Failure(ErrorCodes.CaseTypeNotFound);
            }

            Result<CaseFolder> result = AcceptSms(sms, caseType, currentUser.Id);
            if (result.IsFailure)
            {
                _logger.Warning($"Failed to receive SMS.. {result.ErrorMessage}");
                return Result.Failure(result.ErrorCode);
            }

            var caseFolder = result.Value;
            await _caseFolderRepository.Add(caseFolder);

            await _unitOfWork.CommitAsync();

            await _phoneHubMessageService.NotifyUsersAboutAcceptedSms(caseFolder.Id, currentUser.Id);

            return Result.Success();
        }

        /// <summary>
        /// Принять СМС
        /// </summary>
        /// <param name="sms"></param>
        /// <param name="caseType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private Result<CaseFolder> AcceptSms(Sms sms, CaseType caseType, Guid userId)
        {
            var applicantExt = sms.Applicant.ParticipantExtension;
            sms.Status = SmsStatus.Accepted;

            var caseFolder = new CaseFolder { Sms = sms };
            caseFolder.AddCaseCard(caseType, userId);

            var result = caseFolder.FillSmsData(applicantExt, sms);
            if (result.IsFailure)
            {
                return result;
            }

            return Result.Success(caseFolder);
        }

        private async Task<Result<Participant>> GetApplicant(CaseFolder caseFolder)
        {
            if (caseFolder.Sms != null)
            {
                return Result.Success(caseFolder.Sms.Applicant);
            }

            Result<List<LineByCaseFolderClientDto>> linesResult = await _callManagementServiceClient.GetLinesByCaseFolderId(caseFolder.Id);
            if (linesResult.IsFailure)
            {
                _logger.Warning($"GetApplicant. Failed to get line by caseFolderId: {caseFolder.Id}");
                return Result.Failure<Participant>(ErrorCodes.UnableToCallBackToApplicant);
            }

            var lines = _mapper.Map<List<LineDto>>(linesResult.Value);
            if (lines == null)
            {
                _logger.Warning($"Lines not found for CaseFolder: {caseFolder.Id}");
                return Result.Failure<Participant>(ErrorCodes.LineNotFound);
            }

            var callerId = lines.FirstOrDefault(l => l.ExternalCallerId.HasValue)?.ExternalCallerId;
            if (callerId == null)
            {
                _logger.Warning("Error getting applicant call from CaseFolder");
                return Result.Failure<Participant>(ErrorCodes.UnableToGetApplicant);
            }

            var participant = await GetParticipant(callerId.Value);
            if (participant == null)
            {
                _logger.Warning($"Participant not found. Id: {callerId}");
                return Result.Failure<Participant>(ErrorCodes.UserNotFound);
            }

            return Result.Success(participant);
        }

        private async Task<Participant> GetParticipant(Guid participantId)
        {
            var user = await _userManagementServiceClient.GetUserById(participantId);
            if (user.IsSuccess && user.Value != null)
            {
                var userClientDto = user.Value;
                return new User(userClientDto.Id, userClientDto.Extension, $"{userClientDto.FirstName} {userClientDto.LastName}");
            }

            var participant = await _participantRepository.GetById(participantId);
            return participant;
        }

        private async Task<Participant> GetCaller(string extension)
        {
            Result<ContactClientDto> contactResult = await _contactManagementClient.GetContactByNumber(extension);
            if (contactResult.IsFailure)
            {
                _logger.Debug($"Failed to get Contact from ContactManagementService. Extension: {extension}. {contactResult.ErrorMessage}");
                return new Applicant(extension);
            }

            var contactDto = contactResult.Value;
            var routeName = contactDto.Phones?.FirstOrDefault(x => x.Number == extension)?.ContactRouteName;
            return new Contact(extension, contactDto.Name, contactDto.Organization, contactDto.Position, routeName);
        }

        private async Task NotifyAboutUserCallsEnded(IList<EndedUserLineClientDto> endedUserLines)
        {
            foreach (EndedUserLineClientDto endedLine in endedUserLines)
            {
                await _phoneHubMessageService.NotifyUsersAboutCallEnded(endedLine.UserId, endedLine.LineId);
            }
        }

        private async Task NotifyAboutCallAddedToLine(Guid? caseFolderId)
        {
            await _phoneHubMessageService.NotifyAboutNeedToUpdateCaseTitlesAsync(caseFolderId);
        }

        private async Task<List<JournalCallsDto>> CreateJournalFromCallList(List<CallListItemClientDto> callList)
        {
            var journalCalls = new List<JournalCallsDto>();

            foreach (var callItem in callList)
            {
                Participant participant = await GetCallParticipant(callItem);
                var journalCallItem = JournalCallsDto.MapFromClientCall(callItem, participant);
                journalCalls.Add(journalCallItem);
            }

            return journalCalls;
        }

        private async Task<Participant> GetCallParticipant(CallListItemClientDto callItem)
        {
            Guid? participantId;
            switch (callItem.Type)
            {
                case CallClientType.Incoming:
                    participantId = callItem.CallerId;
                    break;
                case CallClientType.Missed when callItem.CallerId.HasValue:
                    participantId = callItem.CallerId.Value;
                    break;
                default:
                    participantId = callItem.ParticipantId;
                    break;
            }

            Participant participant = null;
            if (participantId.HasValue)
            {
                participant = await GetParticipant(participantId.Value);
            }

            return participant;
        }

        private Result<CallClientTypeFilter> MapCallTypeEnums(CallTypeFilter filter)
        {
            CallClientTypeFilter typeFilter;

            switch (filter)
            {
                case CallTypeFilter.All:

                    typeFilter = CallClientTypeFilter.All;

                    break;
                case CallTypeFilter.Incoming:

                    typeFilter = CallClientTypeFilter.Incoming;

                    break;
                case CallTypeFilter.Outgoing:

                    typeFilter = CallClientTypeFilter.Outgoing;

                    break;
                default:

                    var message = $"CallManagementService. Error on converting CallTypeFilter to CallClientTypeFilter. CallTypeFilter value: {(int)filter}";
                    _logger.Warning(message);
                    return Result.Failure<CallClientTypeFilter>(ErrorCodes.ValidationError);
            }

            return Result.Success(typeFilter);
        }

        private async Task<Result<List<CallRecordInfoDto>>> GetRecordsList(
            Guid lineId,
            List<AudioRecordClientDto> records,
            List<CallTimeInfoClientDto> participantCalls)
        {
            var fullRecord = GetFullRecord(records);
            if (fullRecord == null)
            {
                _logger.Warning($"GenerateParticipantsList. Full record not found. LineId: {lineId}");
                return Result.Failure<List<CallRecordInfoDto>>(ErrorCodes.UnableToGetAudioRecords);
            }

            var minStartTime = fullRecord.RecordingStartTime;
            var maxEndDate = fullRecord.RecordingEndTime;
            var totalCallTime = maxEndDate.Subtract(minStartTime);

            var callRecordsList = new List<CallRecordInfoDto>();
            callRecordsList.Add(new CallRecordInfoDto
            {
                Id = fullRecord.Id,
                ParticipantInfo = new ParticipantInfoDto
                {
                    StartCallTime = FormatCallTime(TimeSpan.Zero),
                    EndCallTime = FormatCallTime(totalCallTime)
                },
                IsFullRecord = true
            });

            using UnitOfWork unitOfWork = _unitOfWork.Begin();

            foreach (var record in records)
            {
                Result<CallRecordInfoDto> recordInfoResult = await GetParticipantRecordInfo(participantCalls, record, fullRecord);
                if (recordInfoResult.IsSuccess)
                {
                    callRecordsList.Add(recordInfoResult.Value);
                }
            }

            return Result.Success(callRecordsList);
        }

        private async Task<Result<CallRecordInfoDto>> GetParticipantRecordInfo(
            List<CallTimeInfoClientDto> participantCalls,
            AudioRecordClientDto participantRecord,
            AudioRecordClientDto fullRecord)
        {
            var participantCallInfo = participantCalls.FirstOrDefault(x => x.CallId == participantRecord.CallId);
            if (participantCallInfo == null)
            {
                _logger.Warning($"GenerateParticipantsList. Participant call not found. CallId: {participantRecord.CallId}");
                return Result.Failure<CallRecordInfoDto>(ErrorCodes.UnableToGetAudioRecords);
            }

            var participant = await GetParticipant(participantCallInfo.ParticipantId.Value);
            var participantInfo = ParticipantInfoCreator.GetParticipantInfoDto(participant);
            FillParticipantTimeStamps(participantInfo, participantRecord, fullRecord);

            var participantRecordInfo = new CallRecordInfoDto
            {
                Id = participantRecord.Id,
                ParticipantInfo = participantInfo,
                IsEmercoreUser = participant is User,
                CallId = participantCallInfo.CallId,
                IsFullRecord = false
            };

            return Result.Success(participantRecordInfo);
        }

        private AudioRecordClientDto GetFullRecord(List<AudioRecordClientDto> records)
        {
            var fullRecord = records.FirstOrDefault(record => !record.CallId.HasValue);
            return fullRecord;
        }

        private void FillParticipantTimeStamps(
            ParticipantInfoDto participantInfo,
            AudioRecordClientDto participantRecord,
            AudioRecordClientDto fullCallRecord)
        {
            var startFullRecordTime = fullCallRecord.RecordingStartTime;
            var endFullRecordTime = fullCallRecord.RecordingEndTime;

            var startRecordDateTime = participantRecord.RecordingStartTime < startFullRecordTime ? startFullRecordTime : participantRecord.RecordingStartTime;
            var endRecordDateTime = participantRecord.RecordingEndTime > endFullRecordTime ? endFullRecordTime : participantRecord.RecordingEndTime;

            var start = startRecordDateTime - startFullRecordTime;
            participantInfo.StartCallTime = FormatCallTime(start);

            var duration = endRecordDateTime - startRecordDateTime;
            var end = start + duration;
            participantInfo.EndCallTime = FormatCallTime(end);
        }

        /// <summary>
        /// Преобразовать TimeSpan в форматированную строку
        /// </summary>
        /// <param name="callTime"></param>
        private static string FormatCallTime(TimeSpan callTime)
        {
            var result = callTime.Hours.ToString("00") +
                ":" + callTime.Minutes.ToString("00") +
                ":" + callTime.Seconds.ToString("00");

            return result;
        }
    }
}
