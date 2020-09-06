using System;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Модель данных активированной инструкции плана
    /// </summary>
    public class ActivatedPlanInstruction
    {

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
        public DateTime ActivationDate { get; set; }
    }
}
