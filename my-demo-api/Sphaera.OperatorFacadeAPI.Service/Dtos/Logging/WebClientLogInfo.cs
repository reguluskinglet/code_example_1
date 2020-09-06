using System;
using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Logging
{
    /// <summary>
    /// Information about WebClient log
    /// </summary>
    public class WebClientLogInfo
    {
        /// <summary>
        /// Log Level (trace, debug, info, warn, error)
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Logger Name
        /// </summary>
        public string Logger { get; set; }

        /// <summary>
        /// Log Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Stacktrace
        /// </summary>
        public string Stacktrace { get; set; }

        /// <summary>
        /// Log time
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Версия сборки приложения клиента
        /// </summary>
        public string ClientVersion { get; set; }

        /// <summary>
        /// Оперделяет, пришли ли логи с ГИС
        /// </summary>
        public bool IsGisLogs { get; set; }

        /// <summary>
        /// Additional parameters sent from client (serviceUrl, etc.)
        /// </summary>
        public Dictionary<string, string> AdditionalParams { get; set; }
    }
}
