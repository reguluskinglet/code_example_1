using System.Linq;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Case;

namespace demo.DemoApi.Service.ApplicationServices.Cases
{
    /// <summary>
    /// Создание Dto для Case
    /// </summary>
    public class CaseDtoCreator
    {
        /// <summary>
        /// Создать DTO с информацией о статусах карточек в CaseFolder
        /// </summary>
        /// <param name="caseFolder"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Result<CaseStatusesInfoDto> GetStatusInfoDto(CaseFolder caseFolder, UserDto user)
        {
            var currentCaseResult = caseFolder.GetCaseForUser(user.Id);

            if (currentCaseResult.IsFailure)
            {
                return Result.Failure<CaseStatusesInfoDto>(currentCaseResult.ErrorCode);
            }

            var cases = caseFolder.Cases;
            var caseStatuses = cases.Select(x => new CaseStatusDto
                {
                    CaseCardId = x.Id,
                    Status = x.Status
                })
                .ToList();

            var statusesInfo = new CaseStatusesInfoDto
            {
                Statuses = caseStatuses,
                CanCloseCaseCard = currentCaseResult.Value.IsInProgress()
            };

            return Result.Success(statusesInfo);
        }
    }
}