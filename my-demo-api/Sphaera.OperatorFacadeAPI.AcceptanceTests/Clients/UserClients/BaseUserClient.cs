using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace demo.DemoApi.AcceptanceTests.Clients.UserClients
{
    public abstract class BaseUserClient
    {
        protected readonly string ApiUrl;
        protected readonly HubConnection Connection;
        protected readonly HttpClient HttpClient;

        protected BaseUserClient(HttpClient httpClient, string apiUrl)
        {
            HttpClient = httpClient;
            ApiUrl = apiUrl;

            Connection = new HubConnectionBuilder()
                .WithUrl($"{apiUrl}/phoneHub")
                .Build();
        }

        public abstract Task LogIn(string phoneNumber);
    }
}
