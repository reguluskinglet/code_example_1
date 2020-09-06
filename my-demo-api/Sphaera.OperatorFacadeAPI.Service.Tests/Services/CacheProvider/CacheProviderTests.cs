using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using Shouldly;
using demo.Monitoring.Logger;
using Xunit;

namespace demo.DemoApi.Service.Tests.Services.CacheProvider
{
    public class CacheProviderTests
    {
        private ApplicationServices.Cache.CacheProvider GetCacheProvider()
        {
            var logger = GetMockedLogger();
            var mockDistributedCache = new Mock<IDistributedCache>();

            var cacheProvider = new ApplicationServices.Cache.CacheProvider(mockDistributedCache.Object, logger);

            return cacheProvider;
        }

        private ILogger GetMockedLogger()
        {
            return new Mock<ILogger>().Object;
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Set_EmptyCollectionName_Failed()
        {
            var cacheProvider = GetCacheProvider();

            var result = await cacheProvider.SetAsync(null, "key", "value");

            result.IsFailure.ShouldBeTrue();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Set_EmptyKey_Failed()
        {
            var cacheProvider = GetCacheProvider();

            var result = await cacheProvider.SetAsync("collectionName", null, "value");

            result.IsFailure.ShouldBeTrue();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Set_CustomObject_ShouldBeSerializedAndEncoded()
        {
            // Arrange
            var customObject = new { a = "str1", b = "str2" };
            var serializedCustomObject = JsonConvert.SerializeObject(customObject);

            var logger = GetMockedLogger();

            var collectionName = "collectionName";
            var keyName = "keyName";
            
            var expectedKeyName = $"{collectionName}_{keyName}";
            byte[] expectedValue = Encoding.UTF8.GetBytes(serializedCustomObject);

            string resultKey = null;
            var resultValue = new byte[0];
            
            var mockDistributedCache = new Mock<IDistributedCache>();
            mockDistributedCache.Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
                .Callback<string,byte[],DistributedCacheEntryOptions,CancellationToken>((key, value, options, token) =>
                {
                    resultKey = key;
                    resultValue = value;
                })
                .Returns(Task.FromResult(default(object)));

            var cacheProvider = new ApplicationServices.Cache.CacheProvider(mockDistributedCache.Object, logger);
            
            //Act
            await cacheProvider.SetAsync(collectionName, keyName, customObject);

            //Assert
            resultKey.ShouldBe(expectedKeyName);
            resultValue.ShouldBe(expectedValue);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Get_EmptyCollection_Failed()
        {
            var cacheProvider = GetCacheProvider();

            var result = await cacheProvider.GetAsync<string>(null, "key");

            result.ShouldBeNull();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Get_EmptyKey_Failed()
        {
            var cacheProvider = GetCacheProvider();

            var result = await cacheProvider.GetAsync<string>("collection", null);

            result.ShouldBeNull();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async Task Get_CustomObject_ShouldBeValid()
        {
            //Arrange
            var customObject = new CacheProviderTestCustomObject()
            {
                CustomPropertyString = "str1",
                CustomPropertyInt = 5
            };
            var serializedCustomObject = JsonConvert.SerializeObject(customObject);
            byte[] encodedCustomObject = Encoding.UTF8.GetBytes(serializedCustomObject);

            var collectionName = "collectionName";
            var keyName = "keyName";

            var fullKeyName = $"{collectionName}_{keyName}";

            var mockLogger = GetMockedLogger();

            var mockDistributedCache = new Mock<IDistributedCache>();
            mockDistributedCache
                .Setup(
                    x => x.GetAsync(
                        It.Is<string>(
                            s => s.Equals(fullKeyName)),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(encodedCustomObject);


            var cacheProvider = new ApplicationServices.Cache.CacheProvider(mockDistributedCache.Object, mockLogger);

            //Act
            var result = await cacheProvider.GetAsync<CacheProviderTestCustomObject>(collectionName, keyName);
            
            //Assert
            result.ShouldNotBeNull();
            result.CustomPropertyInt.ShouldBe(customObject.CustomPropertyInt);
            result.CustomPropertyString.ShouldBe(customObject.CustomPropertyString);
        }
    }
}
