using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace demo.DemoApi.AcceptanceTests.Steps
{
    public abstract class BaseStepsDefinition
    {
        protected readonly string ApiUrl;
        protected readonly HttpClient HttpClient;

        protected BaseStepsDefinition()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json", false, true)
                .Build();
            ApiUrl = config.GetValue<string>("apiUrl");

            HttpClient = new HttpClient();
        }
    }
}
