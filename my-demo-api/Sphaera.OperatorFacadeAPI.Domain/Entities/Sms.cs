using System;
using Newtonsoft.Json;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.Entities.SmsMetadata;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Сущность вызов.
    /// </summary>
    public class Sms : AggregateRoot
    {
        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </remarks>
        public Sms()
        {
        }

        /// <inheritdoc />
        public Sms(Guid id) : base(id)
        {
        }

        /// <summary>
        /// Текущий статус звонка
        /// </summary>
        public SmsStatus Status { get; set; }

        /// <summary>
        /// Время поступления звонка.
        /// </summary>
        public DateTime ArrivalDateTime { get; set; }

        /// <summary>
        /// Время ответа на звонок.
        /// </summary>
        public DateTime? AcceptDateTime { get; private set; }

        /// <summary>
        /// Текст смс.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// Заявитель.
        /// </summary>
        public Participant Applicant { get; set; }

        /// <summary>
        /// Данные о местоположении заявителя.
        /// </summary>
        public virtual string LocationMetadata { get; set; }

        /// <summary>
        /// Дата и время формирования получения смс.
        /// </summary>
        public virtual string Timestamp { get; set; }

        /// <summary>
        /// Получить десериализованные данные о местоположение абонентского устройства, с которого пришла СМС
        /// </summary>
        /// <returns></returns>
        public Result<SmsLocationData> GetLocationData()
        {
            try
            {
                var smsLocationData = JsonConvert.DeserializeObject<SmsLocationData>(LocationMetadata);
                return Result.Success(smsLocationData);
            }
            catch (Exception)
            {
                return Result.Failure<SmsLocationData>(ErrorCodes.UnableToGetLocationData,$"SMS. Не удалось десериализовать данные о местонахождении абонентского устройства. {LocationMetadata}");
            }
        }
    }
}
