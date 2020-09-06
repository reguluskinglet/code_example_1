using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Информация для удаления местонахождения абонентского устройства на GisFacade
    /// </summary>
    public class DeleteApplicantLocationDto
    {
        /// <summary>
        /// Идентификатор CaseFolder
        /// </summary>
        public Guid CaseFolderId { get; set; }
    }
}
