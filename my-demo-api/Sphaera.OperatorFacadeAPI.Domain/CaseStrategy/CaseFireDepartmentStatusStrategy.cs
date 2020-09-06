using demo.FunctionalExtensions;
using demo.DemoApi.Domain.CaseStrategy.Enums;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Domain.CaseStrategy
{
    /// <summary>
    /// Стратегия обработки статусов карточки "Пожарная охрана".
    /// </summary>
    public class CaseFireDepartmentStatusStrategy : ICaseStatusStrategy
    {
        /// <inheritdoc />
        public string GetStatusDefault()
        {
            return CaseFireDepartmentStatus.New.ToString();
        }

        /// <inheritdoc />
        public string GetStatusInProgress()
        {
            return CaseFireDepartmentStatus.InProgress.ToString();
        }
        
        /// <inheritdoc />
        public Result<string> SetStatusInProgress(Case currentCase)
        {
            if (!IsStatusClosed(currentCase))
            {
                currentCase.Status = CaseFireDepartmentStatus.InProgress.ToString();
            
                return Result.Success(currentCase.Status);
            }

            return Result.Failure<string>(ErrorCodes.UnableToSetCaseStatus, currentCase.Status);
        }

        /// <inheritdoc />
        public Result<string> SetStatusClosed(Case currentCase)
        {
            if (currentCase.Status == GetStatusInProgress())
            {
                currentCase.Status = CaseFireDepartmentStatus.Closed.ToString();
                
                return Result.Success(currentCase.Status);
            }
            
            return Result.Failure<string>(ErrorCodes.UnableToSetCaseStatus, currentCase.Status);
        }
        
        /// <inheritdoc />
        public bool IsStatusInProgress(Case currentCase)
        {
            return currentCase.Status == CaseFireDepartmentStatus.InProgress.ToString();
        }
        
        /// <inheritdoc />
        public bool IsStatusClosed(Case currentCase)
        {
            return currentCase.Status == CaseFireDepartmentStatus.Closed.ToString();
        }
    }
}