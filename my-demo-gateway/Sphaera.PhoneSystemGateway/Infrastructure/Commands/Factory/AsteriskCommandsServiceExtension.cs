using Microsoft.Extensions.DependencyInjection;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;

namespace demo.DemoGateway.Infrastructure.Commands.Factory
{
    /// <summary>
    /// Расширение .NET Core конфигурации для списка команд Asterisk ARI.
    /// </summary>
    public static class AsteriskCommandsServiceExtension
    {
        /// <summary>
        /// Расширение для включения дополнительных команд Asterisk ARI.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static void AddAsteriskCommands(this IServiceCollection services)
        {
            services.AddSingleton(provider => new CommandFactory(provider));
            services.AddSingleton<AsteriskAriClient>();

            // Список команд
            services.AddSingleton<RegisterIncomingCallCommand>();
            services.AddSingleton<AcceptedIncomingCallCommand>();
            services.AddSingleton<ConferenceCommand>();
            services.AddSingleton<AssistantCommand>();
            services.AddSingleton<PartialAssistantCommand>();
            services.AddSingleton<AddToSnoopBridgeCommand>();
            services.AddSingleton<IsolationCommand>();
            services.AddSingleton<SwitchRolesCommand>();
            services.AddSingleton<DeleteChannelCommand>();
            services.AddSingleton<ForceHangUpCommand>();
            services.AddSingleton<MuteCommand>();
            services.AddSingleton<CallFromUserCommand>();
            services.AddSingleton<AcceptedCallFromUserCommand>();
            services.AddSingleton<RejectedCallFromUserCommand>();
            services.AddSingleton<RecordingStartedCommand>();
            services.AddSingleton<RecordingEndedCommand>();
        }
    }
}