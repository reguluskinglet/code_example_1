using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using demo.FunctionalExtensions;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Service.ApplicationServices.Cache
{
    /// <summary>
    /// Провайдер для работы с распределенным кэшем и данными сессии
    /// </summary>
    public class CacheProvider
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        /// <inheritdoc />
        public CacheProvider(IDistributedCache cache, ILogger logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Добавить данные в кэш
        /// </summary>
        /// <param name="collection">Namespace для формирования уникального ключа</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public async Task<Result> SetAsync<T>(string collection, string key, T value)
        {
            _logger.Information($"{nameof(CacheProvider)} {nameof(SetAsync)} " +
                $"{nameof(key)}: {key} " +
                $"{nameof(value)} {value}" +
                $"{nameof(collection)} {collection}");
            string serializedData = "no value";
            try
            {
                if (string.IsNullOrEmpty(collection))
                {
                    var message = $"{nameof(collection)} not set";
                    _logger.Warning($"{message}. {nameof(value)}: {value}");
                    return Result.Failure(ErrorCodes.ValidationError);
                }

                if (string.IsNullOrEmpty(key))
                {
                    var message = $"{nameof(key)} not set";
                    _logger.Warning($"{message}. {nameof(value)}: {value}");
                    return Result.Failure(ErrorCodes.ValidationError);
                }

                var fullKeyName = GetFullCacheKeyName(collection, key);

                serializedData = Serialize(value);
                byte[] encodedData = EncodeData(serializedData);


                await _cache.SetAsync(fullKeyName, encodedData);
                _logger.Debug($"{nameof(CacheProvider)}. Данные добавлены в кэш. {nameof(fullKeyName)} {fullKeyName}; {nameof(value)} {value}");
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Ошибка добавления в кэш. Collection: {collection}; Key: {key}; SerializedValue: {serializedData}", ex);

                return Result.Failure(ErrorCodes.UnableToAddDataToCache);
            }

            return Result.Success();
        }

        /// <summary>
        /// Получить данные из кэша
        /// </summary>
        /// <param name="collection">Namespace для формирования уникального ключа</param>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string collection, string key)
        {
            _logger.Information($"{nameof(CacheProvider)} Получение данных из кэш. {nameof(collection)} {collection}. {nameof(key)} {key}");

            if (string.IsNullOrEmpty(collection))
            {
                var message = $"{nameof(collection)} не задан";
                _logger.Warning(message);
                return default;
            }

            if (string.IsNullOrEmpty(key))
            {
                var message = $"{nameof(key)} не задан";
                _logger.Warning(message);
                return default;
            }

            var fullKeyName = GetFullCacheKeyName(collection, key);

            byte[] encodedData;
            try
            {
                encodedData = await _cache.GetAsync(fullKeyName);
            }
            catch (Exception ex)
            {
                _logger.Fatal($"Ошибка получения данных из кэша. Collection: {collection}; Key: {key};", ex);

                return default;
            }

            if (encodedData == null)
            {
                _logger.Debug($"Не найдены данные в кэш. Collection: {collection}; Key: {key};");
                return default;
            }

            var decodedData = DecodeData(encodedData);

            var deserializedData = Deserialize<T>(decodedData);

            _logger.Debug($"{nameof(CacheProvider)}. Данные из кэш получены, декодированы, десериализованы. " +
                $"{nameof(fullKeyName)} {fullKeyName}. {nameof(deserializedData)} {decodedData}");

            return deserializedData;
        }

        /// <summary>
        /// Удаление записи из кэша
        /// </summary>
        /// <param name="collection">Namespace для формирования уникального ключа</param>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public async Task<Result> RemoveAsync(string collection, string key)
        {
            _logger.Debug($"{nameof(CacheProvider)}. Удаление данных из кэш. {nameof(collection)} {collection}. {nameof(key)} {key}");

            var fullKeyName = GetFullCacheKeyName(collection, key);

            try
            {
                await _cache.RemoveAsync(fullKeyName);
            }
            catch
            {
                _logger.Fatal($"Ошибка удаления из кэш. Collection: {collection}; Key: {key};");
                return Result.Failure(ErrorCodes.UnableToRemoveDataToCache);
            }

            _logger.Debug($"Данные из кэш успешно удалены. {nameof(fullKeyName)} {fullKeyName}");
            return Result.Success();
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
    }
}
