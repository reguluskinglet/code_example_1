using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Статусы для всех Case в CaseFolder
    /// </summary>
    public class CaseStatusesInfoDto
    {
        /// <summary>
        /// Возможность закрытия карточки
        /// </summary>
        public bool CanCloseCaseCard { get; set; }
        
        /// <summary>
        /// Информация о статусе для каждой Case в CaseFolder 
        /// </summary>
        public List<CaseStatusDto> Statuses { get; set; }

    }
}