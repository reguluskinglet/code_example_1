using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using demo.CallManagement.Client;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices.Abstractions;
using demo.DemoApi.Service.Dtos.Line;
using demo.DemoApi.Service.Dtos.Participant;
using demo.UserManagement.Client;

namespace demo.DemoApi.Service.ApplicationServices.Lines
{
    /// <summary>
    /// Сервис отвечающий за линию
    /// </summary>
    public class LineService : ILineService
    {
        private readonly ILogger _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly IParticipantRepository _participantRepository;
        private readonly CallManagementServiceClient _callManagementServiceClient;
        private readonly UserManagementServiceClient _userManagementServiceClient;
        private readonly IMapper _mapper;

        /// <summary>
        /// Конструктор для инъекции зависимостей
        /// </summary>
        public LineService(
            CallManagementServiceClient callManagementServiceClient,
            UserManagementServiceClient userManagementServiceClient,
            ILogger logger,
            IMapper mapper,
            UnitOfWork unitOfWork,
            IParticipantRepository participantRepository)
        {
            _userManagementServiceClient = userManagementServiceClient;
            _callManagementServiceClient = callManagementServiceClient;
            _logger = logger;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _participantRepository = participantRepository;
        }

        /// <summary>
        /// Получить все линии, в которых участвует пользователь
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Result<List<LineViewDto>>> GetUserLines(Guid userId)
        {
            var userLinesResult = await _callManagementServiceClient.GetUserLines();

            if (userLinesResult.IsFailure)
            {
                _logger.Warning(userLinesResult.ErrorMessage);
                return Result.Failure<List<LineViewDto>>(ErrorCodes.UnableToGetUserLines);
            }

            using (_unitOfWork.Begin())
            {
                var userLines = userLinesResult.Value.Select(LineViewDto.MapFromClientDto).ToList();
                foreach (var infoDto in userLines.SelectMany(x => x.Participants))
                {
                    var participant = await GetParticipant(infoDto.Id);
                    infoDto.Extension = participant?.ParticipantExtension;
                    infoDto.ParticipantInfo = ParticipantInfoCreator.GetParticipantInfo(participant);
                }

                return Result.Success(userLines);
            }
        }

        /// <summary>
        /// Получить линии по caseFolderId
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        public async Task<Result<List<LineByCaseFolderViewDto>>> GetLinesByCaseFolderId(Guid caseFolderId)
        {
            var getLinesByCaseFolderIdResult = await _callManagementServiceClient.GetLinesByCaseFolderId(caseFolderId);

            if (getLinesByCaseFolderIdResult.IsFailure)
            {
                _logger.Warning(getLinesByCaseFolderIdResult.ErrorMessage);
                return Result.Failure<List<LineByCaseFolderViewDto>>(ErrorCodes.UnableToGetLines);
            }

            using (_unitOfWork.Begin())
            {
                var lineDtos = getLinesByCaseFolderIdResult.Value.Select(x => _mapper.Map<LineByCaseFolderDto>(x)).ToList();

                var linesByCaseFolderView = new List<LineByCaseFolderViewDto>();

                foreach (var lineDto in lineDtos)
                {
                    var lineByCaseFolder = new LineByCaseFolderViewDto
                    {
                        Id = lineDto.Id,
                        CaseFolderId = lineDto.CaseFolderId,
                        CreateDateTime = lineDto.CreateDateTime,
                        FirstCallType = lineDto.FirstCallType
                    };

                    var caller = await GetParticipant(lineDto.CallerId);

                    lineByCaseFolder.CallInitiator = ParticipantInfoCreator.GetParticipantInfoDto(caller);
                    lineByCaseFolder.IsEmercoreUser = caller is User;

                    linesByCaseFolderView.Add(lineByCaseFolder);
                }

                return Result.Success(linesByCaseFolderView);
            }
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
    }
}
