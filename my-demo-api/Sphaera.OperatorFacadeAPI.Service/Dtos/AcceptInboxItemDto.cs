using System;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Отправляется клиентом при принятии звонка или другого элемента очереди
    /// </summary>
    public class AcceptInboxItemDto
    {
        /// <summary>
        /// Идентификатор очереди, из которой приняли звонок
        /// </summary>
        public Guid InboxId { get; set; }

        /// <summary>
        /// Идентификатор элемента очереди, который принимает пользователь. Заполнен в случае, если пользователь выбрал конкретный элемент для принятия
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Проверка на валидность модели
        /// </summary>
        public Result IsValid()
        {
            if (InboxId == default)
            {
                return Result.Failure(ErrorCodes.ValidationError, $"Empty {nameof(InboxId)}");
            }

            if (ItemId.HasValue && ItemId == Guid.Empty)
            {
                return Result.Failure(ErrorCodes.ValidationError, $"Empty {nameof(ItemId)}");
            }

            return Result.Success();
        }
    }
}
