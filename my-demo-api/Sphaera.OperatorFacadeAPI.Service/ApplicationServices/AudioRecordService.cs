using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using demo.FunctionalExtensions;
using demo.MediaRecording.Client;
using demo.Monitoring.Logger;
using demo.DemoApi.Domain.StatusCodes;

namespace demo.DemoApi.Service.ApplicationServices
{
    /// <summary>
    /// Сервис для записей разговоров
    /// </summary>
    public class AudioRecordService
    {
        private readonly MediaRecordingServiceClient _mediaRecordingServiceClient;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public AudioRecordService(MediaRecordingServiceClient mediaRecordingServiceClient, ILogger logger, IMapper mapper)
        {
            _mediaRecordingServiceClient = mediaRecordingServiceClient;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение stream записи разговора
        /// </summary>
        public async Task<Result<Stream>> GetAudioFileStream(Guid audioRecordId)
        {
            Result<Stream> result = await _mediaRecordingServiceClient.GetAudioFileStream(audioRecordId);
            
            if (result.IsFailure)
            {
                var message = $"AudioRecordService. GetAudioFileStream. Unable to get audio file stream by audioRecordId: {audioRecordId}";
                _logger.Warning($"{message}. {result.ErrorMessage}");
                return Result.Failure<Stream>(ErrorCodes.UnableToGetAudioFileStream);
            }

            return Result.Success(result.Value);
        }
    }
}
