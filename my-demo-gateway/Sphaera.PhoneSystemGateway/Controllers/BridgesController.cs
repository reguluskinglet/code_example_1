using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.DemoGateway.Dtos;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;

namespace demo.DemoGateway.Controllers
{
    /// <summary>
    /// Вспомогательный контроллер для управления бриджами Asterisk
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BridgesController : demoControllerBase
    {
        private readonly AsteriskAriApiService _asteriskAriApiService;

        /// <summary>
        /// Конструктор
        /// </summary>
        public BridgesController(AsteriskAriApiService asteriskAriApiService)
        {
            _asteriskAriApiService = asteriskAriApiService;
        }

        /// <summary>
        /// Получить список всех бриджей. Используется для разработки и тестирования.
        /// </summary>
        [HttpGet("getBridges")]
        public async Task<demoResult<IList<BridgeDto>>> GetAllBridges()
        {
            var bridges = await _asteriskAriApiService.GetAllBridges();
            return Ok(bridges);
        }

        /// <summary>
        /// Удалить все бриджи. Используется для разработки и тестирования.
        /// </summary>
        [HttpDelete("deleteBridges")]
        public async Task<demoResult> DeleteAllBridges()
        {
            await _asteriskAriApiService.DeleteAllBridges();

            return Ok();
        }
    }
}
