using demo.DemoApi.Service.Dtos.Logging;

namespace demo.DemoApi.Service.Infrastructure.Logging
{
    /// <summary>
    /// Сервис для работы с логами Web клиента
    /// </summary>
    public interface IWebClientLoggingService
    {
        /// <summary>
        /// Сохранить логи в зависимости от категории
        /// </summary>
        void Log(WebClientLogs logModel);
    }
}