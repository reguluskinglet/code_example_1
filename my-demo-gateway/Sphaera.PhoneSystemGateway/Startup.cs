using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using demo.Http;
using demo.Http.Configuration;
using demo.Monitoring;
using demo.Monitoring.Common;
using demo.Monitoring.Logger.Filters;
using demo.Monitoring.Logger.Options;
using demo.Monitoring.Tracer.Config;
using demo.DemoGateway.DAL;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.Infrastructure.Commands.Factory;
using demo.DemoGateway.Infrastructure.HostedServices.Asterisk;
using demo.DemoGateway.Infrastructure.Options;
using demo.DemoGateway.Infrastructure.Parsers.Sms;
using demo.Transit;
using demo.Transit.Settings;

namespace demo.DemoGateway
{
    /// <summary>
    /// Запуск
    /// </summary>
    public class Startup
    {
        private static readonly string Version =
            typeof(Startup).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        private static readonly string ServiceName = "demo-gateway";


        /// <summary>
        /// Создать
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Конфигурация
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            MonitoringContext.ServiceName = ServiceName;
            services.Configure<LoggerOptions>(Configuration.GetSection(nameof(LoggerOptions)));
            services.Configure<JaegerTracingOptions>(Configuration.GetSection(nameof(JaegerTracingOptions)));
            services.Configure<AsteriskOptions>(Configuration.GetSection(nameof(AsteriskOptions)));
            services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

            services.AddDataAccessLayer();

            services.AddAutoMapper(typeof(Startup).Assembly);

            services.AddAsteriskCommands();

            services.AddTransient<AsteriskAriApiService>();
            services.AddTransient<AsteriskAriSmsService>();
            services.AddTransient<SmsParser>();
            services.AddTransient<SmsFieldParser>();
            services.AddSingleton<AsteriskAriWebSocketService>();

            var connectionString = Configuration.GetConnectionString("DemoGatewayDatabaseConnection");
            services.AddFluentMigratorCore()
                .ConfigureRunner(x => x
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(IChannelRepository).Assembly)
                    .For
                    .Migrations())
                .AddLogging(x => x.AddFluentMigratorConsole());

            services.AddMonitoring().AddDomainExceptionHandler();

            services.AddHttpClient();

            var transitSettings = Configuration.GetSection("TransitSettings").Get<TransitSettings>();
            services.AddTransit(transitSettings);

            services.AddMvc(options =>
                {
                    options.Filters.Add<MonitoringContextLoggingFilter>();
                    options.Filters.Add<ActionMethodParametersLoggingFilter>();
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var pathToDoc = Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Namespace}.xml");
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = Version,
                        Title = ServiceName,
                        Description = $"Сервис {ServiceName} для работы с системой телефонии.",
                        TermsOfService = null,
                        Contact = new OpenApiContact {Name = "АО «СФЕРА»", Email = "info@demo.ru"},
                        License = new OpenApiLicense
                            {Name = "GNU GPL v3.0", Url = new Uri("https://choosealicense.com/licenses/gpl-3.0/")}
                    });
                x.IncludeXmlComments(pathToDoc);
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.ConfigureExceptionHandler();

            app.UseMonitoring(ServiceName);
            app.UseOperationReceiverMiddleware();

            app.UseSwagger(x => x.RouteTemplate = "help/{documentName}/swagger.json");
            app.UseSwaggerUI(x =>
            {
                x.RoutePrefix = "help";
                x.SwaggerEndpoint("v1/swagger.json", $"{ServiceName} API {Version}");
            });

            app.UseMvc();
        }
    }
}