using System;
using System.Reflection;
using System.Threading;
using App.Metrics;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Services.Upstream;

namespace Switchboard
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseCors(builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            // TODO: CORS configuration

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
            });

            app.UseMetricsAllMiddleware();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/api/v1/lambda/socket", async context =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var scope = app.ApplicationServices.CreateScope();
                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await scope.ServiceProvider.GetService<WebSocketController>().Accept(socket,
                            CancellationToken.None);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                });
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();
            services.AddLogging(builder => builder.AddConsole());
            services.AddMetrics(CreateMetrics(_configuration));
            services.AddMetricsReportingHostedService();
            services.AddMetricsTrackingMiddleware();
            services.AddMvc();

            RegisterComponents(services);
            RegisterConfigurations(_configuration, services);
        }

        private static IMetricsRoot CreateMetrics(IConfiguration configuration)
        {
            var config = new MetricsConfiguration();
            configuration.GetSection("metrics").Bind(config);
            return AppMetrics.CreateDefaultBuilder().Configuration.Configure(options =>
                {
                    options.DefaultContextLabel = "KirisameSwitchboard";
                    options.Enabled = true;
                    options.ReportingEnabled = true;
                })
                .Report.ToConsole(TimeSpan.FromSeconds(60))
                .Report.ToInfluxDb(options =>
                {
                    options.FlushInterval = TimeSpan.FromSeconds(config.FlushInterval);
                    options.InfluxDb.BaseUri = new Uri(config.InfluxDb.BaseUri);
                    options.InfluxDb.Database = config.InfluxDb.Database;
                    options.InfluxDb.Password = config.InfluxDb.Password;
                    options.InfluxDb.UserName = config.InfluxDb.Username;
                })
                .Build();
        }

        private static void RegisterConfigurations(IConfiguration config, IServiceCollection services)
        {
            var upstream = new UpstreamServiceConfiguration();
            config.GetSection("upstream").Bind(upstream);
            services.AddSingleton(upstream);

            var websocket = new WebSocketSessionConfiguration();
            config.GetSection("websocket").Bind(websocket);
            services.AddSingleton(websocket);
        }

        private static void AddComponent(Type type, ComponentAttribute attribute, IServiceCollection services)
        {
            var singletons = type.GetCustomAttributes<DependsSingletonAttribute>();
            foreach (var dependency in singletons)
                services.AddSingleton(dependency.Type);

            var serviceType = attribute.Implements ?? type;

            switch (attribute.Lifestyle)
            {
                case ComponentLifestyle.Singleton:
                    services.AddSingleton(serviceType, type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute.Lifestyle), attribute.Lifestyle, null);
            }
        }

        private void RegisterComponents(IServiceCollection services)
        {
            var types = GetType().Assembly.DefinedTypes;
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ComponentAttribute>();
                if (attribute != null)
                    AddComponent(type, attribute, services);
            }
        }
    }
}