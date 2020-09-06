using System;
using System.Threading.Tasks;
using AsterNET.ARI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.StatusCodes;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;

namespace demo.DemoGateway.Controllers
{
    /// <summary>
    /// Удаленный вызов процедур.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AsteriskController : demoControllerBase
    {
        private readonly ILogger _logger;
        private readonly AsteriskAriClient _ariClient;

        /// <summary>
        /// Конструтор для инъекции зависимостей.
        /// </summary>
        /// <param name="ariClient"></param>
        /// <param name="logger"></param>
        public AsteriskController(AsteriskAriClient ariClient, ILogger logger)
        {
            _ariClient = ariClient;
            _logger = logger;
        }

        /// <summary>
        /// Взять данные по астериску.
        /// </summary>
        [HttpGet]
        public async Task<demoResult<string>> GetInfo()
        {
            var result = await _ariClient.GetInfoAsync();
            return Answer(result);
        }
    }
}