using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.DemoApi.Service.Dtos.Logging;
using demo.DemoApi.Service.Infrastructure.Logging;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Логирование
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : demoControllerBase
    {
        private readonly IWebClientLoggingService _webClientLoggingService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webClientLoggingService"></param>
        public LogsController(IWebClientLoggingService webClientLoggingService)
        {
            _webClientLoggingService = webClientLoggingService;
        }

        /// <summary>
        /// Принять логи с Web клиента
        /// loglevel-plugin-remote терубет код возврата 200.
        /// При пустом Body, ASP.NET возвращает HTTP Code 204. Поэтому нам нужно вернуть любое Body.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [Route("WebClient")]
        public demoResult WebClient([FromBody] WebClientLogs model)
        {
            _webClientLoggingService.Log(model);
            return Ok("Success logging");
        }
    }
}