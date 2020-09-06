using System.Net;
using Newtonsoft.Json;

namespace demo.DemoGateway.Serialization
{
    /// <summary>
    /// Класс для сериализации/десериализации объектов
    /// </summary>
    public class JsonSerializer
    {
        /// <summary>
        /// Сериализация объекта с кодированием символов
        /// </summary>
        public static string EncodeData(object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var encodedJson = WebUtility.HtmlEncode(jsonData);
            return encodedJson;
        }

        /// <summary>
        /// Десериализация с декодированием символов
        /// </summary>
        public static T DecodeData<T>(string data)
        {
            var jsonArgs = WebUtility.HtmlDecode(data);
            var args = JsonConvert.DeserializeObject<T>(jsonArgs);
            return args;
        }
    }
}
