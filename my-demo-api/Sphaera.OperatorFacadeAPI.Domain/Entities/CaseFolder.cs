using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.Entities.SmsMetadata;
using demo.DemoApi.Domain.Enums;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Domain.ValueObjects;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Запись с инцидентом
    /// </summary>
    public class CaseFolder : AggregateRoot
    {
        /// <summary>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </summary>
        public CaseFolder(): base(Guid.NewGuid())
        {
            Cases = new List<Case>();

            Status = CaseFolderStatus.InProgress;
            Latitude = 37.403908;
            Longitude = 55.894293;
            Data =
                "[{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000001\",\"FieldId\":\"81fa8ce9-e2e6-0001-0003-000000000004\",\"Value\":\"84955489877\"},{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000002\",\"FieldId\":\"81fa8ce9-e2e6-0001-0001-000000000006\",\"Value\":\"Газовая котельная\"},{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000002\",\"FieldId\":\"81fa8ce9-e2e6-0001-0001-000000000007\",\"Value\":\"Химки ГО\"},{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000002\",\"FieldId\":\"81fa8ce9-e2e6-0001-0001-000000000008\",\"Value\":\"Химки\"},{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000002\",\"FieldId\":\"81fa8ce9-e2e6-0001-0001-000000000009\",\"Value\":\"Нагорное шоссе\"},{\"BlockId\":\"81fa8ce9-e2e6-0001-0001-000000000002\",\"FieldId\":\"81fa8ce9-e2e6-0001-0001-000000000010\",\"Value\":\"6\"}]";
        }

        /// <summary>
        /// Список звонков
        /// </summary>
        public virtual IList<Case> Cases { get; private set; }

        /// <summary>
        /// CaseFolder связан с Call в случае, если Call Это SMS
        /// </summary>
        public virtual Sms Sms { get; set; }

        /// <summary>
        /// Данные для заполнения полей карточки
        /// </summary>
        public virtual string Data { get; set; }

        /// <summary>
        /// Данные о широте
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Данные о долготе
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Статус CaseFolder
        /// </summary>
        public CaseFolderStatus Status { get; set; }

        /// <summary>
        /// Создание и добавление новой карточки в Folder
        /// Case добавляется только в случае, если такого типа еще нет среди существующих Case
        /// </summary>
        /// <param name="caseType"></param>
        /// <param name="userId"></param>
        public Result<Guid> AddCaseCard(CaseType caseType, Guid userId)
        {
            var oldCaseCard = Cases.FirstOrDefault(x => x.Type.Id == caseType.Id);

            if (oldCaseCard != null)
            {
                oldCaseCard.AddToUser(userId);
                return Result.Success(oldCaseCard.Id);
            }

            var caseCard = new Case(caseType, this);
            Cases.Add(caseCard);
            caseCard.AddToUser(userId);

            return Result.Success(caseCard.Id);
        }

        /// <summary>
        /// Получение значения поля карточки
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        public string GetFieldValue(Guid fieldId)
        {
            var caseDataList = JsonConvert.DeserializeObject<List<CaseData>>(Data);

            var value = caseDataList.Find(x => x.FieldId == fieldId)?.Value;

            return value;
        }

        /// <summary>
        /// Обновление данных поля карточки
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Result UpdateField(Guid fieldId, string value)
        {
            var caseDataList = new List<CaseData>();

            if (Data != null)
            {
                caseDataList = JsonConvert.DeserializeObject<List<CaseData>>(Data);
            }

            var caseData = caseDataList.Find(x => x.FieldId == fieldId);

            if (caseData == null)
            {
                caseData = new CaseData { FieldId = fieldId };
                caseDataList.Add(caseData);
            }

            caseData.Value = value;

            var serializedData = JsonConvert.SerializeObject(caseDataList);
            Data = serializedData;

            return Result.Success();
        }

        /// <summary>
        /// Список Заголовков
        /// </summary>
        public virtual IList<Title> GetCaseTitles()
        {
            return Cases.Select(x => new Title
            {
                CaseId = x.Id,
                Text = x.Type.GetTitle()
            }).ToList();
        }

        /// <summary>
        /// Получить карточку, доступную для указанного пользователя
        /// </summary>
        public Result<Case> GetCaseForUser(Guid userId)
        {
            var userCaseCard = Cases.FirstOrDefault(x => x.CaseUsers.Any(user => user.UserId == userId) );
            if (userCaseCard == null)
            {
                return Result.Failure<Case>(ErrorCodes.CaseFolderNotFound, $"CaseFolder for user {userId} not found");
            }

            return Result.Success(userCaseCard);
        }

        /// <summary>
        /// Заполнение карточки данными из СМС
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="sms"></param>
        /// <returns></returns>
        public Result FillSmsData(string phoneNumber, Sms sms)
        {
            var descriptionFieldGuid = Case.DescriptionFieldId;
            var descriptionResult = UpdateField(descriptionFieldGuid, sms.Text);
            if (descriptionResult.IsFailure)
            {
                return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка добаления текста СМС в поле описания события");
            }

            var phoneNumberFieldGuid = Case.NumberFieldId;
            var phoneNumberResult = UpdateField(phoneNumberFieldGuid, phoneNumber);
            if (phoneNumberResult.IsFailure)
            {
                return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка заполнения номера телефона в карточке");
            }

            try
            {
                var smsLocationData = JsonConvert.DeserializeObject<SmsLocationData>(sms.LocationMetadata);

                var applicantLatitudeFieldGuid = Case.ApplicantLatitudeFieldId;
                var applicantLatitudeResult = UpdateField(applicantLatitudeFieldGuid, smsLocationData.Position.Latitude.ToString());
                if (applicantLatitudeResult.IsFailure)
                {
                    return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка заполнения широты в карточке");
                }

                var applicantLongitudeFieldGuid = Case.ApplicantLongitudeFieldId;
                var applicantLongitudeResult = UpdateField(applicantLongitudeFieldGuid, smsLocationData.Position.Longitude.ToString());
                if (applicantLongitudeResult.IsFailure)
                {
                    return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка заполнения долготы в карточке");
                }

                var applicantLocationInfoFieldGuid = Case.LocationInfoFieldId;
                var applicantLocationInfoResult = UpdateField(applicantLocationInfoFieldGuid, smsLocationData.GetFormattedLocationInfoString);
                if (applicantLocationInfoResult.IsFailure)
                {
                    return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка заполнения информации о местоположении в карточке");
                }

                var applicantLocationTimestampFieldGuid = Case.TimestampFieldId;
                var applicantLocationTimestampResult = UpdateField(applicantLocationTimestampFieldGuid, sms.Timestamp);
                if (applicantLocationTimestampResult.IsFailure)
                {
                    return Result.Failure(ErrorCodes.UnableToFillSmsData, "Ошибка заполнения информации о времени получения смс в карточке");
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.UnableToFillSmsData, $"Обнаружен неправильный формат {nameof(SmsLocationData)} из смс: {sms.LocationMetadata}");
            }

            return Result.Success();
        }

        /// <summary>
        /// Перевести CaseFolder в статус Closed
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            if (AllCasesClosed())
            {
                Status = CaseFolderStatus.Closed;
            }
        }

        /// <summary>
        /// Закрыт ли CaseFolder
        /// </summary>
        public bool IsClosed => Status == CaseFolderStatus.Closed;

        /// <summary>
        /// Перевести CaseFolder в статус InProgres
        /// </summary>
        public void Reopen()
        {
            Status = CaseFolderStatus.InProgress;
        }

        private bool AllCasesClosed()
        {
            return Cases.All(x => x.Type.StatusStrategy.IsStatusClosed(x));
        }

        /// <summary>
        /// Получить данные о смс с учетом текущих координат заявителя из карточки
        /// </summary>
        /// <returns></returns>
        public Result<SmsLocationData> GetLocationData()
        {
            var locationData = SmsLocationData.CreateEmpty();

            if (Sms != null)
            {
                Result<SmsLocationData> applicantLocationDataResult = Sms.GetLocationData();
                if (applicantLocationDataResult.IsFailure)
                {
                    return applicantLocationDataResult;
                }

                locationData = applicantLocationDataResult.Value;
            }

            var applicantLatitudeStr = GetFieldValue(Case.ApplicantLatitudeFieldId);
            if (!double.TryParse(applicantLatitudeStr, out var applicantLatitude))
            {
                return Result.Failure<SmsLocationData>(ErrorCodes.UnableToGetLocationData, $"CaseFolder. Не удалось распарсить Latitude заявителя из карточки. {applicantLatitudeStr}");
            }

            var applicantLongitudeStr = GetFieldValue(Case.ApplicantLongitudeFieldId);
            if (!double.TryParse(applicantLongitudeStr, out var applicantLongitude))
            {
                return Result.Failure<SmsLocationData>(ErrorCodes.UnableToGetLocationData, $"CaseFolder. Не удалось распарсить Longitude заявителя из карточки. {applicantLongitudeStr}");
            }

            locationData.Position.Latitude = applicantLatitude;
            locationData.Position.Longitude = applicantLongitude;

            return Result.Success(locationData);
        }
    }
}
