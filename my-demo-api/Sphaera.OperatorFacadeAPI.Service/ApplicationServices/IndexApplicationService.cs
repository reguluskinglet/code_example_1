using System;
using System.Threading.Tasks;
using AutoMapper;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.IndexService.Client;
using demo.IndexService.HttpContracts.Dto;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos.Index;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для работы с индексами инцидентов
    /// </summary>
    public class IndexApplicationService
    {
        private readonly IndexServiceClient _indexServiceClient;
        private readonly PhoneHubMessageService _phoneHubMessageService;
        private readonly UnitOfWork _unitOfWork;
        private readonly ICaseRepository _caseRepository;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public IndexApplicationService(
            IndexServiceClient indexServiceClient,
            UnitOfWork unitOfWork,
            ICaseRepository caseRepository,
            ILogger logger,
            PhoneHubMessageService phoneHubMessageService,
            IMapper mapper)
        {
            _indexServiceClient = indexServiceClient;
            _unitOfWork = unitOfWork;
            _caseRepository = caseRepository;
            _logger = logger;
            _phoneHubMessageService = phoneHubMessageService;
            _mapper = mapper;
        }

        /// <summary>
        /// Сохранить выбранный индекс карточки инцидента
        /// </summary>
        public async Task<Result> SaveCaseIndex(Guid caseId, Guid indexId)
        {
            var saveResult = await _indexServiceClient.SaveCaseIndex(caseId, indexId);

            if (saveResult.IsSuccess)
            {
                await _phoneHubMessageService.NotifyAboutIndexUpdated(caseId, indexId);

                var getResult = await _indexServiceClient.GetCaseIndexPathValue(caseId);
                if (getResult.IsSuccess && getResult.Value != null)
                {
                    var updateResult = await SaveCaseIndexPathValue(caseId, getResult.Value.Path);
                    if (updateResult.IsFailure)
                    {
                        return updateResult;
                    }
                }
            }

            return saveResult;
        }

        /// <summary>
        /// Получить индекс по идентификатору карточки инцидента
        /// </summary>
        public async Task<Result<IndexDto>> GetIndexByCaseId(Guid caseId)
        {
            Result<IndexClientDto> result = await _indexServiceClient.GetIndexByCaseId(caseId);
            if (result.IsFailure)
            {
                _logger.Information($"Index for CaseId {caseId} not found in IndexService. {result.ErrorMessage}");
                return Result.Failure<IndexDto>(ErrorCodes.UnableToGetIndex);
            }

            var indexDto = _mapper.Map<IndexDto>(result.Value);
            return Result.Success(indexDto);
        }

        /// <summary>
        /// Получить дерево индексов по идентификатору типа карточки инцидента
        /// </summary>
        public async Task<Result<IndexesDto>> GetIndexTreeByCaseTypeId(Guid caseTypeId)
        {
            Result<IndexTreeClientDto> result = await _indexServiceClient.GetIndexesByCaseTypeId(caseTypeId);
            if (result.IsFailure)
            {
                _logger.Warning($"Error getting index tree from IndexService. caseTypeId: {caseTypeId}. {result.ErrorMessage}");
                return Result.Failure<IndexesDto>(ErrorCodes.UnableToGetIndex);
            }

            var indexesDto = _mapper.Map<IndexesDto>(result.Value);
            return Result.Success(indexesDto);
        }

        /// <summary>
        /// Сохранить или обновить полный путь текущего индекса карточки инцидента
        /// </summary>
        private async Task<Result> SaveCaseIndexPathValue(Guid caseId, string indexValue)
        {
            using (_unitOfWork.Begin())
            {
                var caseCard = await _caseRepository.GetById(caseId);
                if (caseCard == null)
                {
                    _logger.Warning($"Case with Id {caseId} not found");
                    return Result.Failure(ErrorCodes.CaseNotFound);
                }

                caseCard.IndexValue = indexValue;

                await _unitOfWork.CommitAsync();
                return Result.Success();
            }
        }
    }
}
