using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using demo.DDD;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.Dtos.Case;
using demo.DemoApi.Service.Dtos.Inbox;
using demo.DemoApi.Service.Dtos.Index;
using demo.DemoApi.Service.Hubs;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис отправки сообщений в PhoneHub.
    /// </summary>
    public class PhoneHubMessageService
    {
        private readonly ILogger _logger;
        private readonly IHubContext<PhoneHub> _phoneHubContext;

        /// <summary>
        /// Создает новый экземпляр <see cref="PhoneHubMessageService"/>.
        /// </summary>
        public PhoneHubMessageService(
            ILogger logger,
            IHubContext<PhoneHub> phoneHubContext)
        {
            _logger = logger;
            _phoneHubContext = phoneHubContext;
        }

        /// <summary>
        /// Оповещение клиентов о завершении звонка оператора в активной линии.
        /// </summary>
        public async Task NotifyUsersAboutCallEnded(Guid? userId, Guid? lineId)
        {
            _logger.Debug("Оповещение клиентов о завершении звонка оператора");

            await _phoneHubContext.Clients.All.SendAsync("userCallEnded",
                new
                {
                    userId,
                    lineId
                });
        }

        /// <summary>
        /// Оповестить пользователей об успешно принятом звонке.
        /// </summary>
        public virtual async Task NotifyUsersAboutAcceptedCall(Guid? userId, Guid lineId, Guid? caseFolderId)
        {
            _logger.Debug($"NotifyUsersAboutAcceptedCall. Begin. {userId} lineId {lineId}");

            await _phoneHubContext.Clients.All.SendAsync("operatorAccepted",
                new
                {
                    call = new
                    {
                        @operator = new
                        {
                            id = userId
                        }
                    },
                    line = new
                    {
                        id = lineId
                    },
                    caseFolder = new
                    {
                        id = caseFolderId
                    }
                });
        }

        /// <summary>
        /// Сообщить пользователям о том, что пришло СМС
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task NotifyUsersAboutAcceptedSms(Guid caseFolderId, Guid userId)
        {
            _logger.Debug("Оповещение об успешном принятии СМС оператором");

            await _phoneHubContext.Clients.All.SendAsync("smsAccepted",
                new
                {
                    caseFolder = new
                    {
                        id = caseFolderId
                    },
                    user = new
                    {
                        id = userId
                    }
                });
        }

        /// <summary>
        /// Оповестить пользователя об отправленном внешнем вызове.
        /// </summary>
        public async Task NotifyUserAboutStartedExternalCall(Guid userId, Guid lineId)
        {
            _logger.Debug("Оповещение клиента об отправленном внешнем вызове");

            await _phoneHubContext.Clients.All.SendAsync("externalCallStarted",
                new
                {
                    call = new
                    {
                        @operator = new
                        {
                            id = userId
                        }
                    },
                    line = new
                    {
                        id = lineId
                    }
                });
        }

        /// <summary>
        /// Оповещение клиентов о том, что участник разговора, которому звонит пользователь, принял вызов.
        /// </summary>
        public async Task NotifyUsersAboutExternalCallAccepted(Guid lineId, Guid? caseFolderId, Guid userId)
        {
            _logger.Debug("Оповещение клиентов о том, что заявитель или контакт принял вызов от пользователя");

            await _phoneHubContext.Clients.All.SendAsync("externalCallAccepted",
                new
                {
                    line = new
                    {
                        id = lineId
                    },
                    caseFolder = new
                    {
                        id = caseFolderId
                    },
                    call = new
                    {
                        @operator = new
                        {
                            id = userId
                        }
                    }
                });
        }

        /// <summary>
        /// Отправка оповещения об изменениях в конкретной очереди.
        /// </summary>
        /// <param name="inboxId">Id очереди</param>
        /// <returns></returns>
        public async Task NotifyUsersAboutInboxUpdate(Guid? inboxId)
        {
            _logger.Verbose("NotifyUsersAboutInboxUpdate. Begin");
            
            if (!inboxId.HasValue)
            {
                return;
            }

            await _phoneHubContext.Clients.All.SendAsync("inboxUpdate",
                new InboxUpdateDto
                {
                    InboxId = inboxId.Value
                });
        }

        /// <summary>
        /// Отправка оповещения о добавлении, удалении или изменении сущности.
        /// </summary>
        public async Task NotifyUsersAboutEntityChanged(IEnumerable<TrackingEvent> trackingEvents)
        {
            foreach (TrackingEvent trackingEvent in trackingEvents.OrderBy(t => t.TransactionDate))
            {
                var methodName = "EntityChanged";
                var type = ToCamelCase(trackingEvent.EntityName) + trackingEvent.OperationType;
                _logger.Verbose($"NotifyUsersAboutEntityChanged. Type: {type}, Id: {trackingEvent.EntityId}, AggreagateId: {trackingEvent.AggreagateId}, UserId: {trackingEvent.UserId}");

                await _phoneHubContext.Clients.All.SendAsync(methodName,
                    new
                    {
                        id = trackingEvent.EntityId,
                        aggreagateId = trackingEvent.AggreagateId,
                        userId = trackingEvent.UserId,
                        type = type
                    });
            }
        }


        /// <summary>
        /// Отправка сообщения о добавлении местоположения заявителя.
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <param name="caseId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task NotifyUserAboutApplicantLocationUpdateAsync(Guid caseFolderId, Guid caseId, Guid userId)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnApplicantLocationUpdated",
                new
                {
                    CaseFolderId = caseFolderId,
                    CaseId = caseId,
                    UserId = userId
                });
        }

        /// <summary>
        /// Оповестить всех клиентов об изменении индекса инцидента
        /// </summary>
        public async Task NotifyAboutIndexUpdated(Guid caseId, Guid indexId)
        {
            await _phoneHubContext.Clients.All.SendAsync("indexUpdated", new CaseIndexDto(caseId, indexId));
        }

        /// <summary>
        /// Оповестить всех клиентов о необходимости загрузки заголовков карточек.
        /// </summary>
        public async Task NotifyAboutNeedToUpdateCaseTitlesAsync(Guid? caseFolderId)
        {
            await _phoneHubContext.Clients.All.SendAsync("UpdateCaseTitles",
                new
                {
                    caseFolderId
                });
        }

        /// <summary>
        /// Оповестить клиентов о том, что значение в поле карточки было изменено.
        /// </summary>
        public async Task NotifyClientsAboutFieldChangedAsync(CaseFieldChangeEventDto caseFieldChangeEventDto)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnCaseFieldChanged", caseFieldChangeEventDto);
        }

        /// <summary>
        /// Оповестить клиентов о том, что в данный момент меняется поле карточки.
        /// </summary>
        public async Task NotifyClientsAboutActiveFieldAsync(CaseFieldChangeEventDto caseFieldChangeEventDto)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnActiveFieldUpdating", caseFieldChangeEventDto);
        }

        /// <summary>
        /// Оповестить клиентов о том, что план реагирования обновился.
        /// </summary>
        public async Task NotifyClientsAboutPlanUpdateAsync(Guid caseId, Guid planId)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnPlanUpdated",
                new
                {
                    CaseId = caseId,
                    PlanId = planId
                });
        }

        /// <summary>
        /// Оповестить клиентов об обновлении местоположения происшествия.
        /// </summary>
        public async Task NotifyClientsAboutLocationUpdateAsync(Guid caseFolderId, Guid userId)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnLocationUpdated",
                new
                {
                    CaseFolderId = caseFolderId,
                    OperatorId = userId
                });
        }

        /// <summary>
        /// Оповестить клиентов об изменении координат инцидента.
        /// </summary>
        public async Task NotifyClientsAboutIncidentLocationUpdateAsync(Guid caseFolderId, Guid userId)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnIncidentLocationUpdated",
                new
                {
                    CaseFolderId = caseFolderId,
                    UserId = userId
                });
        }

        /// <summary>
        /// Оповестить клиентов об изменении полей с координатами.
        /// </summary>
        public async Task NotifyClientsAboutLocationActiveFieldAsync(Guid userId, Guid caseFolderId, CoordinateFieldType coordinateFieldType)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnActiveLocationFieldUpdated",
                new
                {
                    OperatorId = userId,
                    CaseFolderId = caseFolderId,
                    FieldType = coordinateFieldType,
                });
        }

        /// <summary>
        /// Оповестить клиентов об изменении местоположения абонента.
        /// </summary>
        public async Task NotifyClientsAboutApplicantLocationUpdateAsync(Guid caseFolderId, Guid userId)
        {
            await _phoneHubContext.Clients.All.SendAsync("OnApplicantLocationUpdated",
                new
                {
                    CaseFolderId = caseFolderId,
                    UserId = userId
                });
        }

        private static string ToCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }

            return str;
        }
    }
}
