using System;

namespace demo.DemoApi.Service.Dtos.Role
{
    /// <summary>
    /// Dto роли
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// Идентификатор роли
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя роли
        /// </summary>
        public string Name { get; set; }
    }
}
