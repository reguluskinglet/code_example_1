using System;
using System.Linq;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.Domain.CaseStrategy
{
    /// <summary>
    /// Опеределение контекста работы со стратегиями статусов карточек.
    /// </summary>
    public class CaseTypeStatusContext
    {
        private readonly CaseType _caseType;
        private static Guid CaseTypeId112 => new Guid("6a9f90c4-2b7e-4ec3-a38f-000000000112");
        private static Guid FireDepartmentCaseTypeId => new Guid("99c543c5-0dd7-4ebc-a87d-000000000002");
        
        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="caseType"></param>
        public CaseTypeStatusContext(CaseType caseType)
        {
            _caseType = caseType;
        }

        /// <summary>
        /// Получить стратегию обработки статусов по типу карточки.
        /// </summary>
        /// <returns></returns>
        public ICaseStatusStrategy GetCaseStatusStrategy()
        {
            switch (_caseType.Id)
            {
                case var x when (x == CaseTypeId112):
                    return new Case112StatusStrategy();
                case var x when (x == FireDepartmentCaseTypeId):
                    return new CaseFireDepartmentStatusStrategy();
                default:
                    throw new Exception($"Не удалось найти стратегию для карточки типа с Id {_caseType.Id}");
            }
        }
    }
}