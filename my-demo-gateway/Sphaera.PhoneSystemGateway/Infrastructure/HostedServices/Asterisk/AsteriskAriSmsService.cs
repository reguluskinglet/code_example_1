using System;
using System.Text;
using AsterNET.ARI;
using AsterNET.ARI.Models;
using Newtonsoft.Json;
using demo.MessageContracts.Dtos.Sms;
using demo.MessageContracts.DemoGateway;
using demo.Monitoring.Logger;
using demo.DemoGateway.Dtos.Sms;
using demo.DemoGateway.Infrastructure.HostedServices.Dtos;
using demo.DemoGateway.Infrastructure.Parsers.Sms;
using demo.Transit.Publisher;

namespace demo.DemoGateway.Infrastructure.HostedServices.Asterisk
{
    /// <summary>
    /// Сервис по обработки входящих СМС
    /// </summary>
    public class AsteriskAriSmsService
    {
        private readonly ILogger _logger;
        private readonly IQueueSender _queueSender;
        private readonly SmsParser _smsParser;

        /// <inheritdoc />
        public AsteriskAriSmsService(ILogger logger, IQueueSender queueSender, SmsParser smsParser)
        {
            _logger = logger;
            _queueSender = queueSender;
            _smsParser = smsParser;
        }

        /// <summary>
        /// Подписка на событие ChannelUserEvent и обработка только SMS
        /// </summary>
        public void OnChannelUserEvent(IAriClient sender, ChannelUsereventEvent e)
        {
            if (!e.Eventname.ToLower().Equals("sms_received"))
            {
                _logger.Debug($"Пришло событие {e.Eventname}");
                return;
            }

            _logger.Debug($"UserEvent: {JsonConvert.SerializeObject(e.Userevent)}");

            ChannelUserEventDto deserializedUserEvent;
            try
            {
                deserializedUserEvent = JsonConvert.DeserializeObject<ChannelUserEventDto>(e.Userevent.ToString());
            }
            catch (Exception ex)
            {
                _logger.Warning($"Не удалось десериализовать {nameof(e.Userevent)} {e.Userevent}", ex);
                return;
            }

            if (string.IsNullOrEmpty(deserializedUserEvent.SenderExtension))
            {
                _logger.Warning("AsteriskAriSmsService. Sms не содежрит отправителя");
                return;
            }

            string messageBody;
            try
            {
                var bytes = Convert.FromBase64String(deserializedUserEvent.Body);
                messageBody = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                _logger.Debug($"SMS Body: {JsonConvert.SerializeObject(messageBody)}");
            }
            catch (Exception ex)
            {
                _logger.Warning("Ошибка конвертации sms body из base64", ex);
                return;
            }

            ProcessSms(messageBody, deserializedUserEvent.SenderExtension);
        }

        private void ProcessSms(string body, string senderExtension)
        {
            var smsTextResult = _smsParser.GetSmsText(body);
            if (smsTextResult.IsFailure)
            {
                _logger.Warning($"Ошибка парсинга текста из sms body. {smsTextResult.ErrorMessage}");
                return;
            }

            var metadataResult = _smsParser.GetMetadata(body);
            if (metadataResult.IsFailure)
            {
                _logger.Warning($"Не удалось получить метаданные из sms body. {metadataResult.ErrorMessage}");
            }

            var metadata = metadataResult.Value;

            var smsId = Guid.NewGuid();

            NotifyAboutNewSms(smsId, senderExtension, smsTextResult.Value, metadata);
        }

        private string CreateResultMessage(string text, string position, string timestamp)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Текст сообщения: {text}");
            sb.AppendLine($"Координаты места нахождения: {position}");
            sb.AppendLine($"Время определения места нахождения: {timestamp}");

            return sb.ToString();
        }

        private void NotifyAboutNewSms(Guid smsId, string senderExtension, string smsText, SmsMetadata smsMetadata)
        {
            _logger.Debug($"Отправка SMS в шину. SenderExtension: {senderExtension}");

            var position = smsMetadata.Position == null
                ? null
                : new Position
                {
                    Latitude = smsMetadata.Position.Latitude,
                    Longitude = smsMetadata.Position.Longitude
                };
            var smsEvent = new IncomingSmsIntegrationEvent
            {
                CallerExtension = senderExtension,
                Sms = new Sms
                {
                    Message = smsText,
                    Timestamp = smsMetadata.Timestamp,
                    Location = new Location
                    {
                        InnerRadius = smsMetadata.InnerRadius,
                        OpeningAngle = smsMetadata.OpeningAngle,
                        OuterRadius = smsMetadata.OuterRadius,
                        Radius = smsMetadata.Radius,
                        StartAngle = smsMetadata.StartAngle,
                        Position = position
                    }
                },
                SmsId = smsId
            };

            _queueSender.Publish(smsEvent);
        }
    }
}