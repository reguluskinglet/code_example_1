using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto для запроса на звонок на внешний номер.
    /// </summary>
    public class CallToNumberDto
    {
        /// <summary>
        /// Номер телефона на который совершаем звонок
        /// </summary>
        public string Extension { get; set; }
    }
}