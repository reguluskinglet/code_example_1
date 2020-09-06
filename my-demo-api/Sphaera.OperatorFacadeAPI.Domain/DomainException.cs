using System;
using System.Runtime.Serialization;

namespace demo.DemoApi.Domain
{
    /// <summary>
    /// Ошибка доменной модели
    /// </summary>
    [Serializable]
    public class DomainException : Exception
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public DomainException()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        protected DomainException(SerializationInfo info, StreamingContext context) : 
            base(info, context)
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DomainException(string message) : 
            base(message)
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public DomainException(string message, Exception innerException) : 
            base(message, innerException)
        {
        }
    }
}
