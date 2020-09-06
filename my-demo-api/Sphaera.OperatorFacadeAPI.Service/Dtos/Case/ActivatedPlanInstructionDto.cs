using System;

namespace demo.DemoApi.Service.Dtos.Case
{
    /// <summary>
    /// Данные активированной инструкции плана
    /// </summary>
    public class ActivatedPlanInstructionDto
    {
        /// <summary>
        /// Id карточки
        /// </summary>
        public Guid CaseId { get; set; }

        /// <summary>
        /// Id плана
        /// </summary>
        public Guid PlanId { get; set; }

        /// <summary>
        /// Id инструкции
        /// </summary>
        public Guid InstructionId { get; set; }

        /// <summary>
        /// Время активации
        /// </summary>
        public DateTime? ActivationDate { get; set; }

        /// <summary>
        /// Проверка на валидности модели
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return CaseId != default
                && PlanId != default
                && InstructionId != default
                && ActivationDate != null;
        }
    }
}
