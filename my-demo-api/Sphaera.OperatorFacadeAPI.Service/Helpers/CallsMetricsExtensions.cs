using System;
using System.Collections.Generic;
using demo.Monitoring.Metrics;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.Dtos.Calls;

namespace demo.DemoApi.Service.Helpers
{
    /// <summary>
    /// Расширения для добавления метрик звонков
    /// </summary>
    public static class CallsMetricsExtensions
    {

        /// <summary>
        /// Метрики при добавлении нового звонка
        /// </summary>
        public static void CallAdded(this IMetric metric)
        {
            metric.IncrementCounter((options) => {
                options.Name = "incomming_call";
                options.LabelValues = new List<string>().ToArray();
                options.LabelNames = new List<string>().ToArray();
            });
        }
        
        /// <summary>
        /// Метрики при добавлении нового cмс-сообщения
        /// </summary>
        public static void SmsAdded(this IMetric metric)
        {
            metric.IncrementCounter((options) => {
                options.Name = "incomming_sms";
                options.LabelValues = new List<string>().ToArray();
                options.LabelNames = new List<string>().ToArray();
            });
        }
    }
}
