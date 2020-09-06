using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace demo.DemoApi.Service.Dtos.CaseFolder.CaseFolderList
{
    /// <summary>
    /// Dto для запросов на получения данных карточек событий
    /// </summary>
    public class CaseFolderListItemDto
    {
        /// <summary>
        /// Id поля с описанием объекта.
        /// </summary>
        public static Guid ObjectFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000006");

        /// <summary>
        /// Id поля с описанием муниципалитет.
        /// </summary>
        public static Guid MunicipalityFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000007");

        /// <summary>
        /// Id поля с описанием населенного пункта.
        /// </summary>
        public static Guid LocalityFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000008");

        /// <summary>
        /// Id поля с описанием улицы.
        /// </summary>
        public static Guid StreetFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000009");

        /// <summary>
        /// Id поля с описанием дома.
        /// </summary>
        public static Guid HomeFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000010");

        /// <summary>
        /// Id поля с описанием корпуса.
        /// </summary>
        public static Guid HousingFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000012");

        /// <summary>
        /// Id поля с описанием подъезда.
        /// </summary>
        public static Guid PorchFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000013");

        /// <summary>
        /// Id поля с описанием этажа.
        /// </summary>
        public static Guid FloorFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000014");

        /// <summary>
        /// Id поля с описанием квартиры.
        /// </summary>
        public static Guid ApartmentFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000015");

        /// <summary>
        /// Id поля с описанием кода.
        /// </summary>
        public static Guid CodeFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000016");

        /// <summary>
        /// Id поля с описанием ФИО пострадавшего.
        /// </summary>
        public static Guid VictimFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000017");

        /// <summary>
        /// Id поля с описанием ФИО заявителя.
        /// </summary>
        public static Guid ApplicantFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000002");

        /// <summary>
        /// Id инцидента
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Карточки инцидента
        /// </summary>
        public List<CaseListItemDto> Cases { get; set; }

        /// <summary>
        /// Создать DTO из сущности
        /// </summary>
        public static CaseFolderListItemDto MapFromCaseEntity(Domain.Entities.CaseFolder caseFolder)
        {
            if (caseFolder == null)
            {
                return null;
            }

            var dto = new CaseFolderListItemDto
            {
                Id = caseFolder.Id, 
                Cases = new List<CaseListItemDto>()
            };

            var caseFolderData = JsonConvert.DeserializeObject<List<CaseFolderDataDto>>(caseFolder.Data);
            string address = null;
            string applicant = null;
            string victim = null;

            if (caseFolderData != null && caseFolderData.Any())
            {
                var addressItems = new List<string>
                {
                    caseFolderData.FirstOrDefault(c => c.FieldId == ObjectFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == MunicipalityFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == LocalityFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == StreetFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == HomeFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == HousingFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == PorchFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == FloorFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == ApartmentFieldId)?.Value,
                    caseFolderData.FirstOrDefault(c => c.FieldId == CodeFieldId)?.Value
                };

                address = string.Join(", ", addressItems.Where(a => !string.IsNullOrEmpty(a)));
                applicant = caseFolderData.FirstOrDefault(c => c.FieldId == ApplicantFieldId)?.Value;
                victim = caseFolderData.FirstOrDefault(c => c.FieldId == VictimFieldId)?.Value;
            }

            var orderedCases = caseFolder.Cases.OrderByDescending(x => x.Created).ToList();
            foreach (var caseItem in orderedCases)
            {
                dto.Cases.Add(CaseListItemDto.MapFromCaseEntity(caseItem, address, applicant, victim));
            }

            var firstCasesCreated = orderedCases.FirstOrDefault();
            if (firstCasesCreated != null)
            {
                dto.CreatedDate = firstCasesCreated.Created;
            }

            return dto;
        }
    }
}