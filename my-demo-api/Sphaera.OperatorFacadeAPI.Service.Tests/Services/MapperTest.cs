using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using demo.DemoApi.Service.Tests.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Services
{
    [Collection("ServicesFixture")]
    public class MapperTest
    {
        private readonly IMapper _mapper;

        public MapperTest(ServicesFixture fixture)
        {
            var serviceProvider = fixture.Services.BuildServiceProvider();
            _mapper = serviceProvider.GetService<IMapper>();
        }

        [Fact]
        public void MapperTest_CallMapperValidationMethod()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
