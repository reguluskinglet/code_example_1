using System.Threading.Tasks;
using demo.DemoApi.AcceptanceTests.Clients;
using demo.DemoApi.AcceptanceTests.Clients.UserClients;
using TechTalk.SpecFlow;
using Xunit;

namespace demo.DemoApi.AcceptanceTests.Steps
{
    [Binding]
    public class ClientCanMakeACallSteps: BaseStepsDefinition
    {
        private readonly ApplicantClient _applicantUser;
        private int _callsCountBeforeCall;
        private readonly CtiClient _ctiClient;

        public ClientCanMakeACallSteps()
        {
            _ctiClient = new CtiClient(HttpClient, ApiUrl);
            _applicantUser = new ApplicantClient(HttpClient, ApiUrl);
        }

        [Given(@"заявитель авторизовался")]
        public async Task ApplicantLoggedIn()
        {
            await _applicantUser.LogIn("caller1");
        }

        [When(@"он начнет звонок")]
        public async Task ApplicantStartACall()
        {
            _callsCountBeforeCall = await _ctiClient.GetActiveCallsCount();
            await _applicantUser.StartCall();
        }

        [Then(@"количество активных звонков увеличится на (\d+)")]
        public async Task ActiveCallCountIncrease(int delta)
        {
            var callsCount = await _ctiClient.GetActiveCallsCount();
            Assert.Equal(delta, callsCount - _callsCountBeforeCall);
        }

    }
}
