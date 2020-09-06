using System.Collections.Generic;
using System.Linq;

namespace demo.DemoApi.Service.Dtos.Participant
{
    /// <summary>
    /// Класс для получения информации об участнике разговора
    /// </summary>
    public class ParticipantInfoCreator
    {
        /// <summary>
        /// Получение суммарную информации об участнике разговора
        /// </summary>
        public static string GetParticipantInfo(Domain.Entities.Participant participant)
        {
            if (participant == null)
            {
                return null;
            }

            var shortParticipantInfo = participant.ParticipantName ?? participant.ParticipantExtension;

            var contact = participant as Domain.Entities.Contact;
            if (contact != null)
            {
                var participantInfoElements = new List<string>();
                if (!string.IsNullOrEmpty(contact.Organization))
                {
                    participantInfoElements.Add(contact.Organization);
                }

                if (!string.IsNullOrEmpty(contact.Position))
                {
                    participantInfoElements.Add(contact.Position);
                }

                if (!string.IsNullOrEmpty(contact.Name))
                {
                    participantInfoElements.Add(contact.Name);
                }

                return participantInfoElements.Any() ? string.Join(" - ", participantInfoElements) : shortParticipantInfo;
            }

            return shortParticipantInfo;
        }

        /// <summary>
        /// Получить подробную информацию об участнике вызова
        /// </summary>
        public static ParticipantInfoDto GetParticipantInfoDto(Domain.Entities.Participant participant)
        {
            if (participant == null)
            {
                return null;
            }

            var participantInfo = new ParticipantInfoDto
            {
                Id = participant.Id,
                Name = participant.ParticipantName,
                Extension = participant.ParticipantExtension,
                ParticipantInfo = GetParticipantInfo(participant)
            };

            if (participant is Domain.Entities.Contact contact)
            {
                participantInfo.Name = contact.Name;
                participantInfo.Organization = contact.Organization;
                participantInfo.Position = contact.Position;
            }

            return participantInfo;
        }
    }
}
