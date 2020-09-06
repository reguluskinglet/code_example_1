using System;
using System.Collections.Generic;
using System.Linq;
using demo.CallManagement.HttpContracts.Dto;
using demo.DemoApi.Service.Dtos.Enums;
using demo.DemoApi.Service.Dtos.Participant;

namespace demo.DemoApi.Service.Dtos.Line
{
    /// <summary>
    /// Dto линии вызова для отображения в UI.
    /// </summary>
    public class LineViewDto
    {
        /// <summary>
        /// Идентификатор линии.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор инцидента.
        /// </summary>
        public Guid? CaseFolderId { get; set; }

        /// <summary>
        /// Операторы занятые в линии.
        /// </summary>
        public List<LineParticipantInfoDto> Participants { get; set; }

        /// <summary>
        /// Создать Dto из ClientDto
        /// </summary>
        public static LineViewDto MapFromClientDto(LineViewClientDto clientDto)
        {
            return new LineViewDto
            {
                Id = clientDto.Id,
                CaseFolderId = clientDto.CaseFolderId,
                Participants = clientDto.Calls.Select(x => new LineParticipantInfoDto
                {
                    Id = x.ParticipantId,
                    CallId = x.CallId,
                    CallStates = x.CallStates.Select(c => (CallStatusDto)(int)c).ToList(),
                    ConnectionMode = x.ConnectionMode,
                    CanChangeIsolationStatus = x.CanChangeIsolationStatus
                }).ToList()
            };
        }
    }
}
