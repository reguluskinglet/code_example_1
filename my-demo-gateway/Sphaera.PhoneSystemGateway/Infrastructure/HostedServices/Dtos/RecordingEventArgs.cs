using System;

namespace demo.DemoGateway.Infrastructure.HostedServices.Dtos
{
    /// <summary>
    /// Аргументы событий записи разговоров.
    /// </summary>
    public class RecordingEventArgs : StasisStartEventArgs
    {
        /// <inheritdoc />
        public RecordingEventArgs(DateTime eventTime, string recordName)
        {
            EventTime = eventTime;
            RecordName = recordName;
        }

        /// <summary>
        /// Время возникновения события.
        /// </summary>
        public DateTime EventTime { get; set; }

        /// <summary>
        /// Имя записи.
        /// </summary>
        public string RecordName { get; set; }
    }
}