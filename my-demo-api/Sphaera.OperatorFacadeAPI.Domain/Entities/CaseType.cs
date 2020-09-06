using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using demo.DDD;
using demo.DemoApi.Domain.CaseStrategy;

namespace demo.DemoApi.Domain.Entities
{
    /// <summary>
    /// Шаблон карточки
    /// </summary>
    public class CaseType : AggregateRoot
    {
        private ICaseStatusStrategy _statusStrategy;

        /// <summary>
        /// Текущая реализация стратегии обработки статусов.
        /// </summary>
        public ICaseStatusStrategy StatusStrategy
        {
            get
            {
                if (_statusStrategy == null)
                {
                    var context = new CaseTypeStatusContext(this);
                    _statusStrategy = context.GetCaseStatusStrategy();
                }
                return _statusStrategy;
            }
        }

        /// <remarks>
        /// Конструктор без параметров необходим для маппинга при наличии других конструкторов. Удалять нельзя
        /// </remarks>
        public CaseType()
        {
        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="data"></param>
        public CaseType(string data) : base(Guid.NewGuid())
        {
            Data = data;
        }

        /// <summary>
        /// Шаблон
        /// </summary>
        public string Data { get; set; }
        
        /// <summary>
        /// Стили для шаблона
        /// </summary>
        public string Css { get; set; }

        /// <summary>
        /// Карточки, связанные с шаблоном
        /// </summary>
        public virtual IList<Case> Cases { get; set; }

        /// <summary>
        /// Взять заголовок из кейса
        /// </summary>
        public string GetTitle()
        {
            if (string.IsNullOrEmpty(Data))
            {
                return null;
            }

            var jObject = JObject.Parse(Data);
            return jObject["card"]["title"].ToString();
        }
    }
}
