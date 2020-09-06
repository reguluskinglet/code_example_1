using demo.DemoGateway.Dtos;

namespace demo.DemoGateway.Infrastructure.HostedServices.Dtos
{
    /// <summary>
    /// Аргументы события IsolationStatusEventArgs, передаваемые при вызове.
    /// </summary>
    public class IsolationStasisStartEventArgs : StasisStartEventArgs
    {
        /// <summary>
        /// Данные об изоляции передаваемые с CTI сервиса.
        /// </summary>
        public IsolationStatusDto IsolationStatusData { get; set; }
    }
}