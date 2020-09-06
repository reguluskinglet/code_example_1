using System.Collections.Generic;

namespace demo.DemoApi.Service.Dtos.Logging
{
    /// <summary>
    /// Logs collection
    /// </summary>
    public class WebClientLogs
    {
        /// <summary>
        /// Collection of logs
        /// </summary>
        public List<WebClientLogInfo> Logs { get; set; }
    }
}
