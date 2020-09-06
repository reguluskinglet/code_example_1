using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Service.Dtos.Calls;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для пользователя
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Создает новый экземпляр <see cref="UserDto"/>.
        /// </summary>
        public UserDto()
        {
            CurrentCallStates = new List<CallStatusDto>();
        }

        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество пользователя
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Признак того, что оператор сейчас залогинен
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Extension участника звонка
        /// </summary>
        [RegularExpression(@"^\d+$")]
        public string Extension { get; set; }

        /// <summary>
        /// Статусы текущего вызова.
        /// </summary>
        public List<CallStatusDto> CurrentCallStates { get; set; }

        /// <summary>
        /// Идентификатор текущего вызова
        /// </summary>
        public Guid? CallId { get; set; }

        /// <summary>
        /// Тип подключения.
        /// </summary>
        public string ConnectionMode { get; set; }

        /// <summary>
        /// Установить статусы текущего принятого вызова
        /// </summary>
        public void SetCurrentCallStates(CallDto activeCall)
        {
            if (activeCall == null || activeCall.Status != CallStatus.Start)
            {
                return;
            }

            CallId = activeCall.Id;
            ConnectionMode = activeCall.ConnectionMode.ToString();

            if (activeCall.OnHold)
            {
                CurrentCallStates.Add(CallStatusDto.IsOnHold);
            }
            else
            {
                CurrentCallStates.Add(CallStatusDto.CallActive);
            }

            if (activeCall.IsMutedMicrophone)
            {
                CurrentCallStates.Add(CallStatusDto.IsMutedMicrophone);
            }

            if (activeCall.Isolated)
            {
                CurrentCallStates.Add(CallStatusDto.IsIsolated);
            }
        }
    }
}
