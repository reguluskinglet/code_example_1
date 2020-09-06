using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using demo.FunctionalExtensions;
using demo.Http.Client;
using demo.Monitoring.Logger;
using demo.DemoGateway.Client.Dto;
using demo.DemoGateway.Client.Options;
using demo.DemoGateway.Client.StatusCodes;

namespace demo.DemoGateway.Client
{
    /// <summary>
    /// Клиент для передачи данных в DemoGateway
    /// </summary>
    public class DemoGatewayClient : BaseClient
    {
        private readonly ILogger _logger;
        private readonly string _baseUrl;

        /// <summary>
        /// Создает новый экземпляр <see cref="DemoGatewayClient"/>.
        /// </summary>
        public DemoGatewayClient(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IOptions<DemoGatewayClientOptions> servicesOptions) : base(httpClientFactory, logger)
        {
            _logger = logger;

            var gatewayUri = $"{servicesOptions.Value.Url}/api/";
            _baseUrl = $"{gatewayUri}rpc/";
        }

        /// <summary>
        /// Сообщить Gateway о принятии вызова от заявителя
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Result> NotifyGatewayAboutAcceptedApplicantCall(RouteCallClientDto data)
        {
            var url = $"{_baseUrl}acceptIncomingCall";
            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Добавить ассистента в разговор
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Result> AddAssistant(RouteCallClientDto data)
        {
            var url = $"{_baseUrl}addAssistant";

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Добавить частичного ассистента в разговор
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Result> AddPartialAssistant(RouteCallClientDto data)
        {
            var url = $"{_baseUrl}addPartialAssistant";

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Добавить в конференцию
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Result> AddToConference(RouteCallClientDto data)
        {
            var url = $"{_baseUrl}addToConference";

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Отправить данные о изменении роли в DemoGateway
        /// </summary>
        public async Task<Result> NotifyGatewayAboutChangeRole(Guid lineId, Guid fromCallId, Guid toCallId)
        {
            var url = $"{_baseUrl}exchangeRoles";
            var data =
                new RouteCallClientDto
                {
                    ToCallId = toCallId,
                FromCallId = fromCallId,
                LineId = lineId
            };

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Отправить данные о непредвиденном прерывании вызова
        /// </summary>
        public async Task<Result> NotifyGatewayAboutInterruptedCall(Guid callId, Guid? applicantCallId)
        {
            var url = $"{_baseUrl}forceHangUp";
            var data =
                new RouteCallClientDto
            {
                FromCallId = applicantCallId,
                ToCallId = callId
            };

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        /// <summary>
        /// Отправить информацию о изолированном звонке.
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="isolation"></param>
        /// <returns></returns>
        public async Task<Result> NotifyGatewayAboutChangedIsolationStatus(Guid callId, bool isolation)
        {
            var DemoGatewayIsolationUri = $"{_baseUrl}isolate";

            var data =
                new SetIsolationStatusClientDto
                {
                    CallId = callId,
                    Isolated = isolation
                };

            BaseClientResult<ErrorCodes> baseResult = await SendData<SetIsolationStatusClientDto, ErrorCodes>(data, DemoGatewayIsolationUri);

            Result result = ProcessResult(baseResult);

            if (result.IsFailure)
            {
                _logger.Warning(result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Отправить информацию о новом состоянии микрофона.
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="newMicState"></param>
        /// <returns></returns>
        public async Task<Result> ChangedMicrophoneMuteStatus(Guid callId, bool newMicState)
        {
            var microphoneMuteUri = $"{_baseUrl}mute";

            var data =
                new SetMuteStatusClientDto
                {
                    CallId = callId,
                    Muted = newMicState
                };

            BaseClientResult<ErrorCodes> baseResult =
                await SendData<SetMuteStatusClientDto, ErrorCodes>(data, microphoneMuteUri);

            Result result = ProcessResult(baseResult);

            if (result.IsFailure)
            {
                _logger.Warning(result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Сообщить Gateway о необходимости совершить звонок от пользователя
        /// </summary>
        public async Task<Result> CallFromUser(RouteCallClientDto data)
        {
            var url = $"{_baseUrl}callFromUser";

            BaseClientResult<ErrorCodes> baseResult = await SendData<RouteCallClientDto, ErrorCodes>(data, url);

            Result result = ProcessResult(baseResult);

            return result;
        }

        private Result ProcessResult(BaseClientResult<ErrorCodes> baseResult)
        {
            if (baseResult.IsSuccess)
            {
                return Result.Success();
            }

            if (baseResult.CodeType == ErrorCodes.Unknown)
            {
                return Result.Failure(ErrorCodes.UnexpectedServiceResponse);
            }

            return Result.Failure(baseResult.CodeType);
        }
    }
}
