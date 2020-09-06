using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace demo.DemoApi.DAL
{
    /// <summary>
    /// Менеджер ресурсов
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Взять ресурс
        /// </summary>
        /// <param name="subName"></param>
        /// <returns></returns>
        public static string GetResource(string subName)
        {
            var assembly = Assembly.GetCallingAssembly();
            var listOfFileNames = assembly.GetManifestResourceNames();
            var listOfCandidate = listOfFileNames.Where(name => name.Contains(subName)).ToList();
            if (listOfCandidate.Count > 1 || listOfCandidate.Count == 0)
            {
                throw new Exception("Не получается однозначно определить имя ресурса");
            }
            
            var resourceStream = assembly.GetManifestResourceStream(listOfCandidate.FirstOrDefault());
            using var reader = new StreamReader(resourceStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
