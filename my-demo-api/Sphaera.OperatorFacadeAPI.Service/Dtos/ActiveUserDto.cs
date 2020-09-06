using System.Collections.Generic;
using System.Linq;
using demo.CallManagement.HttpContracts.Dto;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto активного пользователя
    /// </summary>
    public class ActiveUserDto : UserDto
    {
        /// <summary>
        /// Линии в который пользователь участвует
        /// </summary>
        public List<ConnectedLineDto> ConnectedLines { get; set; }

        /// <summary>
        /// Создать Dto модель из сущности.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="actualUserCalls"></param>
        /// <returns></returns>
        public static ActiveUserDto MapFromQueueEntity(UserDto entity, List<CallClientDto> actualUserCalls)
        {
            if (entity == null)
            {
                return null;
            }

            var model = new ActiveUserDto
            {
                Id = entity.Id,
                Extension = entity.Extension,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                MiddleName = entity.MiddleName,
                IsActive = entity.IsActive,
                ConnectedLines = actualUserCalls.Select(call => new ConnectedLineDto
                {
                    Mode = (ConnectionMode)(int)call.ConnectionMode,
                    LineId = call.Line.Id
                }).ToList()
            };

            return model;
        }
    }
}
