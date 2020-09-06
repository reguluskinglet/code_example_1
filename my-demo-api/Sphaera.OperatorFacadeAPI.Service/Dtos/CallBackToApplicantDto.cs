using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для запроса на перезвон заявителю из ГИС.
    /// </summary>
    public class CallBackToApplicantDto
    {
        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid CaseFolderId { get; set; }
    }
}
