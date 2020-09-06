using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для изменения статуса пользователя
    /// </summary>
    public class ChangeActivityStatusDto
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Статус активности пользователя
        /// </summary>
        public bool IsActive { get; set; }
    }
}
