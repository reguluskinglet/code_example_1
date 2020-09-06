using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace demo.DemoApi.AcceptanceTests.Clients
{
    public class CtiClient
    {
        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;

        public CtiClient(HttpClient httpClient, string apiUrl)
        {
            _httpClient = httpClient;
            _apiUrl = apiUrl;
        }

        public async Task<int> GetActiveCallsCount()
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/api/calls/count");
            return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        }
    }
}
