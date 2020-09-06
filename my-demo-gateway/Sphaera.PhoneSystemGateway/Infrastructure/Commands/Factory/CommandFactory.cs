using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using demo.DemoGateway.Enums;
using demo.DemoGateway.Infrastructure.Commands.Base;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.Options;

namespace demo.DemoGateway.Infrastructure.Commands.Factory
{
    /// <summary>
    /// Фабрика инициализации списка команд для Asterisk ARI.
    /// </summary>
    public class CommandFactory
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// Экземпляр фабрики для работы с командами Asterisk ARI.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Получить объект готовой инициализации AsteriskAriClient
        /// для сервисов Asterisk ARI.
        /// </summary>
        /// <returns></returns>
        public AsteriskAriClient GetAsteriskAriClient()
        {
            return _serviceProvider.GetService<AsteriskAriClient>();
        }

        /// <summary>
        /// Получить настройки для клиента Asterisk ARI.
        /// </summary>
        /// <returns></returns>
        public AsteriskOptions GetAsteriskAriOptions()
        {
            var options = _serviceProvider.GetService<IOptions<AsteriskOptions>>();
            return options.Value;
        }

        /// <summary>
        /// Получить экземпляр команды по указанному типу события.
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public BaseAsteriskCommand GetCommand(StasisStartEventType eventType)
        {
            switch (eventType)
            {
                case StasisStartEventType.AcceptIncomingCall:
                    return GetCommand<AcceptedIncomingCallCommand>();
                case StasisStartEventType.Conference:
                    return GetCommand<ConferenceCommand>();
                case StasisStartEventType.Assistant:
                    return GetCommand<AssistantCommand>();
                case StasisStartEventType.PartialAssistant:
                    return GetCommand<PartialAssistantCommand>();
                case StasisStartEventType.AddToSnoopBridge:
                case StasisStartEventType.AddToSpeakSnoopBridge:
                    return GetCommand<AddToSnoopBridgeCommand>();
                case StasisStartEventType.RegisterIncomingCallCommand:
                    return GetCommand<RegisterIncomingCallCommand>();
                case StasisStartEventType.DeleteChannelCommand:
                    return GetCommand<DeleteChannelCommand>();
                case StasisStartEventType.ForceHangUpCommand:
                    return GetCommand<ForceHangUpCommand>();
                case StasisStartEventType.SwitchRolesCommand:
                    return GetCommand<SwitchRolesCommand>();
                case StasisStartEventType.IsolationCommand:
                    return GetCommand<IsolationCommand>();
                case StasisStartEventType.MuteCommand:
                    return GetCommand<MuteCommand>();
                case StasisStartEventType.CallFromUser:
                    return GetCommand<CallFromUserCommand>();
                case StasisStartEventType.CallToDestination:
                    return GetCommand<AcceptedCallFromUserCommand>();
                case StasisStartEventType.RejectedCallFromUser:
                    return GetCommand<RejectedCallFromUserCommand>();
                case StasisStartEventType.RecordingStarted:
                    return GetCommand<RecordingStartedCommand>();
                case StasisStartEventType.RecordingEnded:
                    return GetCommand<RecordingEndedCommand>();
                default:
                    throw new NotImplementedException("Команда с указанным типом не добавлена в AsteriskCommandServiceExtension.");
            }
        }

        /// <summary>
        /// Получить экземпляр команды по указанному типу.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCommand<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
