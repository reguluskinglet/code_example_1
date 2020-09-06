using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace demo.DemoApi.Service.Hubs.Core
{
    /// <summary>
    /// Контент типа "application/json"
    /// </summary>
    public class JsonContent : StringContent
    {
        /// <summary>
        /// Создать
        /// </summary>
        /// <param name="obj"></param>
        public JsonContent(object obj) :
            base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
        {
        }
    }
}
