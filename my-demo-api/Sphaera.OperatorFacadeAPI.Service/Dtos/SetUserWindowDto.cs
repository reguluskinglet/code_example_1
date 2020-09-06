using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Параметры настроек окон клиента
    /// </summary>
    public class SetUserWindowDto
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Настройки
        /// </summary>
        public string WindowSettings { get; set; }
    }
}
