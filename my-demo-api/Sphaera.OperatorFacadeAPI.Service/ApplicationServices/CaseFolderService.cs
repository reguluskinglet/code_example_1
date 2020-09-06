using System;
using System.Linq;
using System.Threading.Tasks;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для операций с индидентом
    /// </summary>
    public class CaseFolderService
    {
        private readonly ILogger _logger;
        private readonly UnitOfWork _unitOfWork;
        private readonly ICaseFolderRepository _caseFolderRepository;

        /// <inheritdoc />
        public CaseFolderService(
            ILogger logger,
            UnitOfWork unitOfWork,
            ICaseFolderRepository caseFolderRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _caseFolderRepository = caseFolderRepository;
        }

        /// <summary>
        /// Получение страницы инцидента
        /// </summary>
        public async Task<Result<CaseFolderPageDto>> GetCaseFolderPage(int pageIndex, int pageSize)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolderPage = await _caseFolderRepository.GetCaseFolderPage(pageIndex, pageSize);
                if (caseFolderPage == null)
                {
                    _logger.Warning($"Page with CaseFolders not found");
                    return Result.Failure<CaseFolderPageDto>(ErrorCodes.CaseFolderPageNotFound);
                }

                var caseFolderPageDto = new CaseFolderPageDto
                {
                    CaseFolders = caseFolderPage.CaseFolders.Select(CaseFolderListItemDto.MapFromCaseEntity).ToList(),
                    PageView = new PageDto(caseFolderPage.TotalCount, pageIndex, pageSize)
                };

                _logger.Debug($"Получена {pageIndex} страница со списком карточек.");

                return Result.Success(caseFolderPageDto);
            }
        }

        /// <summary>
        /// Получить инцидент по Id
        /// </summary>
        public async Task<Result<CaseFolderListItemDto>> Get(Guid caseFolderId)
        {
            using (_unitOfWork.Begin())
            {
                var caseFolder = await _caseFolderRepository.GetById(caseFolderId);
                if (caseFolder == null)
                {
                    _logger.Warning($"CaseFolder not found");
                    return Result.Failure<CaseFolderListItemDto>(ErrorCodes.CaseFolderNotFound);
                }

                var caseFolderListItem = CaseFolderListItemDto.MapFromCaseEntity(caseFolder);

                _logger.Debug($"Найден инцидент по id {caseFolderId}.");

                return Result.Success(caseFolderListItem);
            }
        }
    }
}
