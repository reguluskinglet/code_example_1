using System.Threading.Tasks;

namespace demo.DemoApi.Service.Abstractions
{
    /// <summary>
    /// Клиент sip прокси
    /// </summary>
    public interface ISipProxyClient
    {
        /// <summary>
        /// Маршрутизация вызова по определенному адресу
        /// </summary>
        /// <param name="transactionId">Id транзакции(вызова)</param>
        /// <param name="sipUri">Адрес на который нужно переадресовать вызов</param>
        Task RouteCall(string transactionId, string sipUri);
    }
}
