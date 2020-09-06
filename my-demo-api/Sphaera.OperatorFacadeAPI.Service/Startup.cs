using System;
using System.Collections.Generic;
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
using demo.ContactManagement.Client;
using demo.ContactManagement.Client.Options;
using Microsoft.OpenApi.Models;
using demo.Authorization.Configuration;
using demo.CallManagement.Client;
using demo.CallManagement.Client.Options;
using demo.DDD;
using demo.GisFacade.Client;
using demo.GisFacade.Client.Options;
using demo.DDD.Configuration;
using demo.Http;
using demo.IndexService.Client;
using demo.IndexService.Client.Options;
using demo.Http.Configuration;
using demo.InboxDistribution.Client;
using demo.InboxDistribution.Client.Options;
using demo.MediaRecording.Client;
using demo.MediaRecording.Client.Options;
using demo.Monitoring;
using demo.Monitoring.Common;
using demo.Monitoring.Logger.Filters;
using demo.Monitoring.Logger.Options;
using demo.Monitoring.Tracer.Config;
using demo.DemoApi.DAL;
using demo.DemoApi.DAL.Abstractions;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Configuration;
using demo.DemoApi.Service.Hubs;
using demo.DemoApi.Service.Infrastructure.Logging;
using demo.DemoApi.Service.Infrastructure.Options;
using demo.DemoApi.Service.IntegrationEventHandlings;
using demo.DemoApi.Service.Middlewares;
using demo.DemoApi.Service.OperationFilters;
using demo.Transit;
using demo.Transit.Settings;
using demo.UserManagement.Client;
using demo.UserManagement.Client.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace demo.DemoApi.Service
{
    /// <summary>
    /// Запуск
    /// </summary>
    public class Startup
    {
        private static readonly string Version = typeof(Startup).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        private const string ServiceName = "demo-api-service";

        /// <summary>
        /// Создать класс Запуск
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
            services.Configure<CallManagementServiceOptions>(Configuration.GetSection("ServicesOptions:CallManagementServiceClientOptions"));
            services.Configure<UserManagementServiceOptions>(Configuration.GetSection("ServicesOptions:UserManagementServiceOptions"));
            services.Configure<RegulatoryTimeZones>(Configuration.GetSection(nameof(RegulatoryTimeZones)));
            services.Configure<IndexServiceOptions>(Configuration.GetSection("ServicesOptions:IndexServiceOptions"));
            services.Configure<ContactManagementServiceOptions>(Configuration.GetSection("ServicesOptions:ContactManagementServiceOptions"));
            services.Configure<GisFacadeClientOptions>(Configuration.GetSection("ServicesOptions:GisFacadeClientOptions"));
            services.Configure<InboxDistributionServiceOptions>(Configuration.GetSection("ServicesOptions:InboxDistributionServiceOptions"));
            services.Configure<MediaRecordingServiceOptions>(Configuration.GetSection("ServicesOptions:MediaRecordingServiceOptions"));
            services.Configure<RouteOptions>(x => x.LowercaseUrls = true);

            var userManagementServiceOptions = Configuration.GetSection("ServicesOptions:UserManagementServiceOptions").Get<UserManagementServiceOptions>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = userManagementServiceOptions.Url;
                    options.RequireHttpsMetadata = false;
                });

            services.AddDataAccessLayer();
            services.AddApplicationLayer();

            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            services.AddCors();

            var connectionString = Configuration.GetConnectionString("DatabaseConnection");

            var assembly = Assembly.GetAssembly(typeof(IVersionInfoRepository));
            services.AddSingleton(_ => new SessionFactory(connectionString, assembly));

            services.AddTransient<TrackingEventHandler>();
            services.AddUserAuthorization();
            services.AddTrackingEvents();

            services.AddScoped<IWebClientLoggingService, WebClientLoggingService>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<GisFacadeClient>();
            services.AddScoped<IndexServiceClient>();
            services.AddScoped<ContactManagementServiceClient>();
            services.AddScoped<CallManagementServiceClient>();
            services.AddScoped<InboxDistributionServiceClient>();
            services.AddScoped<UserManagementServiceClient>();
            services.AddScoped<MediaRecordingServiceClient>();

            services.AddFluentMigratorCore()
                .ConfigureRunner(x => x
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(IVersionInfoRepository).Assembly)
                    .For
                    .Migrations())
                .AddLogging(x => x.AddFluentMigratorConsole());

            services.AddMonitoring().AddDomainExceptionHandler();

            services.AddHttpClient();

            var transitSettings = Configuration.GetSection("TransitSettings").Get<TransitSettings>();
            services.AddTransit(transitSettings,
                new EndPointConsumerConfigurator()
                    .Add<IncomingInboxIntegrationEventHandler>()
                    .Add<EndCallIntegrationEventHandler>()
                    .Add<RejectCallIntegrationEventHandler>()
                    .Add<AcceptCallFromUserIntegrationEventHandler>()
                    .Add<EntityChangedIntegrationEventHandler>()
                    .Add<UpdateInboxIntegrationEventHandler>()
            );
            services.AddMvc(options =>
                {
                    options.Filters.Add<MonitoringContextLoggingFilter>();
                    options.Filters.Add<ActionMethodParametersLoggingFilter>();
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(
                        new Newtonsoft.Json.Converters.StringEnumConverter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var redisOptions = Configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>();

            services.AddSignalR().AddRedisBackPlane(redisOptions);

            services.AddStackExchangeRedisCache(options => { options.Configuration = redisOptions.Configuration; });

            AddSwagger(services);
        }

        private static void AddSwagger(IServiceCollection services)
        {
            var pathToDoc = Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Namespace}.xml");
            
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Version = Version,
                        Title = ServiceName,
                        Description = $"Прототип сервиса {ServiceName}.",
                        TermsOfService = null,
                        Contact = new OpenApiContact { Name = "АО «СФЕРА»", Email = "info@demo.ru" },
                        License = new OpenApiLicense { Name = "GNU GPL v3.0", Url = new Uri("https://choosealicense.com/licenses/gpl-3.0/") }
                    });

                AddSwaggerAuthorization(x);

                x.IncludeXmlComments(pathToDoc);
                x.DescribeAllEnumsAsStrings();
            });
        }

        private static void AddSwaggerAuthorization(SwaggerGenOptions x)
        {
            x.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

            x.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.ConfigureExceptionHandler();
            app.UseMonitoring(ServiceName);
            app.UseOperationReceiverMiddleware();
            
            app.UseAuthentication();
            app.UseAuthorizationMiddleware();
            
            app.UseSignalR(routes =>
            {
                routes.MapHub<PhoneHub>("/phoneHub");
                routes.MapHub<GisHub>("/gisHub");
            });

            app.UseSwagger(x => x.RouteTemplate = "help/{documentName}/swagger.json");
            app.UseSwaggerUI(x =>
            {
                x.RoutePrefix = "help";
                x.SwaggerEndpoint("v1/swagger.json", $"{ServiceName} API {Version}");
            });

            app.AddTrackingEventHandler<TrackingEventHandler>();

            app.UseMiddleware<ETagMiddleware>();
            app.UseMvc();
        }
    }
}
