using System.Threading.Tasks;
using demo.FunctionalExtensions;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Service.ApplicationServices.Cache
{
    /// <summary>
    /// Методы расширения для CacheProvider
    /// </summary>
    public static class CacheProviderExtensions
    {
        /// <summary>
        /// Добавить в кэш одновременно key-value и value-key
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="collection">Namespace для формирования уникального ключа</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<Result> SetKeyValueAsync(this CacheProvider cacheProvider, string collection, string key, string value)
        {
            var setKeyResult = await cacheProvider.SetAsync(collection, key, value);
            if (setKeyResult.IsFailure)
            {
                return setKeyResult;
            }

            var setValueResult = await cacheProvider.SetAsync(collection, value, key);

            return setValueResult;
        }

        /// <summary>
        /// Найти ключ по значению и удалить ключ-значение, затем удалить значение-ключ из кэша
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="collection">Namespace для формирования уникального ключа</param>
        /// <param name="value">значение, по которому идет поиск ключа</param>
        /// <returns></returns>
        public static async Task<Result> RemoveKeyByValue(this CacheProvider cacheProvider, string collection, string value)
        {
            var key = await cacheProvider.GetAsync<string>(collection, value);
            if (string.IsNullOrWhiteSpace(key))
            {
                return Result.Failure(ErrorCodes.KeyForCacheNotFound);
            }

            await cacheProvider.RemoveAsync(collection, key);
            await cacheProvider.RemoveAsync(collection, value);

            return Result.Success();
        }
    }
}
