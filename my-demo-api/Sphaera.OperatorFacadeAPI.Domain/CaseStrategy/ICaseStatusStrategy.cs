using demo.FunctionalExtensions;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.Domain.CaseStrategy
{
    /// <summary>
    /// Интерфейс для реализации стратегии обработки статусов.
    /// </summary>
    public interface ICaseStatusStrategy
    {
        /// <summary>
        /// Получить статус карточки по умолчанию.
        /// </summary>
        /// <returns></returns>
        string GetStatusDefault();

        /// <summary>
        /// Получить статус карточки "В работе".
        /// </summary>
        /// <returns></returns>
        string GetStatusInProgress();

        /// <summary>
        /// Указать статус карточки "В работе".
        /// </summary>
        /// <param name="currentCase"></param>
        Result<string> SetStatusInProgress(Case currentCase);

        /// <summary>
        /// Указать статус карточки "Закрыта".
        /// </summary>
        /// <param name="currentCase"></param>
        Result<string> SetStatusClosed(Case currentCase);

        /// <summary>
        /// Нахолится ли карточка в статусе "В Работе".
        /// </summary>
        /// <param name="currentCase"></param>
        /// <returns></returns>
        bool IsStatusInProgress(Case currentCase);

        /// <summary>
        /// Находится ли карточка в статусе "Закрыта".
        /// </summary>
        /// <param name="currentCase"></param>
        /// <returns></returns>
        bool IsStatusClosed(Case currentCase);
    }
}