using System;
using demo.CallManagement.HttpContracts.Dto;
using demo.DemoApi.Service.Dtos.Enums;

namespace demo.DemoApi.Service.Dtos.Calls
{
    /// <summary>
    /// Dto для журнала звонков
    /// </summary>
    public class JournalCallsDto
    {
        /// <summary>
        /// Тип звонка
        /// </summary>
        public CallType Type { get; set; }

        /// <summary>
        /// Время поступления звонка
        /// </summary>
        public DateTime ArrivalDateTime { get; set; }

        /// <summary>
        /// Время ответа на звонок
        /// </summary>
        public DateTime? AcceptDateTime { get; set; }

        /// <summary>
        /// Номер звонка (вместе с видом контакта, например, "20095443 - мобильный")
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Extension звонка
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Должность контакта
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Организация, к которой относится контакт
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// ФИО контакта
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// Создать DTO из сущности
        /// </summary>
        public static JournalCallsDto MapFromClientCall(CallListItemClientDto call, Domain.Entities.Participant participant)
        {
            var journalCallItem = new JournalCallsDto
            {
                ArrivalDateTime = call.ArrivalDateTime,
                AcceptDateTime = call.AcceptDateTime,
                Type = (CallType)call.Type,
                Number = participant?.ParticipantExtension,
                Extension = participant?.ParticipantExtension
            };

            if (participant is Domain.Entities.Contact contact)
            {
                if (!string.IsNullOrEmpty(contact.ContactRouteName))
                {
                    journalCallItem.Number += " - " + contact.ContactRouteName;
                }

                journalCallItem.Organization = contact.Organization;
                journalCallItem.Position = contact.Position;
                journalCallItem.ContactName = contact.Name;
            }

            return journalCallItem;
        }
    }
}
