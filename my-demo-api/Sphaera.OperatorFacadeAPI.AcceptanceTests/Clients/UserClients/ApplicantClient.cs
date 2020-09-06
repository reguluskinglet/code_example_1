using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.AcceptanceTests.Clients.UserClients
{
    public class ApplicantClient : BaseUserClient
    {
        private Applicant _applicant;

        public ApplicantClient(HttpClient httpClient, string apiUrl) : base(httpClient, apiUrl)
        {
        }

        public override async Task LogIn(string phoneNumber)
        {
            _applicant = new Applicant
            {
                Extension = phoneNumber
            };
            await Connection.StartAsync();
        }

        public async Task StartCall()
        {
            await Connection.SendAsync("call", _applicant.Id);
        }

        public async Task RejectCall()
        {
            await Connection.SendAsync("RejectFromApplicant", _applicant.Id);
        }
    }
}
