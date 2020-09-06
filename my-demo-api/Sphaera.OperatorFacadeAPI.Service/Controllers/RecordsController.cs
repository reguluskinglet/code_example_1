using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using demo.Http.ServiceResponse;
using demo.Http.ServiceResponse.BaseController;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;
using demo.DemoApi.Service.ApplicationServices;

namespace demo.DemoApi.Service.Controllers
{
    /// <summary>
    /// Контроллер для работы с файлами записи разговора
    /// </summary>
    [Route("api/[controller]"), ApiController]
    public class RecordsController : demoControllerBase
    {
        private readonly ILogger _logger;
        private readonly AudioRecordService _audioRecordService;

        /// <inheritdoc />
        public RecordsController(ILogger logger, AudioRecordService audioRecordService)
        {
            _logger = logger;
            _audioRecordService = audioRecordService;
        }

        /// <summary>
        /// Получить stream записи разговора
        /// </summary>
        [HttpGet("audioStream/{audioRecordId}")]
        public async Task<demoResult> GetAudioFileStream(Guid audioRecordId)
        {
            if (audioRecordId == default)
            {
                _logger.Warning("RecordsController. GetAudioFileStream method. AudioRecordId has default value.");
                return BadRequest(ErrorCodes.ValidationError);
            }

            var result = await _audioRecordService.GetAudioFileStream(audioRecordId);
            if (result.IsFailure)
            {
                _logger.Warning("RecordsController. GetAudioFileStream method. Audio file not found.");
                return BadRequest(result.ErrorCode);
            }

            var stream = result.Value;

            Response.Headers.Add("Content-Length", stream.Length.ToString());
            Response.Headers.Add("Accept-Ranges", $"bytes");
            Response.Headers.Add("Content-Range", $"bytes 0-{stream.Length}/{stream.Length}");

            return Ok(stream);
        }
    }
}
