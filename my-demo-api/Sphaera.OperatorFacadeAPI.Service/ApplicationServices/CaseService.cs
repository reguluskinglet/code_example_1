using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using demo.Authorization.Contexts;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.GisFacade.Client;
using demo.GisFacade.Client.Dto;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Entities.SmsMetadata;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Domain.ValueObjects;
using demo.DemoApi.Service.ApplicationServices.Cases;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Case;
using demo.UserManagement.Client;
using demo.UserManagement.HttpContracts.Dto;
using ActivatedPlanInstruction = demo.DemoApi.Domain.Entities.ActivatedPlanInstruction;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для операций с карточкой
    /// </summary>
    public class CaseService
    {
        private readonly ILogger _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly PhoneHubMessageService _phoneHubMessageService;
        private readonly ICaseRepository _caseRepository;
        private readonly ICaseTypeRepository _caseTypeRepository;
        private readonly ICaseFolderRepository _caseFolderRepository;
        private readonly GisService _gisService;
        private readonly GisFacadeClient _gisFacadeClient;
        private readonly IMapper _mapper;
        private readonly UserManagementServiceClient _userManagementServiceClient;

        /// <summary>
        /// Конструктор сервиса обработки карточки события.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="phoneHubMessageService"></param>
        /// <param name="caseRepository"></param>
        /// <param name="caseTypeRepository"></param>
        /// <param name="caseFolderRepository"></param>
        /// <param name="gisService"></param>
        /// <param name="gisFacadeClient"></param>
        /// <param name="mapper"></param>
        /// <param name="userManagementServiceClient"></param>
        public CaseService(
            ILogger logger,
            UnitOfWork unitOfWork,
            PhoneHubMessageService phoneHubMessageService,
            ICaseRepository caseRepository,
            ICaseTypeRepository caseTypeRepository,
            ICaseFolderRepository caseFolderRepository,
            GisService gisService,
            GisFacadeClient gisFacadeClient,
            IMapper mapper,
            UserManagementServiceClient userManagementServiceClient)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _phoneHubMessageService = phoneHubMessageService;
            _caseRepository = caseRepository;
            _caseTypeRepository = caseTypeRepository;
            _caseFolderRepository = caseFolderRepository;
            _gisService = gisService;
            _gisFacadeClient = gisFacadeClient;
            _mapper = mapper;
            _userManagementServiceClient = userManagementServiceClient;
        }

        /// <summary>
        /// Получение карточки по Id инцидента
        /// </summary>
        public async Task<Result<CaseDto>> GetCaseByCaseFolderIdAsync(Guid caseFolderId, Guid userId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<CaseDto>(ErrorCodes.CaseFolderNotFound);
                }

                Result<UserClientDto> result = await _userManagementServiceClient.GetUserById(userId);
                if (result.IsFailure)
                {
                    _logger.Warning($"User with Id {userId} not found");
                    return Result.Failure<CaseDto>(ErrorCodes.UserNotFound);
                }

                var user = _mapper.Map<UserDto>(result.Value);

                Result<Case> userCaseCard = caseFolder.GetCaseForUser(user.Id);
                if (userCaseCard.IsFailure)
                {
                    _logger.Warning(userCaseCard.ErrorMessage);
                    return Result.Failure<CaseDto>(userCaseCard.ErrorCode);
                }

                await NotifyGisFacadeAboutNewApplicantLocation(caseFolder, userId);

                return Result.Success(CaseDto.MapFromCaseEntity(userCaseCard.Value));
            }
        }

        /// <summary>
        /// Получить карточку по Id
        /// </summary>
        public async Task<Result<CaseDto>> GetCaseById(Guid caseId)
        {
            using (_unitOfWork.Begin())
            {
                var caseCard = await _caseRepository.GetById(caseId);
                if (caseCard == null)
                {
                    _logger.Warning($"Case with Id {caseId} not found");
                    return Result.Failure<CaseDto>(ErrorCodes.CaseNotFound);
                }

                return Result.Success(CaseDto.MapFromCaseEntity(caseCard));
            }
        }

        /// <summary>
        /// Оповестить клиентов о добавлении местоположения абонента.
        /// </summary>
        private async Task NotifyGisFacadeAboutNewApplicantLocation(CaseFolder caseFolder, Guid operatorId)
        {
            Result<SmsLocationData> applicantLocationDataResult = caseFolder.GetLocationData();
            if (applicantLocationDataResult.IsFailure)
            {
                _logger.Warning($"CaseService. Не удалось получить данные о местоположении из CaseFolder. {applicantLocationDataResult.ErrorMessage}");
            }

            if (applicantLocationDataResult.IsSuccess && !caseFolder.IsClosed)
            {
                var applicantLocationDto = _mapper.Map<LocationDataClientDto>(applicantLocationDataResult.Value);
                await _gisFacadeClient.AddApplicantLocationMarker(caseFolder.Id, applicantLocationDto);
                await _phoneHubMessageService.NotifyClientsAboutApplicantLocationUpdateAsync(caseFolder.Id, operatorId);
            }
        }

        /// <summary>
        /// Получение данных поля карточки
        /// </summary>
        /// <param name="caseFolderId">Идентификатор инцидента</param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<Result<CaseFieldDto>> GetFieldDataAsync(Guid caseFolderId, Guid fieldId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<CaseFieldDto>(ErrorCodes.CaseFolderNotFound);
                }

                var fieldValue = caseFolder.GetFieldValue(fieldId);

                return Result.Success(new CaseFieldDto
                {
                    FieldId = fieldId,
                    Value = fieldValue
                });
            }
        }

        /// <summary>
        /// Обновление данных карточки
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fieldDto"></param>
        /// <returns></returns>
        public async Task<Result> UpdateFieldAsync(Guid userId, CaseFieldDto fieldDto)
        {
            using (_unitOfWork.Begin(IsolationLevel.Serializable))
            {
                var caseFolder = await _caseFolderRepository.GetById(fieldDto.CaseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {fieldDto.CaseFolderId} not found");
                    return Result.Failure(ErrorCodes.CaseFolderNotFound);
                }

                Result<UserClientDto> result = await _userManagementServiceClient.GetUserById(userId);
                if (result.IsFailure)
                {
                    _logger.Warning($"User with Id {userId} not found");
                    return Result.Failure(ErrorCodes.UserNotFound);
                }

                var user = _mapper.Map<UserDto>(result.Value);

                var updateResult = caseFolder.UpdateField(fieldDto.FieldId, fieldDto.Value);
                if (updateResult.IsFailure)
                {
                    _logger.Warning(updateResult.ErrorMessage);
                    return updateResult;
                }

                var caseCard = caseFolder.GetCaseForUser(user.Id);
                if (caseCard.IsFailure)
                {
                    _logger.Warning(caseCard.ErrorMessage);
                    return Result.Failure(caseCard.ErrorCode);
                }

                var caseStatusResult = caseCard.Value.SetStatusInProgress();
                if (caseStatusResult.IsFailure)
                {
                    _logger.Warning(caseStatusResult.ErrorMessage);
                    return Result.Failure(caseStatusResult.ErrorCode);
                }

                await _unitOfWork.CommitAsync();

                var changeEventDto = new CaseFieldChangeEventDto
                {
                    CaseFolderId = fieldDto.CaseFolderId,
                    FieldId = fieldDto.FieldId,
                    OperatorId = userId
                };

                await _phoneHubMessageService.NotifyClientsAboutFieldChangedAsync(changeEventDto);

                return Result.Success();
            }
        }

        /// <summary>
        /// Получение данных об активированных инструкциях плана
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task<List<ActivatedPlanInstructionDto>> GetActivatedPlanInstructionsAsync(Guid caseId, Guid planId)
        {
            using (_unitOfWork.Begin())
            {
                var caseCard = await _caseRepository.GetById(caseId);
                List<ActivatedPlanInstruction> activatedInstructions = caseCard.GetActivatedInstructions(planId);

                return activatedInstructions.Select(x => new ActivatedPlanInstructionDto
                {
                    CaseId = caseId,
                    PlanId = x.PlanId,
                    InstructionId = x.InstructionId,
                    ActivationDate = x.ActivationDate
                }).ToList();
            }
        }

        /// <summary>
        /// Обновление активированных инструкциях плана
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="activatedInstructionDto"></param>
        /// <returns></returns>
        public async Task<Result> UpdateActivatedPlanInstructionsAsync(
            Guid userId,
            ActivatedPlanInstructionDto activatedInstructionDto)
        {
            if (!activatedInstructionDto.ActivationDate.HasValue)
            {
                _logger.Warning($"Время активации инструкции с Id {activatedInstructionDto.InstructionId} не задано. CaseId: {activatedInstructionDto.CaseId}");
                return Result.Failure(ErrorCodes.ValidationError);
            }

            using (_unitOfWork.Begin())
            {
                var caseCard = await _caseRepository.GetById(activatedInstructionDto.CaseId);
                if (caseCard == null)
                {
                    _logger.Warning($"Case with Id: {activatedInstructionDto.CaseId} not found");
                    return Result.Failure(ErrorCodes.CaseNotFound);
                }

                var updateResult = caseCard.UpdateActivatedInstructions(
                    activatedInstructionDto.PlanId,
                    activatedInstructionDto.InstructionId,
                    activatedInstructionDto.ActivationDate.Value);

                if (updateResult.IsFailure)
                {
                    _logger.Warning(updateResult.ErrorMessage);
                    return updateResult;
                }

                Result<string> caseStatusResult = caseCard.SetStatusInProgress();
                if (caseStatusResult.IsFailure)
                {
                    _logger.Warning(caseStatusResult.ErrorMessage);
                    return Result.Failure(caseStatusResult.ErrorCode);
                }
                await _unitOfWork.CommitAsync();
                await _phoneHubMessageService.NotifyClientsAboutPlanUpdateAsync(activatedInstructionDto.CaseId, activatedInstructionDto.PlanId);

                return Result.Success();
            }
        }

        /// <summary>
        /// Взять заголовки по caseFolderId
        /// </summary>
        public async Task<Result<TitlesDto>> GetCaseTitlesAsync(Guid caseFolderId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<TitlesDto>(ErrorCodes.CaseFolderNotFound);
                }

                IList<Title> entityTitles = caseFolder.GetCaseTitles();

                return Result.Success(new TitlesDto
                {
                    Titles = entityTitles.OrderBy(x => x.Text).Select(title => new CaseTitleDto
                    {
                        CaseId = title.CaseId,
                        Text = title.Text,
                    })
                });
            }
        }

        /// <summary>
        /// Получить список шаблонов
        /// </summary>
        public async Task<List<CaseTypeDto>> GetCaseTypesInfoAsync()
        {
            using (_unitOfWork.Begin())
            {
                IList<CaseType> types = await _caseTypeRepository.GetAll();
                List<CaseTypeDto> typeDtos = types.Select(x => new CaseTypeDto
                {
                    Id = x.Id,
                    Title = x.GetTitle()
                }).ToList();

                return typeDtos;
            }
        }

        /// <summary>
        /// Информирование операторов о изменении поля карточки.
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="caseFolderId"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public async Task<Result> SetActiveFieldAsync(Guid operatorId, Guid caseFolderId, Guid fieldId)
        {
            var changeEventDto = new CaseFieldChangeEventDto
            {
                CaseFolderId = caseFolderId,
                FieldId = fieldId,
                OperatorId = operatorId
            };
            await _phoneHubMessageService.NotifyClientsAboutActiveFieldAsync(changeEventDto);
            return Result.Success();
        }

        /// <summary>
        /// Информирование операторов о изменении поля локации
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="caseFolderId"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public async Task<Result> SetActiveLocationFieldAsync(Guid userId, Guid caseFolderId,
            CoordinateFieldType fieldType)
        {
            await _phoneHubMessageService.NotifyClientsAboutLocationActiveFieldAsync(userId, caseFolderId, fieldType);
            return Result.Success();
        }

        /// <summary>
        /// Обновление координат места происшествия
        /// </summary>
        /// <param name="locationDto"></param>
        /// <returns></returns>
        public async Task<Result> UpdateLocationAsync(UpdateLocationDto locationDto)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolderId = locationDto.CaseFolderId;
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolser with Id {caseFolderId} not found");
                    return Result.Failure(ErrorCodes.CaseFolderNotFound);
                }

                if (locationDto.CoordinateFieldType == CoordinateFieldType.Latitude)
                {
                    caseFolder.Latitude = locationDto.Value;
                }
                else if (locationDto.CoordinateFieldType == CoordinateFieldType.Longitude)
                {
                    caseFolder.Longitude = locationDto.Value;
                }
                else
                {
                    _logger.Warning("Unknown coordinate field type to update");
                    return Result.Failure(ErrorCodes.ValidationError);
                }

                await _unitOfWork.CommitAsync();

                await _phoneHubMessageService.NotifyClientsAboutLocationUpdateAsync(caseFolderId, locationDto.OperatorId);

                return Result.Success();
            }
        }

        /// <summary>
        /// Получение координат места
        /// </summary>
        /// <param name="caseFolderId">Идентификатор инцидента</param>
        /// <returns></returns>
        public async Task<Result<IncidentLocationDto>> GetLocationAsync(Guid caseFolderId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<IncidentLocationDto>(ErrorCodes.CaseFolderNotFound);
                }

                return Result.Success(new IncidentLocationDto(caseFolderId, caseFolder.Latitude, caseFolder.Longitude));
            }
        }

        /// <summary>
        /// Получение координат места происшествия в ESPG:3857.
        /// </summary>
        /// <param name="caseFolderId">Идентификатор инцидента</param>
        /// <returns></returns>
        public async Task<Result<IncidentLocationDto>> GetIncidentLocationEspg3857Async(Guid caseFolderId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<IncidentLocationDto>(ErrorCodes.CaseFolderNotFound);
                }

                var getLocationResult = await _gisFacadeClient.GetLocationEspg3857(caseFolder.Latitude.Value, caseFolder.Longitude.Value);
                if (getLocationResult.IsFailure)
                {
                    _logger.Warning($"GetIncidentLocationEspg3857Async. Failed to get caseFolder location. {getLocationResult.ErrorMessage}");
                    return Result.Failure<IncidentLocationDto>(ErrorCodes.UnableToGetLocationEspg3857);
                }

                return Result.Success(new IncidentLocationDto(caseFolderId, getLocationResult.Value.Latitude, getLocationResult.Value.Longitude));
            }
        }

        /// <summary>
        /// Отобразить локацию инцидента на карте по заданным координатам.
        /// </summary>
        /// <param name="caseOperatorDto"></param>
        /// <returns></returns>
        public async Task<Result> AddIncidentLocationOnMapAsync(CaseOperatorDto caseOperatorDto)
        {
            _logger.Verbose("Begin AddIncidentLocationOnMapAsync");

            using (_unitOfWork.Begin())
            {
                var caseFolderId = caseOperatorDto.CaseFolderId;
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure(ErrorCodes.CaseFolderNotFound);
                }

                if (caseFolder.Latitude.HasValue && caseFolder.Longitude.HasValue)
                {
                    await _phoneHubMessageService.NotifyClientsAboutIncidentLocationUpdateAsync(caseFolderId, AuthorizationContext.CurrentUserId.Value);
                    return await _gisService.AddIncidentMarkerToMap(
                        caseFolderId,
                        caseFolder.Latitude.Value,
                        caseFolder.Longitude.Value);
                }

                _logger.Warning("No location coordinates set");
                return Result.Failure(ErrorCodes.ValidationError);
            }
        }

        /// <summary>
        /// Сделать запрос в GisFacade для создания маркера инцидента по местоположению заявителя.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="caseApplicantDto"></param>
        /// <returns></returns>
        public async Task<Result> UseApplicantLocationAsIncidentLocationAsync(Guid userId, CaseApplicantDto caseApplicantDto)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolderId = caseApplicantDto.CaseFolderId;
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure(ErrorCodes.CaseFolderNotFound);
                }

                var locationDataResult = caseFolder.GetLocationData();
                if (locationDataResult.IsFailure)
                {
                    _logger.Warning(locationDataResult.ErrorMessage);
                    return Result.Failure(locationDataResult.ErrorCode);
                }

                var position = locationDataResult.Value;
                caseFolder.Latitude = position.Position.Latitude;
                caseFolder.Longitude = position.Position.Longitude;

                await _unitOfWork.CommitAsync();

                var addLocationResult = await _gisService.AddIncidentMarkerToMap(
                    caseFolderId,
                    position.Position.Latitude,
                    position.Position.Longitude);

                if (addLocationResult.IsFailure)
                {
                    _logger.Warning(addLocationResult.ErrorMessage);
                }

                await _phoneHubMessageService.NotifyClientsAboutIncidentLocationUpdateAsync(caseFolderId, userId);
                await _phoneHubMessageService.NotifyClientsAboutLocationUpdateAsync(caseFolderId, userId);
                return Result.Success();
            }
        }

        /// <summary>
        /// Сделать запрос в GisFacade для создания маркера местоположения заявителя.
        /// </summary>
        /// <param name="applicantLocationDto"></param>
        /// <returns></returns>
        public async Task<Result> AddApplicantLocationOnMapAsync(CaseApplicantDto applicantLocationDto)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolderId = applicantLocationDto.CaseFolderId;
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure(ErrorCodes.CaseFolderNotFound);
                }

                var locationDataResult = caseFolder.GetLocationData();
                if (locationDataResult.IsFailure)
                {
                    _logger.Warning(locationDataResult.ErrorMessage);
                    return Result.Failure(locationDataResult.ErrorCode);
                }

                var location = locationDataResult.Value;
                var locationDto = _mapper.Map<LocationDataClientDto>(location);
                var addLocationResult = await _gisFacadeClient.AddApplicantLocationMarker(caseFolderId, locationDto);
                if (addLocationResult.IsFailure)
                {
                    _logger.Warning($"AddApplicantLocationOnMapAsync. Failed to add applicant location. {addLocationResult.ErrorMessage}");
                    return Result.Failure(ErrorCodes.UnableToAddApplicantLocationMarker);
                }

                await _phoneHubMessageService.NotifyClientsAboutApplicantLocationUpdateAsync(caseFolderId, AuthorizationContext.CurrentUserId.Value);
                return addLocationResult;
            }
        }

        /// <summary>
        /// Получение координат метоположения заявителя в ESPG-3857.
        /// </summary>
        /// <param name="caseFolderId">Идентификатор инцидента</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<ApplicantLocationDto>> GetApplicantLocationAsync(Guid caseFolderId, Guid userId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<ApplicantLocationDto>(ErrorCodes.CaseFolderNotFound);
                }

                var locationDataResult = caseFolder.GetLocationData();
                if (locationDataResult.IsFailure)
                {
                    _logger.Warning(locationDataResult.ErrorMessage);
                    return Result.Failure<ApplicantLocationDto>(locationDataResult.ErrorCode);
                }

                var position = locationDataResult.Value;
                var getLocationResult = await _gisFacadeClient.GetLocationEspg3857(position.Position.Latitude, position.Position.Longitude);
                if (getLocationResult.IsFailure)
                {
                    _logger.Warning($"GetApplicantLocationAsync. Failed to get applicant location. {getLocationResult.ErrorMessage}");
                    return Result.Failure<ApplicantLocationDto>(ErrorCodes.UnableToGetLocationEspg3857);
                }

                return Result.Success(new ApplicantLocationDto(
                    caseFolderId,
                    getLocationResult.Value.Latitude,
                    getLocationResult.Value.Longitude,
                    new ApplicantLocationInfoDto()
                    {
                        Radius = position.Radius,
                        InnerRadius = position.InnerRadius,
                        OpeningAngle = position.OpeningAngle,
                        OuterRadius = position.OuterRadius,
                        StartAngle = position.StartAngle
                    }
                ));
            }
        }

        /// <summary>
        /// Получение информации о статусах карточек в CaseFolder
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<CaseStatusesInfoDto>> GetStatusesInfo(Guid caseFolderId, Guid userId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder with Id {caseFolderId} not found");
                    return Result.Failure<CaseStatusesInfoDto>(ErrorCodes.CaseFolderNotFound);
                }

                Result<UserClientDto> result = await _userManagementServiceClient.GetUserById(userId);
                if (result.IsFailure)
                {
                    _logger.Warning($"User with Id {userId} not found");
                    return Result.Failure<CaseStatusesInfoDto>(ErrorCodes.UserNotFound);
                }

                var user = _mapper.Map<UserDto>(result.Value);

                var dtoCreator = new CaseDtoCreator();
                var dtoResult = dtoCreator.GetStatusInfoDto(caseFolder, user);
                if (dtoResult.IsFailure)
                {
                    _logger.Warning(dtoResult.ErrorMessage);
                    return Result.Failure<CaseStatusesInfoDto>(dtoResult.ErrorCode);
                }

                return Result.Success(dtoResult.Value);
            }
        }

        /// <summary>
        /// Сообщаяет о том, что нужно изменить статус в "Closed"
        /// </summary>
        /// <returns></returns>
        public async Task<Result> CloseCaseCard(Guid userId, CloseCaseCardDto model)
        {
            using (_unitOfWork.Begin())
            {
                var caseCard = await _caseRepository.GetById(model.CaseCardId);
                if (caseCard == null)
                {
                    _logger.Warning($"Case with Id {model.CaseCardId} not found");
                    return Result.Failure(ErrorCodes.CaseNotFound);
                }

                var caseCardResult = caseCard.Close();
                if (caseCardResult.IsFailure)
                {
                    _logger.Warning(caseCardResult.ErrorMessage);
                    return Result.Failure(caseCardResult.ErrorCode);
                }

                var isCaseFolderClosed = caseCard.CaseFolder.IsClosed;
                var caseFolderId = caseCard.CaseFolder.Id;

                await _unitOfWork.CommitAsync();

                if (isCaseFolderClosed)
                {
                    await _gisFacadeClient.DeleteIncidentMarker(caseFolderId);
                    await _gisFacadeClient.DeleteApplicantLocationMarker(caseFolderId);
                }

                return Result.Success();
            }
        }
    }
}
