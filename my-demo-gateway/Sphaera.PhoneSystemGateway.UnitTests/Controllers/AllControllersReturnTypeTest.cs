using System.Reflection;
using demo.Http.ServiceResponse.BaseController.Tester;
using Xunit;

namespace demo.DemoGateway.UnitTests.Controllers
{
    public class AllControllersReturnTypeTest
    {
        [Fact]
        public void CheckActionMethodsReturnValue_ShouldBedemoResult()
        {
            var tester = new ControllersTester();

            var assembly = Assembly.GetAssembly(typeof(demo.DemoGateway.Startup));

            tester.TestActionMethodsReturnType(assembly);
        }
    }
}