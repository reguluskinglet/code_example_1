using demo.FunctionalExtensions;
using demo.DemoApi.Domain.CaseStrategy.Enums;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Domain.CaseStrategy
{
    /// <summary>
    /// Стратегия обработки статусов карточки "112".
    /// </summary>
    public class Case112StatusStrategy : ICaseStatusStrategy
    {
        /// <inheritdoc />
        public string GetStatusDefault()
        {
            return Case112Status.InProgress.ToString();
        }

        /// <inheritdoc />
        public string GetStatusInProgress()
        {
            return Case112Status.InProgress.ToString();
        }

        /// <inheritdoc />
        public Result<string> SetStatusInProgress(Case currentCase)
        {
            if (!IsStatusClosed(currentCase))
            {
                currentCase.Status = Case112Status.InProgress.ToString();
                
                return Result.Success(currentCase.Status);
            }

            return Result.Failure<string>(ErrorCodes.UnableToSetCaseStatus, currentCase.Status);
        }

        /// <inheritdoc />
        public Result<string> SetStatusClosed(Case currentCase)
        {
            if (currentCase.Status == GetStatusInProgress())
            {
                currentCase.Status = Case112Status.Closed.ToString();
                
                return Result.Success(currentCase.Status);
            }
            
            return Result.Failure<string>(ErrorCodes.UnableToSetCaseStatus, currentCase.Status);
        }

        /// <inheritdoc />
        public bool IsStatusInProgress(Case currentCase)
        {
            return currentCase.Status == Case112Status.InProgress.ToString();
        }

        /// <inheritdoc />
        public bool IsStatusClosed(Case currentCase)
        {
            return currentCase.Status == Case112Status.Closed.ToString();
        }
    }
}