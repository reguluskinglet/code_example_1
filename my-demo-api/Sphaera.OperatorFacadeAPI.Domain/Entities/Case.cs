using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using demo.DDD;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Карточка оператора
    /// </summary>
    public class Case : BaseEntity
    {
        /// <summary>
        /// Id поля с описанием инцидента.
        /// </summary>
        public static Guid DescriptionFieldId => new Guid("81fa8ce9-e2e6-0001-0001-000000000001");

        /// <summary>
        /// Id поля с номером.
        /// </summary>
        public static Guid NumberFieldId => new Guid("81fa8ce9-e2e6-0001-0003-000000000004");

        /// <summary>
        /// Id поля с широтой.
        /// </summary>
        public static Guid ApplicantLatitudeFieldId => new Guid("81fa8ce9-e2e6-0001-0003-000000000006");

        /// <summary>
        /// Id поля с долготой.
        /// </summary>
        public static Guid ApplicantLongitudeFieldId => new Guid("81fa8ce9-e2e6-0001-0003-000000000007");

        /// <summary>
        /// id поля с информацией местоположения.
        /// </summary>
        public static Guid LocationInfoFieldId => new Guid("81fa8ce9-e2e6-0001-0003-000000000005");

        /// <summary>
        /// id поля даты и времени отпрфвки смс.
        /// </summary>
        public static Guid TimestampFieldId => new Guid("81fa8ce9-e2e6-0001-0003-000000000008");

        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя.
        /// </remarks>
        public Case()
        {
        }

        /// <inheritdoc />
        internal Case(CaseType type, CaseFolder caseFolder) : base(Guid.NewGuid())
        {
            Type = type;
            Created = DateTime.UtcNow;
            Updated = DateTime.UtcNow;
            CaseFolder = caseFolder;
            Status = type.StatusStrategy.GetStatusDefault();
            CaseUsers = new List<CaseUser>();
            caseFolder.Reopen();
        }

        /// <summary>
        /// Дата создания карточки.
        /// </summary>
        public virtual DateTime Created { get; set; }

        /// <summary>
        /// Дата обновления карточки.
        /// </summary>
        public virtual DateTime Updated { get; set; }

        /// <summary>
        /// Шаблон карточки.
        /// </summary>
        public virtual CaseType Type { get; set; }

        /// <summary>
        /// Шаблон карточки.
        /// </summary>
        public virtual IList<CaseUser> CaseUsers { get; set; }

        /// <summary>
        /// Данные о значении полного пути индекса
        /// </summary>
        public string IndexValue { get; set; }

        /// <summary>
        /// Инцидент по которому создана карточка.
        /// </summary>
        public virtual CaseFolder CaseFolder { get; private set; }

        /// <summary>
        /// Данные об активированных инструкциях плана.
        /// </summary>
        public string ActivatedPlanInstructions { get; set; }

        /// <summary>
        /// Статус карточки, который замаплен на поле в БД
        /// </summary>
        private string _status;

        /// <summary>
        /// Статус карточки
        /// </summary>
        public string Status
        {
            get => _status ??= Type.StatusStrategy.GetStatusDefault();
            set => _status = value;
        }

        /// <inheritdoc />
        public override AggregateRoot Root => CaseFolder;

        /// <summary>
        /// Получение активированных инструкций плана.
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public List<ActivatedPlanInstruction> GetActivatedInstructions(Guid planId)
        {
            if (ActivatedPlanInstructions == null)
            {
                return new List<ActivatedPlanInstruction>();
            }

            var activatedInstructions = JsonConvert.DeserializeObject<List<ActivatedPlanInstruction>>(ActivatedPlanInstructions);
            return activatedInstructions.Where(x => x.PlanId == planId).ToList();
        }

        /// <summary>
        /// Обновление данных об активированных инструкциях плана.
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="instructionId"></param>
        /// <param name="activationDate"></param>
        /// <returns></returns>
        public Result UpdateActivatedInstructions(Guid planId, Guid instructionId, DateTime activationDate)
        {
            var activatedInstructions = new List<ActivatedPlanInstruction>();

            if (ActivatedPlanInstructions != null)
            {
                activatedInstructions = JsonConvert.DeserializeObject<List<ActivatedPlanInstruction>>(ActivatedPlanInstructions);
            }

            var activatedInstruction = activatedInstructions.Find(x => x.PlanId == planId && x.InstructionId == instructionId);

            if (activatedInstruction == null)
            {
                activatedInstruction = new ActivatedPlanInstruction
                {
                    PlanId = planId,
                    InstructionId = instructionId
                };
                activatedInstructions.Add(activatedInstruction);
            }

            activatedInstruction.ActivationDate = activationDate;

            var serializedData = JsonConvert.SerializeObject(activatedInstructions);
            ActivatedPlanInstructions = serializedData;

            return Result.Success();
        }

        /// <summary>
        /// Изменить статус карточки "В работе".
        /// </summary>
        public Result<string> SetStatusInProgress()
        {
            return Type.StatusStrategy.SetStatusInProgress(this);
        }

        /// <summary>
        /// Закрыта ли карточка
        /// </summary>
        /// <returns></returns>
        public bool IsInProgress()
        {
            return Type.StatusStrategy.IsStatusInProgress(this);
        }

        /// <summary>
        /// Перевести Case в статус "Закрыто"
        /// </summary>
        public Result<string> Close()
        {
            var result = Type.StatusStrategy.SetStatusClosed(this);
            if (result.IsSuccess)
            {
                CaseFolder.Close();
            }

            return result;
        }

        private bool CanBeClosedByUser(Guid caseTypeId)
        {
            return caseTypeId == Type.Id;
        }

        /// <summary>
        /// Добавлению карточки юзеру
        /// </summary>
        /// <param name="userId"></param>
        public void AddToUser(Guid userId)
        {
            if (CaseUsers.Any(x => x.UserId == userId))
            {
                return;
            }
            var newCaseUser = new CaseUser(userId, this);
            CaseUsers.Add(newCaseUser);
        }
    }
}
