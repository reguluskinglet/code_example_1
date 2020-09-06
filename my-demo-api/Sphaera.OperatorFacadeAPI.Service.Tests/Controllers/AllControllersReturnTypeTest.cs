using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using demo.Http.ServiceResponse.BaseController.Tester;
using Xunit;

namespace demo.DemoApi.Service.Tests.Controllers
{
    public class AllControllersReturnTypeTest
    {
        [Fact]
        public void CheckActionMethodsReturnValue_ShouldBedemoResult()
        {
            var tester = new ControllersTester();

            var assembly = Assembly.GetAssembly(typeof(demo.DemoApi.Service.Startup));

            tester.TestActionMethodsReturnType(assembly);
        }
    }
}
