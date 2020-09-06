using System.Collections.Generic;
using Newtonsoft.Json;
using demo.Monitoring.Logger;
using demo.DemoApi.Service.Dtos.Logging;

namespace demo.DemoApi.Service.Infrastructure.Logging
{
    /// <summary>
    /// Сервис для работы с логами Web клиента
    /// </summary>
    public class WebClientLoggingService : IWebClientLoggingService
    {
        private const string WebClientPrefix = "webclient";
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public WebClientLoggingService(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Сохранить логи в зависимости от категории
        /// </summary>
        /// <param name="logModel"></param>
        public void Log(WebClientLogs logModel)
        {
            foreach (var log in logModel.Logs)
            {
                Log(log);
            }
        }

        private void Log(WebClientLogInfo logInfo)
        {
            IDictionary<string, string> commonParams = CreateCommonParams(logInfo);

            var message = NormalizeLogString(logInfo.Message);
            _logger.Information(message, properties: commonParams);
        }

        private string CreateKey(string value)
        {
            return $"{WebClientPrefix}_{value}";
        }
     
        /// <summary>
        /// Создать дополнительные параметры для отправки вместе с сообщением
        /// Все дополнительные параметры от клиента имеют префикс "webclient_"
        /// </summary>
        /// <param name="logInfo"></param>
        /// <returns></returns>
        private IDictionary<string, string> CreateCommonParams(WebClientLogInfo logInfo)
        {
            var parameters = new Dictionary<string, string>
            {
                [WebClientPrefix] = true.ToString(),
                [CreateKey("level")] = logInfo.Level,
                [CreateKey("clientVersion")] = logInfo.ClientVersion,
                [CreateKey("level")] = logInfo.Level,
                [CreateKey("stacktrace")] = NormalizeLogString(logInfo.Stacktrace),
                [CreateKey("gisLogs")] = logInfo.IsGisLogs.ToString(),
            };

            foreach ((string key, string value) in logInfo.AdditionalParams)
            {
                parameters[key] = value;
            }

            return parameters;
        }

        private string NormalizeLogString(string logString)
        {
            if (!string.IsNullOrEmpty(logString))
            {
                var normalizedString = JsonConvert.SerializeObject(logString);
                return normalizedString.Substring(1, normalizedString.Length - 2);
            }

            return logString;
        }
    }
}
