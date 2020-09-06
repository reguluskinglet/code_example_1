using System;
using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Role
{
    /// <summary>
    /// Dto рабочего задания
    /// </summary>
    public class WorkChoiceDto
    {
        /// <summary>
        /// Идентификатор рабочего задания
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя рабочего задания
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Роли рабочего задания
        /// </summary>
        public List<RoleDto> Roles { get; set; }
    }
}
