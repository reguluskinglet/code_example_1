using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace demo.DemoApi.Service.Attributes
{
    /// <summary>
    /// Атрибут для проверки контрольной суммы ответа
    /// </summary>
    public class CheckETagAttribute : Attribute, IAsyncActionFilter
    {
        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext actionExecutingContext, ActionExecutionDelegate next)
        {
            var context = actionExecutingContext.HttpContext;
            context.Items.Add("CheckETagEnabled", true);

            await next();
        }
    }
}
