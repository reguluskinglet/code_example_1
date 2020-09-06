using System;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shouldly;
using demo.DemoApi.Service.ApplicationServices.Cache;
using demo.DemoApi.Service.Tests.Core;
using Xunit;

namespace demo.DemoApi.Service.Tests.Scenario
{
    [Collection("ServicesFixture")]
    public class CacheProviderIntegrationTests: IDisposable
    {
        private const string CollectionNameSetAsync = "TestCollectionSetAsync";
        private const string KeyNameSetAsync = "TestKeySetAsync";

        private const string CollectionNameGetAsync = "TestCollectionGetAsync";
        private const string KeyNameGetAsync = "TestKeyGetAsync";

        private const string CollectionNameRemoveAsync = "TestCollectionRemoveAsync";
        private const string KeyNameRemoveAsync = "TestKeyRemoveAsync";
        
        private readonly ServiceProvider _serviceProvider;
        
        public CacheProviderIntegrationTests(ServicesFixture fixture)
        {
            var services = fixture.Services;

            services.AddSingleton<CacheProvider>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void SetAsync_ValidData_SuccessfullySaved()
        {
            //Arrange
            var value = "TestValue";
            var collectionName = CollectionNameSetAsync;
            var keyName = KeyNameSetAsync;
            var fullKeyName = GetFullCacheKeyName(collectionName, keyName);
            
            var cacheProvider = _serviceProvider.GetService<CacheProvider>();
            var distributedCache = _serviceProvider.GetService<IDistributedCache>();

            //Act
            var result = await cacheProvider.SetAsync(collectionName, keyName, value);

            byte[] encodedData = await distributedCache.GetAsync(fullKeyName);
            var decodedData = DecodeData(encodedData);
            var deserializedData = Deserialize<string>(decodedData); 

            //Assert
            result.IsSuccess.ShouldBeTrue();
            deserializedData.ShouldBe(value);
        }

        
        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void GetAsync_SavedData_SuccessfullyRead()
        {
            //Arrange
            var value = "TestValue";
            var collectionName = CollectionNameGetAsync;
            var keyName = KeyNameGetAsync;
            var fullKeyName = GetFullCacheKeyName(collectionName, keyName);
            
            var cacheProvider = _serviceProvider.GetService<CacheProvider>();
            var distributedCache = _serviceProvider.GetService<IDistributedCache>();

            byte[] encodedValue = EncodeData(Serialize(value));

            await distributedCache.SetAsync(fullKeyName, encodedValue);

            //Act
            var data = await cacheProvider.GetAsync<string>(collectionName, keyName);

            //Assert
            data.ShouldBe(value);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void GetAsync_WithNotExistingKey_ShouldNotExists()
        {
            //Arrange
            var collectionName = "NonExistingCollection";
            var keyName = "NonExistingKey";
            var fullKeyName = GetFullCacheKeyName(collectionName, keyName);
            
            var cacheProvider = _serviceProvider.GetService<CacheProvider>();

            //Act
            var data = await cacheProvider.GetAsync<string>(collectionName, keyName);

            //Assert
            data.ShouldBe(default);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public async void RemoveAsync_ExistingKey_SuccessfullyRemoved()
        {
            //Arrange
            var value = "TestValue";
            var collectionName = CollectionNameRemoveAsync;
            var keyName = KeyNameRemoveAsync;
            var fullKeyName = GetFullCacheKeyName(collectionName, keyName);
            
            var cacheProvider = _serviceProvider.GetService<CacheProvider>();
            var distributedCache = _serviceProvider.GetService<IDistributedCache>();

            byte[] encodedValue = EncodeData(Serialize(value));

            await distributedCache.SetAsync(fullKeyName, encodedValue);
            
            //Act
            var result = await cacheProvider.RemoveAsync(collectionName, keyName);

            byte[] data = await distributedCache.GetAsync(fullKeyName);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            data.ShouldBeNull();
        }
        
        private string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        private T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        private byte[] EncodeData(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        private string DecodeData(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }
        
        private string GetFullCacheKeyName(string collectionName, string keyName)
        {
            return $"{collectionName}_{keyName}";
        }

        public void Dispose()
        {
            var distributedCache = _serviceProvider.GetService<IDistributedCache>();
            
            distributedCache.Remove(GetFullCacheKeyName(CollectionNameSetAsync,KeyNameSetAsync));
            distributedCache.Remove(GetFullCacheKeyName(CollectionNameGetAsync,KeyNameGetAsync));
            distributedCache.Remove(GetFullCacheKeyName(CollectionNameRemoveAsync, KeyNameRemoveAsync));
        }
    }
}
